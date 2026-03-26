namespace GitWorktreeManager.Services;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GitException : Exception
{
    public required string Command { get; init; }
    public required int ExitCode { get; init; }
    public required string Error { get; init; }

    public GitException(string message) : base(message)
    {
    }
}

internal sealed partial class GitApi
{
    private readonly Helpers helpers;

    public GitApi(string workingDir)
    {
        this.helpers = new Helpers
        {
            RootPath = workingDir,
            WorktreeRootRelativePath = ".worktrees"
        };
    }

    private async Task<string> RunCommand(string command, TimeSpan? timeout = null)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = "cmd",
            Arguments = $"/c {command}",
            WorkingDirectory = this.helpers.RootPath
        });

        try
        {
            using var cts = timeout.HasValue
                ? new CancellationTokenSource(timeout.Value)
                : new CancellationTokenSource();

            var output = await process!.StandardOutput.ReadToEndAsync(cts.Token);
            var error = await process!.StandardError.ReadToEndAsync(cts.Token);

            await process.WaitForExitAsync(cts.Token);
            if (process.ExitCode is not 0)
            {
                throw new GitException($"Git command terminated with error code: {process.ExitCode}")
                {
                    Command = command,
                    ExitCode = process.ExitCode,
                    Error = error
                };
            }

            return output;
        }
        catch (OperationCanceledException)
        {
            try { process!.Kill(entireProcessTree: true); } catch { }
            throw;
        }
    }

    private async Task RunCommandNoResponse(string command, TimeSpan? timeout = null)
        => _ = await RunCommand(command, timeout);

    private async Task<TResult> RunCommandProcessResponse<TResult>(string command, Func<string, TResult> resultProcessor)
    {
        var result = await RunCommand(command);
        return resultProcessor(result);
    }

    /// <summary>
    /// <code>git branch -a --format=%(refname)#%(symref)#%(upstream:track,nobracket)#%(worktreepath)</code>
    /// </summary>
    public async Task<ListBranchesResult> ListBranchesAsync()
    {
        return await RunCommandProcessResponse(
            helpers.ListBranches_CreateCommand(),
            helpers.ListBranches_ProcessResult);
    }

    /// <summary>
    /// <code>git worktree add {path} {branch}</code>
    /// </summary>
    public async Task AddWorktreeForLocalBranch(string branch)
    {
        await RunCommandNoResponse(helpers.AddWorktreeForLocalBranch_CreateCommand(branch));
    }

    /// <summary>
    /// <code>git worktree add --track -b {branch} {path} {remote}/{branch}</code>
    /// </summary>
    public async Task AddWorktreeForRemoteBranch(string branch)
    {
        await RunCommandNoResponse(helpers.AddWorktreeForRemoteBranch_CreateCommand(branch));
    }

    /// <summary>
    /// <code>git worktree add -b {newBranch} {path} {baseBranch}</code>
    /// </summary>
    public async Task AddWorktreeBasedOnLocalBranch(string newBranch, string baseBranch)
    {
        await RunCommandNoResponse(helpers.AddWorkTreeBasedOnLocalBranch_CreateCommand(newBranch, baseBranch));
    }

    /// <summary>
    /// <code>
    /// git worktree add -b {newBranch} {path} {remote}/{baseBranch}
    /// git branch --unset-upstream {newBranch}
    /// </code>
    /// </summary>
    public async Task AddWorktreeBasedOnRemoteBranch(string newBranch, string baseBranch)
    {
        await RunCommandNoResponse(helpers.AddWorkTreeBasedOnRemoteBranch_CreateCommand(newBranch, baseBranch));
        await RunCommandNoResponse(helpers.AddWorkTreeUnsetUpstream_CreateCommand(newBranch));
    }

    /// <summary>
    /// <code>git worktree remove {branch}</code>
    /// </summary>
    public async Task RemoveWorktree(string branch)
    {
        await RunCommandNoResponse(helpers.RemoveWorktree_CreateCommand(branch));
    }

    /// <summary>
    /// <code>git fetch</code>
    /// </summary>
    public async Task Fetch(TimeSpan? timeout = null)
    {
        await RunCommandNoResponse(helpers.Fetch_CreateCommand(), timeout);
    }
}