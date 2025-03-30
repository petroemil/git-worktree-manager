namespace GitWorktreeManager.ViewModel;

using GitWorktreeManager.Services;
using GitWorktreeManager.Services.Abstractions;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

internal sealed partial class RepoViewModel
{
    public static class Helpers
    {
        public static ImmutableList<BranchViewModel>? FilterBranches(ImmutableList<BranchViewModel>? branches, string query)
        {
            return branches?
                .Where(branch => string.IsNullOrWhiteSpace(query) || branch.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
                .Take(50)
                .ToImmutableList();
        }

        public static ImmutableList<BranchViewModel> CreateBranchVms(ListBranchesResult branches, RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
        {
            var localHeadVm = new HeadBranchWithWorktreeViewModel(repoVm, repoService, dialogService)
            {
                Name = branches.LocalHead.Name,
                Path = branches.LocalHead.WorktreePath,
                Ahead = branches.LocalHead.Ahead,
                Behind = branches.LocalHead.Behind
            };

            // Local branches with worktree
            var worktreeVms = branches.LocalBranches
                .Select(branch => branch as Services.BranchWithWorktree)
                .Where(branch => branch is not null)
                .Select(branch => branch!)
                .Select(branch => new LocalBranchWithWorktreeViewModel(repoVm, repoService, dialogService)
                {
                    Name = branch.Name,
                    Path = branch.WorktreePath,
                    Ahead = branch.Ahead,
                    Behind = branch.Behind
                });

            // Local branches without worktree
            var localBranchVms = branches.LocalBranches
                .Where(branch => branch is not Services.BranchWithWorktree)
                .Select(branch => new LocalBranchWithoutWorktreeViewModel(repoVm, repoService, dialogService)
                {
                    Name = branch.Name,
                    Ahead = branch.Ahead,
                    Behind = branch.Behind
                });

            // Remote branches
            var remoteBranchVms = branches.RemoteBranches
                .Select(branch => new RemoteBranchWithoutWorktreeViewModel(repoVm, repoService, dialogService)
                {
                    Name = branch.Name,
                    Ahead = branch.Ahead,
                    Behind = branch.Behind
                });

            return Enumerable.Empty<BranchViewModel>()
                .Append(localHeadVm)
                .Concat(worktreeVms)
                .Concat(localBranchVms)
                .Concat(remoteBranchVms)
                .ToImmutableList();
        }

        public static string GetFolderPathForBranch(BranchWithWorktreeViewModel branch)
        {
            return Path.GetFullPath(branch.Path);
        }
    }
}