namespace GitWorktreeManager.Services;

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

internal class GitApi
{
    private readonly string workingDir;

    public GitApi(string workingDir)
    {
        this.workingDir = workingDir;
    }

    private static async Task<ImmutableList<string>> RunGitCommand(string workingDir, string command)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = "git",
            CreateNoWindow = true,
            WorkingDirectory = workingDir,
            Arguments = command
        });

        var output = await process!.StandardOutput.ReadToEndAsync();
        var lines = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToImmutableList();

        await process.WaitForExitAsync();
        if (process.ExitCode is not 0)
        {
            throw new Exception($"Git command terminated with error code: {process.ExitCode}");
        }

        return lines;
    }

    private async Task<bool> IsLocalBranchAvailable(string branch)
    {
        // branch --list <branch>
        var result = await RunGitCommand(workingDir, $"branch --list \"{branch}\"");
        return result.IsEmpty is false;
    }

    private async Task<bool> IsKnownRemoteBranchAvailable(string branch)
    {
        // branch --list -r <remote>/<branch>
        var result = await RunGitCommand(workingDir, $"branch --list -r \"origin/{branch}\"");
        return result.IsEmpty is false;
    }

    private async Task<bool> IsUnknownRemoteBranchAvailable(string branch)
    {
        // ls-remote --heads origin <branch>
        var result = await RunGitCommand(workingDir, $"ls-remote --heads origin \"{branch}\"");
        return result.IsEmpty is false;
    }

    private async Task<bool> IsRemoteBranchAvailable(string branch)
    {
        return await IsKnownRemoteBranchAvailable(branch)
            || await IsUnknownRemoteBranchAvailable(branch);
    }

    public async Task<ImmutableDictionary<string, string>> ListWorktrees()
    {
        var lines = await RunGitCommand(workingDir, "worktree list --porcelain");

        // Output format:
        // --------------------------
        // worktree <path>
        // HEAD <commit-id>
        // branch refs/heads/<branch>
        var worktrees = lines
            .Chunk(3)
            .Skip(1)
            .Select(x =>
            {
                var path = x[0]["worktree ".Length..];
                path = Path.GetFullPath(path);

                var branch = x[2]["branch refs/heads/".Length..];

                return (branch, path);
            })
            .ToImmutableDictionary(x => x.branch, x => x.path);

        return worktrees;
    }

    public async Task AddWorktree(string branch)
    {
        var worktrees = await ListWorktrees();
        var worktreeExists = worktrees.ContainsKey(branch);
        if (worktreeExists)
        {
            return;
        }

        var localBranchExists = await IsLocalBranchAvailable(branch);
        if (localBranchExists)
        {
            await AddWorktreeForLocalBranch(branch);
            return;
        }

        var remoteBranchExists = await IsRemoteBranchAvailable(branch);
        if (remoteBranchExists)
        {
            await AddWorktreeForRemoteBranch(branch);
        }
        else
        {
            await AddWorktreeForNewBranch(branch);
        }
    }

    private async Task AddWorktreeForLocalBranch(string branch)
    {
        var path = GetWorktreePath(branch);

        // git worktree add <path> <branch>
        _ = await RunGitCommand(workingDir, $"worktree add \"{path}\" \"{branch}\"");
    }

    private async Task AddWorktreeForRemoteBranch(string branch)
    {
        var path = GetWorktreePath(branch);
        var remote = $"origin/{branch}";

        // git worktree add --track -b <branch> <path> <remote>/<branch>
        _ = await RunGitCommand(workingDir, $"worktree add --track -b \"{branch}\" \"{path}\" \"{remote}\"");
    }

    private async Task AddWorktreeForNewBranch(string branch)
    {
        var path = GetWorktreePath(branch);

        // git worktree add -b <branch> <path>
        _ = await RunGitCommand(workingDir, $"worktree add -b \"{branch}\" \"{path}\"");
    }

    public async Task RemoveWorktree(string branch)
    {
        // git worktree remove <branch>
        _ = await RunGitCommand(workingDir, $"worktree remove \"{branch}\"");
    }

    public string GetWorktreePath(string branch)
    {
        const string worktreesRoot = ".worktrees";

        var tempPath = Path.Combine(this.workingDir, worktreesRoot, branch);
        var path = Path.GetFullPath(tempPath);
        return path;
    }
}