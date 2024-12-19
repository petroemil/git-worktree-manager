namespace GitWorktreeManager.ViewModel;

using GitWorktreeManager.Services;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public static ImmutableList<BranchViewModel> CreateBranchVms(
            ListBranchesResult branches,
            Func<BranchWithoutWorktreeViewModel, Task> createWorktreeForBranchFunc,
            Func<BranchViewModel, Task> createWorktreeFromBranchFunc,
            Func<LocalBranchWithWorktreeViewModel, Task> removeFunc,
            Func<BranchWithWorktreeViewModel, Task> openFolderFunc,
            Func<BranchWithWorktreeViewModel, Task> openTerminalFunc,
            Func<BranchWithWorktreeViewModel, Task> openVisualStudioCodeFunc,
            Func<BranchWithWorktreeViewModel, Task> openVisualStudioFunc)
        {
            var localHeadVm = new HeadBranchWithWorktreeViewModel
            {
                Name = branches.LocalHead.Name,
                Path = branches.LocalHead.WorktreePath,
                Ahead = branches.LocalHead.Ahead,
                Behind = branches.LocalHead.Behind,
                CreateWorktreeFromBranchCommand = CommandHelper.CreateCommand(createWorktreeFromBranchFunc),
                OpenFolderCommand = CommandHelper.CreateCommand(openFolderFunc),
                OpenTerminalCommand = CommandHelper.CreateCommand(openTerminalFunc),
                OpenVisualStudioCodeCommand = CommandHelper.CreateCommand(openVisualStudioCodeFunc),
                OpenVisualStudioCommand = CommandHelper.CreateCommand(openVisualStudioFunc)
            };

            // Local branches with worktree
            var worktreeVms = branches.LocalBranches
                .Select(branch => branch as Services.BranchWithWorktree)
                .Where(branch => branch is not null)
                .Select(branch => branch!)
                .Select(branch => new LocalBranchWithWorktreeViewModel
                {
                    Name = branch.Name,
                    Path = branch.WorktreePath,
                    Ahead = branch.Ahead,
                    Behind = branch.Behind,
                    CreateWorktreeFromBranchCommand = CommandHelper.CreateCommand(createWorktreeFromBranchFunc),
                    RemoveCommand = CommandHelper.CreateCommand(removeFunc),
                    OpenFolderCommand = CommandHelper.CreateCommand(openFolderFunc),
                    OpenTerminalCommand = CommandHelper.CreateCommand(openTerminalFunc),
                    OpenVisualStudioCodeCommand = CommandHelper.CreateCommand(openVisualStudioCodeFunc),
                    OpenVisualStudioCommand = CommandHelper.CreateCommand(openVisualStudioFunc)
                });

            // Local branches without worktree
            var localBranchVms = branches.LocalBranches
                .Where(branch => branch is not Services.BranchWithWorktree)
                .Select(branch => new LocalBranchWithoutWorktreeViewModel
                {
                    Name = branch.Name,
                    Ahead = branch.Ahead,
                    Behind = branch.Behind,
                    CreateWorktreeForBranchCommand = CommandHelper.CreateCommand(createWorktreeForBranchFunc),
                    CreateWorktreeFromBranchCommand = CommandHelper.CreateCommand(createWorktreeFromBranchFunc)
                });

            // Remote branches
            var remoteBranchVms = branches.RemoteBranches
                .Select(branch => new RemoteBranchWithoutWorktreeViewModel
                {
                    Name = branch.Name,
                    Ahead = branch.Ahead,
                    Behind = branch.Behind,
                    CreateWorktreeForBranchCommand = CommandHelper.CreateCommand(createWorktreeForBranchFunc),
                    CreateWorktreeFromBranchCommand = CommandHelper.CreateCommand(createWorktreeFromBranchFunc)
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