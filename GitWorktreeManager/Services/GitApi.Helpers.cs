namespace GitWorktreeManager.Services;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

internal sealed class ListBranchResult
{
    public required BranchWithWorktree LocalHead { get; init; }
    public required ImmutableList<Branch> LocalBranches { get; init; }
    public required ImmutableList<Branch> RemoteBranches { get; init; }
}

internal class Branch
{
    public required string Name { get; init; }
    public required uint Ahead { get; init; }
    public required uint Behind { get; init; }
}

internal sealed class BranchWithWorktree : Branch
{
    public required string WorktreePath { get; init; }
}

internal sealed partial class GitApi
{
    public class Helpers
    {
        public required string RootPath { get; init; }
        public required string WorktreeRootRelativePath { get; init; }

        public string ListBranches_CreateCommand()
            => "git branch -a --format=%(refname)#%(symref)#%(upstream:track,nobracket)#%(worktreepath)";

        public ListBranchResult ListBranches_ProcessResult(string result)
        {
            const string LocalPrefix = "refs/heads/";
            const string RemotesPrefix = "refs/remotes/origin/";
            const string OriginHead = "refs/remotes/origin/HEAD";

            var branches = result
                .ReadLines()
                .Select<string, (string refName, string? symRef, (uint ahead, uint behind) upstreamTrack, string? worktreePath)?>(l =>
                {
                    var regex = new Regex("^(?<refname>[^#]+)#(?<symref>[^#]*)#(?:(?:ahead (?<ahead>\\d+))(?:, )?)?(?:(?:behind (?<behind>\\d+)))?#(?<worktreepath>[^#]*)$");

                    if (regex.Match(l) is Match match)
                    {
                        var refName = match.Groups["refname"].Value;
                        var symRef = match.Groups.TryGetValue("symref", out var symRefGroup) && symRefGroup.Success ? symRefGroup.Value : null;
                        var worktreePath = match.Groups.TryGetValue("worktreepath", out var worktreePathGroup) && worktreePathGroup.Success ? worktreePathGroup.Value : null;
                        var ahead = match.Groups.TryGetValue("ahead", out var aheadGroup) && aheadGroup.Success ? uint.Parse(aheadGroup.Value) : 0;
                        var behind = match.Groups.TryGetValue("behind", out var behindGroup) && behindGroup.Success ? uint.Parse(behindGroup.Value) : 0;

                        return (refName, symRef, (ahead, behind), worktreePath);
                    }

                    return null;
                })
                .Where(x => x is not null)
                .Select(x => x!.Value)
                .ToImmutableList();

            var originHeadBranchName = branches
                .Where(b => b.refName.Equals(OriginHead, StringComparison.InvariantCultureIgnoreCase) is true)
                .Select(b => b.symRef![RemotesPrefix.Length..])
                .First();

            var originHeadBranch = branches
                .Where(b => b.refName.Equals(OriginHead, StringComparison.InvariantCultureIgnoreCase) is false)
                .Where(b => b.refName.Equals(LocalPrefix + originHeadBranchName, StringComparison.InvariantCultureIgnoreCase))
                .Select(b => new BranchWithWorktree
                {
                    Name = b.refName[LocalPrefix.Length..],
                    WorktreePath = b.worktreePath!,
                    Ahead = b.upstreamTrack.ahead,
                    Behind = b.upstreamTrack.behind
                })
                .First();

            var localBranches = branches
                .Where(b => b.refName.Equals(OriginHead, StringComparison.InvariantCultureIgnoreCase) is false)
                .Where(b => b.refName.StartsWith(LocalPrefix, StringComparison.InvariantCultureIgnoreCase))
                .Select(b => !string.IsNullOrEmpty(b.worktreePath)
                    ? new BranchWithWorktree
                    {
                        Name = b.refName[LocalPrefix.Length..],
                        WorktreePath = b.worktreePath,
                        Ahead = b.upstreamTrack.ahead,
                        Behind = b.upstreamTrack.behind
                    }
                    : new Branch
                    {
                        Name = b.refName[LocalPrefix.Length..],
                        Ahead = b.upstreamTrack.ahead,
                        Behind = b.upstreamTrack.behind
                    })
                .Where(b => b.Name != originHeadBranchName)
                .ToImmutableList();

            var localBranchNames = localBranches.Select(b => b.Name).ToImmutableHashSet();
            var remoteBranches = branches
                .Where(b => b.refName.Equals(OriginHead, StringComparison.InvariantCultureIgnoreCase) is false)
                .Where(b => b.refName.StartsWith(RemotesPrefix, StringComparison.InvariantCultureIgnoreCase))
                .Select(b => new Branch
                {
                    Name = b.refName[RemotesPrefix.Length..],
                    Ahead = b.upstreamTrack.ahead,
                    Behind = b.upstreamTrack.behind
                })
                .Where(b => b.Name != originHeadBranchName)
                .Where(b => localBranchNames.Contains(b.Name) is false)
                .ToImmutableList();

            return new ListBranchResult
            {
                LocalHead = originHeadBranch,
                LocalBranches = localBranches,
                RemoteBranches = remoteBranches
            };
        }

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
