﻿namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using GitWorktreeManager.Services;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

public class RepoInfo
{
    public string Name { get; }
    public string Path { get; }

    public RepoInfo(string path)
    {
        this.Path = System.IO.Path.GetFullPath(path);
        this.Name = System.IO.Path.GetFileName(path);
    }
}

public partial class RepoViewModel : ObservableObject
{
    private readonly GitApi gitClient;

    public RepoInfo RepoInfo { get; }

    public RepoViewModel(RepoInfo repoInfo)
    {
        this.RepoInfo = repoInfo;
        this.gitClient = new GitApi(this.RepoInfo.Path);

        MainWindow.Instance.Title = this.RepoInfo.Name;
    }

    private ImmutableList<Branch>? branches;

    [ObservableProperty]
    private ImmutableList<Branch>? filteredBranches;

    private string mostRecentQuery = string.Empty;

    [RelayCommand]
    private void QueryChanged(string query)
    {
        this.mostRecentQuery = query;
        this.FilteredBranches = Helpers.FilterBranches(this.branches, query);
    }

    [RelayCommand]
    private async Task Remove(LocalBranchWithWorktree worktree)
    {
        try
        {
            await this.gitClient.RemoveWorktree(worktree.Path);
            await this.Refresh();
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            await this.gitClient.Fetch();
            var branches = await this.gitClient.ListBranchesAsync();
            var worktrees = await this.gitClient.ListWorktrees();

            this.branches = Helpers.CreateBranchVms(branches, worktrees,
                this.CreateWorktreeForBranchCommand,
                this.CreateWorktreeFromBranchCommand,
                this.RemoveCommand,
                this.OpenFolderCommand,
                this.OpenTerminalCommand,
                this.OpenVisualStudioCodeCommand,
                this.OpenVisualStudioCommand);

            QueryChanged(this.mostRecentQuery);
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenFolder(BranchWithWorktree vm)
    {
        try
        {
            var path = Helpers.GetFolderPathForBranch(vm);

            await Launcher.LaunchFolderPathAsync(path);
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenTerminal(BranchWithWorktree vm)
    {
        try
        {
            var path = Helpers.GetFolderPathForBranch(vm);

            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "wt",
                Arguments = $"-d {path}"
            });
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenVisualStudioCode(BranchWithWorktree vm)
    {
        try
        {
            var path = Helpers.GetFolderPathForBranch(vm);

            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "code",
                Arguments = ".",
                WorkingDirectory = path
            });
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenVisualStudio(BranchWithWorktree vm)
    {
        try
        {
            var path = Helpers.GetFolderPathForBranch(vm);

            var sln = Directory.EnumerateFiles(path, "*.sln").FirstOrDefault();
            if (sln is not null)
            {
                await Launcher.LaunchUriAsync(new Uri(sln));
            }
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task CreateWorktreeForBranch(BranchWithoutWorktree vm)
    {
        try
        {
            if (vm is LocalBranchWithoutWorktree)
            {
                await this.gitClient.AddWorktreeForLocalBranch(vm.Name);
                await this.Refresh();
            }
            else if (vm is RemoteBranchWithoutWorktree)
            {
                await this.gitClient.AddWorktreeForRemoteBranch(vm.Name);
                await this.Refresh();
            }
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task CreateWorktreeFromBranch(Branch vm)
    {
        try
        {
            var newBranchName = await DialogHelper.ShowNewBranchDialogAsync(vm.Name);

            if (string.IsNullOrWhiteSpace(newBranchName))
            {
                return;
            }

            if (vm is RemoteBranchWithoutWorktree)
            {
                await this.gitClient.AddWorktreeBasedOnRemoteBranch(newBranchName, vm.Name);
                await this.Refresh();
            }
            else
            {
                await this.gitClient.AddWorktreeBasedOnLocalBranch(newBranchName, vm.Name);
                await this.Refresh();
            }
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }
}
