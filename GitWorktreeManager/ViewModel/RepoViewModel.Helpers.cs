namespace GitWorktreeManager.ViewModel;

using GitWorktreeManager.Services;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Windows.Input;

internal sealed partial class RepoViewModel
{
    public static class Helpers
    {
        public static ImmutableList<Branch>? FilterBranches(ImmutableList<Branch>? branches, string query)
        {
            return branches?
                .Where(branch => string.IsNullOrWhiteSpace(query) || branch.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
                .Take(50)
                .ToImmutableList();
        }

        public static ImmutableList<Branch> CreateBranchVms(
            ListBranchResult branches,
            ImmutableList<Worktree> worktrees,
            ICommand createWorktreeForBranchCommand,
            ICommand createWorktreeFromBranchCommand,
            ICommand removeCommand,
            ICommand openFolderCommand,
            ICommand openTerminalCommand,
            ICommand openVisualStudioCodeCommand,
            ICommand openVisualStudioCommand)
        {
            // Worktree for HEAD
            var worktreeForLocalHead = worktrees.FirstOrDefault(x => x.Branch == branches.LocalHead)
                ?? throw new Exception($"Make sure to have '{branches.LocalHead}' checked out in the root of the repo, then hit Refresh.");

            var localHeadVm = new HeadBranchWithWorktree
            {
                Name = branches.LocalHead,
                Path = worktreeForLocalHead.Path,
                CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand,
                OpenFolderCommand = openFolderCommand,
                OpenTerminalCommand = openTerminalCommand,
                OpenVisualStudioCodeCommand = openVisualStudioCodeCommand,
                OpenVisualStudioCommand = openVisualStudioCommand
            };

            // Local branches with worktree
            var worktreeVms = branches.LocalBranches
                .Where(branch => worktrees.Any(x => x.Branch == branch) is true)
                .Select(branch => new LocalBranchWithWorktree
                {
                    Name = branch,
                    Path = worktrees.First(x => x.Branch == branch).Path,
                    CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand,
                    RemoveCommand = removeCommand,
                    OpenFolderCommand = openFolderCommand,
                    OpenTerminalCommand = openTerminalCommand,
                    OpenVisualStudioCodeCommand = openVisualStudioCodeCommand,
                    OpenVisualStudioCommand = openVisualStudioCommand
                });

            // Local branches without worktree
            var localBranchVms = branches.LocalBranches
                .Where(branch => worktrees.Any(x => x.Branch == branch) is false)
                .Select(branch => new LocalBranchWithoutWorktree
                {
                    Name = branch,
                    CreateWorktreeForBranchCommand = createWorktreeForBranchCommand,
                    CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand
                });

            // Remote branches
            var remoteBranchVms = branches.RemoteBranches
                .Select(branch => new RemoteBranchWithoutWorktree
                {
                    Name = branch,
                    CreateWorktreeForBranchCommand = createWorktreeForBranchCommand,
                    CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand
                });

            return Enumerable.Empty<Branch>()
                .Append(localHeadVm)
                .Concat(worktreeVms)
                .Concat(localBranchVms)
                .Concat(remoteBranchVms)
                .ToImmutableList();
        }

        public static string GetFolderPathForBranch(BranchWithWorktree branch)
        {
            return Path.GetFullPath(branch.Path);
        }
    }
}