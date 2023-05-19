﻿namespace GitWorktreeManager.ViewModel;

using GitWorktreeManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Windows.Input;

public static class ViewModelHelpers
{
    public static ImmutableList<Branch> FilterBranches(ImmutableList<Branch> branches, string query)
    {
        return branches?
            .Where(branch => string.IsNullOrWhiteSpace(query) || branch.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToImmutableList();
    }

    public static ImmutableList<Branch> CreateBranchVms(
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
        var localHeadVm = new HeadBranchWithWorktree
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
            .Select(branch => new LocalBranchWithWorktree
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
