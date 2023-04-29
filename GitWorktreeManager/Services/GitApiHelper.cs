using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace GitWorktreeManager.Services
{
    public sealed class ListBranchResult
    {
        public required string LocalHead { get; init; }
        public required ImmutableList<string> LocalBranches { get; init; }
        public required string RemoteHead { get; init; }
        public required ImmutableList<string> RemoteBranches { get; init; }
    }

    internal class GitApiHelper
    {
        public string RootPath { get; }
        public string WorktreeRootRelativePath { get; }

        public GitApiHelper(string rootPath, string worktreeRootRelativePath)
        {
            this.RootPath = rootPath;
            this.WorktreeRootRelativePath = worktreeRootRelativePath;
        }

        public string ListBranches_CreateCommand()
            => "branch --list -a";

        public ListBranchResult ListBranches_ProcessResult(ImmutableList<string> result)
        {
            const string RemotesPrefix = "remotes/";
            const string OriginPrefix = "origin/";
            const string OriginHead = "origin/HEAD";

            var trimmedLines = result
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
                .Where(line => line.StartsWith(OriginHead) is false) // filter out special HEAD line
                .Where(line => line != originHeadBranch)
                .ToImmutableList();

            return new()
            {
                LocalHead = localHeadBranch,
                RemoteHead = originHeadBranch,
                LocalBranches = localBranches,
                RemoteBranches = remoteBranches
            };
        }

        public string ListWorktrees_CreateCommand()
            => "worktree list --porcelain";

        public ImmutableDictionary<string, string> ListWorktrees_ProcessResult(ImmutableList<string> result)
            => result
            .Chunk(3)
            .Select(x =>
            {
                var path = x[0]["worktree ".Length..];
                var branch = x[2]["branch refs/heads/".Length..];

                return (branch, path);
            })
            .ToImmutableDictionary(x => x.branch, x => x.path);

        public string AddWorktreeForLocalBranch_CreateCommand(string branch)
        {
            var path = CreateWorktreePath(branch);
            return $"worktree add \"{path}\" \"{branch}\"";
        }

        public string AddWorktreeForRemoteBranch_CreateCommand(string branch)
        {
            var path = CreateWorktreePath(branch);
            var remote = $"origin/{branch}";
            return $"worktree add --track -b \"{branch}\" \"{path}\" \"{remote}\"";
        }

        public string AddWorkTree_CreateCommand(string newBranch, string baseBranch)
        {
            var path = CreateWorktreePath(newBranch);
            return $"worktree add -b \"{newBranch}\" \"{path}\" \"{baseBranch}\"";
        }

        public string RemoveWorktree_CreateCommand(string branch)
            => $"worktree remove \"{branch}\"";

        public string Fetch_CreateCommand()
            => "fetch";

        private string CreateWorktreePath(string branch)
        {
            var tempPath = Path.Combine(this.RootPath, this.WorktreeRootRelativePath, branch);
            var path = Path.GetFullPath(tempPath);
            return path;
        }
    }
}
