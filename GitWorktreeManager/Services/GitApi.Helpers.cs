namespace GitWorktreeManager.Services;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

internal sealed class ListBranchResult
{
    public required string LocalHead { get; init; }
    public required ImmutableList<string> LocalBranches { get; init; }
    public required ImmutableList<string> RemoteBranches { get; init; }
}

internal sealed class Worktree
{
    public required string Branch { get; init; }
    public required string Path { get; init; }
}

internal sealed partial class GitApi
{
    public class Helpers
    {
        public required string RootPath { get; init; }
        public required string WorktreeRootRelativePath { get; init;  }

        public string ListBranches_CreateCommand()
            => "git branch --list -a";

        public ListBranchResult ListBranches_ProcessResult(string result)
        {
            const string RemotesPrefix = "remotes/";
            const string OriginPrefix = "origin/";
            const string OriginHead = "origin/HEAD";

            var trimmedLines = result
                .ReadLines()
                .Select(line => line[2..]) // Trim first 2 characters
                .Select(line => line.Replace(RemotesPrefix, string.Empty)) // Remove "remotes/" prefix
                .ToImmutableList();

            var originHeadLine = trimmedLines
                .First(line => line.StartsWith(OriginHead));

            var originHeadBranch = originHeadLine.Split("->", StringSplitOptions.TrimEntries)[1];
            var localHeadBranch = originHeadBranch[OriginPrefix.Length..];

            var localBranches = trimmedLines
                .Where(line => line.StartsWith(OriginPrefix) is false)
                .Where(line => line != localHeadBranch)
                .ToImmutableList();

            var remoteBranches = trimmedLines
                .Where(line => line.StartsWith(OriginPrefix) is true)
                .Where(line => line.StartsWith(OriginHead) is false) // Filter out special HEAD line
                .Where(line => line != originHeadBranch)
                .Select(line => line[OriginPrefix.Length..]) // Remove "origin/" prefix
                .Where(line => !localBranches.Contains(line))
                .ToImmutableList();

            return new()
            {
                LocalHead = localHeadBranch,
                LocalBranches = localBranches,
                RemoteBranches = remoteBranches
            };
        }

        public string ListWorktrees_CreateCommand()
            => "git worktree list --porcelain";

        public ImmutableList<Worktree> ListWorktrees_ProcessResult(string result)
            => result
            .ReadLines()
            .Chunk(3)
            .Select(x =>
            {
                var path = x[0]["worktree ".Length..];
                var branch = x[2]["branch refs/heads/".Length..];

                return new Worktree { Branch = branch, Path = path };
            })
            .ToImmutableList();

        public string AddWorktreeForLocalBranch_CreateCommand(string branch)
        {
            var path = CreateWorktreePath(branch);
            return $"git worktree add \"{path}\" \"{branch}\"";
        }

        public string AddWorktreeForRemoteBranch_CreateCommand(string branch)
        {
            var path = CreateWorktreePath(branch);
            var remote = $"origin/{branch}";
            return $"git worktree add --track -b \"{branch}\" \"{path}\" \"{remote}\"";
        }

        public string AddWorkTreeBasedOnLocalBranch_CreateCommand(string newBranch, string baseBranch)
        {
            var path = CreateWorktreePath(newBranch);
            return $"git worktree add -b \"{newBranch}\" \"{path}\" \"{baseBranch}\"";
        }

        public string AddWorkTreeBasedOnRemoteBranch_CreateCommand(string newBranch, string baseBranch)
        {
            var path = CreateWorktreePath(newBranch);
            return $"git worktree add -b \"{newBranch}\" \"{path}\" origin/\"{baseBranch}\"";
        }

        public string AddWorkTreeUnsetUpstream_CreateCommand(string branch) 
            => $"git branch --unset-upstream \"{branch}\"";

        public string RemoveWorktree_CreateCommand(string branch)
            => $"git worktree remove \"{branch}\"";

        public string Fetch_CreateCommand()
            => "git fetch";

        private string CreateWorktreePath(string branch)
        {
            var tempPath = Path.Combine(this.RootPath, this.WorktreeRootRelativePath, branch);
            var path = Path.GetFullPath(tempPath);
            return path;
        }
    }
}
