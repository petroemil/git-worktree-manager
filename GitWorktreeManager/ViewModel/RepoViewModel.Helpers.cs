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
            ICommand createWorktreeForBranchCommand,
            ICommand createWorktreeFromBranchCommand,
            ICommand removeCommand,
            ICommand openFolderCommand,
            ICommand openTerminalCommand,
            ICommand openVisualStudioCodeCommand,
            ICommand openVisualStudioCommand)
        {
            var localHeadVm = new HeadBranchWithWorktree
            {
                Name = branches.LocalHead.Name,
                Path = branches.LocalHead.WorktreePath,
                CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand,
                OpenFolderCommand = openFolderCommand,
                OpenTerminalCommand = openTerminalCommand,
                OpenVisualStudioCodeCommand = openVisualStudioCodeCommand,
                OpenVisualStudioCommand = openVisualStudioCommand
            };

            // Local branches with worktree
            var worktreeVms = branches.LocalBranches
                .Select(branch => branch as Services.BranchWithWorktree)
                .Where(branch => branch is not null)
                .Select(branch => branch!)
                .Select(branch => new LocalBranchWithWorktree
                {
                    Name = branch.Name,
                    Path = branch.WorktreePath,
                    CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand,
                    RemoveCommand = removeCommand,
                    OpenFolderCommand = openFolderCommand,
                    OpenTerminalCommand = openTerminalCommand,
                    OpenVisualStudioCodeCommand = openVisualStudioCodeCommand,
                    OpenVisualStudioCommand = openVisualStudioCommand
                });

            // Local branches without worktree
            var localBranchVms = branches.LocalBranches
                .Where(branch => branch is not Services.BranchWithWorktree)
                .Select(branch => new LocalBranchWithoutWorktree
                {
                    Name = branch.Name,
                    CreateWorktreeForBranchCommand = createWorktreeForBranchCommand,
                    CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand
                });

            // Remote branches
            var remoteBranchVms = branches.RemoteBranches
                .Select(branch => new RemoteBranchWithoutWorktree
                {
                    Name = branch.Name,
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