namespace GitWorktreeManager.Services;

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class GitException : Exception
{
    public string Command { get; init; }
    public int ExitCode { get; init; }
    public string Error { get; init; }

    public GitException(string message) : base(message) 
    {
    }
}

internal class GitApi
{
    private readonly string workingDir;

    public GitApi(string workingDir)
    {
        this.workingDir = workingDir;
    }

    private async Task<ImmutableList<string>> RunCommand(string command)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = "git",
            CreateNoWindow = true,
            WorkingDirectory = this.workingDir,
            Arguments = command
        });

        var output = await process!.StandardOutput.ReadToEndAsync();
        var lines = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToImmutableList();

        var error = await process!.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();
        if (process.ExitCode is not 0)
        {
            throw new GitException($"Git command terminated with error code: {process.ExitCode}")
            {
                Command = command,
                ExitCode = process.ExitCode,
                Error = error
            };
        }

        return lines;
    }

    /// <summary>
    /// <code>git branch --list {branch}</code>
    /// </summary>
    private async Task<bool> IsLocalBranchAvailable(string branch)
    {
        var result = await RunCommand($"branch --list \"{branch}\"");
        return result.IsEmpty is false;
    }

    /// <summary>
    /// <code>git branch --list -r {remote}/{branch}</code>
    /// </summary>
    private async Task<bool> IsKnownRemoteBranchAvailable(string branch)
    {
        var result = await RunCommand($"branch --list -r \"origin/{branch}\"");
        return result.IsEmpty is false;
    }

    /// <summary>
    /// <code>git ls-remote --heads origin {branch}</code>
    /// </summary>
    private async Task<bool> IsUnknownRemoteBranchAvailable(string branch)
    {
        var result = await RunCommand($"ls-remote --heads origin \"{branch}\"");
        return result.IsEmpty is false;
    }

    /// <summary>
    /// <code>git worktree list --porcelain</code>
    /// </summary>
    public async Task<ImmutableDictionary<string, string>> ListWorktrees()
    {
        var lines = await RunCommand("worktree list --porcelain");

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

    /// <summary>
    /// <code>git worktree add ...</code>
    /// </summary>
    public async Task AddWorktree(string branch)
    {
        var worktrees = await ListWorktrees();
        var worktreeExists = worktrees.ContainsKey(branch);
        if (worktreeExists)
        {
            return;
        }

        if (await IsLocalBranchAvailable(branch))
        {
            await AddWorktreeForLocalBranch(branch);
        }
        else if (await IsKnownRemoteBranchAvailable(branch))
        {
            await AddWorktreeForRemoteBranch(branch);
        }
        else if (await IsUnknownRemoteBranchAvailable(branch))
        {
            await Fetch();
            await AddWorktreeForRemoteBranch(branch);
        }
        else
        {
            await AddWorktreeForNewBranch(branch);
        }
    }

    /// <summary>
    /// <code>git worktree add {path} {branch}</code>
    /// </summary>
    private async Task AddWorktreeForLocalBranch(string branch)
    {
        var path = GetWorktreePath(branch);

        _ = await RunCommand($"worktree add \"{path}\" \"{branch}\"");
    }

    /// <summary>
    /// <code>git worktree add --track -b {branch} {path} {remote}/{branch}</code>
    /// </summary>
    private async Task AddWorktreeForRemoteBranch(string branch)
    {
        var path = GetWorktreePath(branch);
        var remote = $"origin/{branch}";

        _ = await RunCommand($"worktree add --track -b \"{branch}\" \"{path}\" \"{remote}\"");
    }

    /// <summary>
    /// <code>git worktree add -b {branch} {path}</code>
    /// </summary>
    private async Task AddWorktreeForNewBranch(string branch)
    {
        var path = GetWorktreePath(branch);

        _ = await RunCommand($"worktree add -b \"{branch}\" \"{path}\"");
    }

    /// <summary>
    /// <code>git worktree remove {branch}</code>
    /// </summary>
    public async Task RemoveWorktree(string branch)
    {
        _ = await RunCommand($"worktree remove \"{branch}\"");
    }

    /// <summary>
    /// <code>git fetch</code>
    /// </summary>
    private async Task Fetch()
    {
        _ = await RunCommand("fetch");
    }

    private string GetWorktreePath(string branch)
    {
        const string worktreesRoot = ".worktrees";

        var tempPath = Path.Combine(this.workingDir, worktreesRoot, branch);
        var path = Path.GetFullPath(tempPath);
        return path;
    }
}