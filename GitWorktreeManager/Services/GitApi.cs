namespace GitWorktreeManager.Services;

using System;
using System.Collections.Immutable;
using System.Diagnostics;
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
    private readonly GitApiHelper helper;

    public GitApi(string workingDir)
    {
        this.helper = new GitApiHelper(workingDir, ".worktrees");
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
            WorkingDirectory = this.helper.RootPath,
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

    private async Task RunCommandNoResponse(string command)
        => _ = await RunCommand(command);

    private async Task<TResult> RunCommandProcessResponse<TResult>(string command, Func<ImmutableList<string>, TResult> resultProcessor)
    {
        var result = await RunCommand(command);
        return resultProcessor(result);
    }

    public async Task<ListBranchResult> ListBranchesAsync()
    {
        return await RunCommandProcessResponse<ListBranchResult>(
            helper.ListBranches_CreateCommand(),
            helper.ListBranches_ProcessResult);
    }

    /// <summary>
    /// <code>git worktree list</code>
    /// </summary>
    public async Task<ImmutableDictionary<string, string>> ListWorktrees()
    {
        return await RunCommandProcessResponse(
            helper.ListWorktrees_CreateCommand(),
            helper.ListWorktrees_ProcessResult);
    }

    /// <summary>
    /// <code>git worktree add {path} {branch}</code>
    /// </summary>
    public async Task AddWorktreeForLocalBranch(string branch)
    {
        await RunCommandNoResponse(helper.AddWorktreeForLocalBranch_CreateCommand(branch));
    }

    /// <summary>
    /// <code>git worktree add --track -b {branch} {path} {remote}/{branch}</code>
    /// </summary>
    public async Task AddWorktreeForRemoteBranch(string branch)
    {
        await RunCommandNoResponse(helper.AddWorktreeForRemoteBranch_CreateCommand(branch));
    }

    /// <summary>
    /// <code>git worktree add -b {branch} {path}</code>
    /// </summary>
    public async Task AddWorktreeForNewBranch(string branch, string baseBranch)
    {
        await RunCommandNoResponse(helper.AddWorkTree_CreateCommand(branch, baseBranch));
    }

    /// <summary>
    /// <code>git worktree remove {branch}</code>
    /// </summary>
    public async Task RemoveWorktree(string branch)
    {
        await RunCommandNoResponse(helper.RemoveWorktree_CreateCommand(branch));
    }

    /// <summary>
    /// <code>git fetch</code>
    /// </summary>
    public async Task Fetch()
    {
        await RunCommandNoResponse(helper.Fetch_CreateCommand());
    }
}