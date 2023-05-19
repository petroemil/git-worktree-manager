namespace GitWorktreeManager.ViewModel;
using GitWorktreeManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Windows.Input;

public static class ViewModelHelpers
{
    public static ImmutableList<BranchViewModel> FilterBranches(ImmutableList<BranchViewModel> branches, string query)
    {
        return branches?
            .Where(branch => string.IsNullOrWhiteSpace(query) || branch.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToImmutableList();
    }

    public static ImmutableList<BranchViewModel> CreateBranchVms(
        ListBranchResult branches,
        IReadOnlyDictionary<string, string> worktrees,
        ICommand createWorktreeForBranchCommand,
        ICommand createWorktreeFromBranchCommand,
        ICommand removeCommand,
        ICommand openFolderCommand,
        ICommand openTerminalCommand,
        ICommand openVisualStudioCodeCommand,
        ICommand openVisualStudioCommand)
    {
        // Work tree for HEAD
        var localHeadVm = new LocalHeadBranchWithWorkTreeViewModel
        {
            Name = branches.LocalHead,
            Path = worktrees[branches.LocalHead],
            CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand,
            OpenFolderCommand = openFolderCommand,
            OpenTerminalCommand = openTerminalCommand,
            OpenVisualStudioCodeCommand = openVisualStudioCodeCommand,
            OpenVisualStudioCommand = openVisualStudioCommand
        };

        // Local branches with worktree
        var worktreeVms = branches.LocalBranches
            .Where(branch => worktrees.ContainsKey(branch) is true)
            .Select(branch => new LocalBranchWithWorktreeViewModel
            {
                Name = branch,
                Path = worktrees[branch],
                CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand,
                RemoveCommand = removeCommand,
                OpenFolderCommand = openFolderCommand,
                OpenTerminalCommand = openTerminalCommand,
                OpenVisualStudioCodeCommand = openVisualStudioCodeCommand,
                OpenVisualStudioCommand = openVisualStudioCommand
            });

        // Local branches without worktree
        var localBranchVms = branches.LocalBranches
            .Where(branch => worktrees.ContainsKey(branch) is false)
            .Select(branch => new LocalBranchViewModel
            {
                Name = branch,
                CreateWorktreeForBranchCommand = createWorktreeForBranchCommand,
                CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand
            });

        // Remote branches
        var remoteBranchVms = branches.RemoteBranches
            .Select(branch => new RemoteBranchViewModel
            {
                Name = branch,
                CreateWorktreeForBranchCommand = createWorktreeForBranchCommand,
                CreateWorktreeFromBranchCommand = createWorktreeFromBranchCommand
            });

        return Enumerable.Empty<BranchViewModel>()
            .Append(localHeadVm)
            .Concat(worktreeVms)
            .Concat(localBranchVms)
            .Concat(remoteBranchVms)
            .ToImmutableList();
    }

    public static string GetFolderPathForBranch(BranchViewModel vm)
    {
        var path = vm switch
        {
            LocalBranchWithWorktreeViewModel x => x.Path,
            LocalHeadBranchWithWorkTreeViewModel x => x.Path,
            _ => null
        };

        if (path is null)
        {
            return null;
        }

        return Path.GetFullPath(path);
    }
}
