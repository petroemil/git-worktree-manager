namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using GitWorktreeManager.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;

public class RepoInfo
{
    public string Name { get; init; }
    public string Path { get; init; }
}

public abstract class BranchViewModel
{
    public string Name { get; init; }
    public string DisplayName => Name.Replace("/", " / ");
}

public sealed class LocalHeadBranchWithWorkTreeViewModel : BranchViewModel
{
    public string Path { get; init; }

    public ICommand CreateWorktreeFromBranch { get; init; }
    public ICommand OpenFolderCommand { get; init; }
    public ICommand OpenTerminalCommand { get; init; }
    public ICommand OpenVisualStudioCodeCommand { get; init; }
    public ICommand OpenVisualStudioCommand { get; init; }
}

public class LocalBranchWithWorktreeViewModel : BranchViewModel
{
    public string Path { get; init; }

    public ICommand CreateWorktreeFromBranch { get; init; }
    public ICommand RemoveCommand { get; init; }
    public ICommand OpenFolderCommand { get; init; }
    public ICommand OpenTerminalCommand { get; init; }
    public ICommand OpenVisualStudioCodeCommand { get; init; }
    public ICommand OpenVisualStudioCommand { get; init; }
}

public sealed class LocalBranchViewModel : BranchViewModel
{
    public ICommand CreateWorktreeForBranch { get; init; }
    public ICommand CreateWorktreeFromBranch { get; init; }
}

public sealed class RemoteBranchViewModel : BranchViewModel
{
    public ICommand CreateWorktreeForBranch { get; init; }
    public ICommand CreateWorktreeFromBranch { get; init; }
}

public sealed class BranchTemplateSelector : DataTemplateSelector
{
    public DataTemplate LocalHeadBranchTemplate { get; set; }
    public DataTemplate LocalBranchWithWorktreeTemplate { get; set; }
    public DataTemplate LocalBranchTemplate { get; set; }
    public DataTemplate RemoteBranchTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            LocalHeadBranchWithWorkTreeViewModel => LocalHeadBranchTemplate,
            LocalBranchWithWorktreeViewModel => LocalBranchWithWorktreeTemplate,
            LocalBranchViewModel => LocalBranchTemplate,
            RemoteBranchViewModel => RemoteBranchTemplate,
            _ => null
        };
    }
}

[INotifyPropertyChanged]
public partial class RepoViewModel
{
    private readonly GitApi gitClient;

    public RepoInfo RepoInfo { get; }

    [ObservableProperty]
    private ImmutableList<BranchViewModel> worktrees;

    public RepoViewModel(string repoPath)
    {
        var path = Path.GetFullPath(repoPath);
        var name = Path.GetFileName(path);

        this.RepoInfo = new RepoInfo
        {
            Path = path,
            Name = name
        };

        this.gitClient = new GitApi(path);
    }

    [RelayCommand]
    private async Task Remove(LocalBranchWithWorktreeViewModel worktree)
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

            // Work tree for HEAD
            var localHeadVm = new LocalHeadBranchWithWorkTreeViewModel
            {
                Name = branches.LocalHead,
                Path = worktrees[branches.LocalHead],
                CreateWorktreeFromBranch = this.CreateWorktreeFromBranchCommand,
                OpenFolderCommand = this.OpenFolderCommand,
                OpenTerminalCommand = this.OpenTerminalCommand,
                OpenVisualStudioCodeCommand = this.OpenVisualStudioCodeCommand,
                OpenVisualStudioCommand = this.OpenVisualStudioCommand
            };

            // Local branches with worktree
            var worktreeVms = branches.LocalBranches
                .Where(branch => worktrees.ContainsKey(branch) is true)
                .Select(branch => new LocalBranchWithWorktreeViewModel
                {
                    Name = branch,
                    Path = worktrees[branch],
                    CreateWorktreeFromBranch = this.CreateWorktreeFromBranchCommand,
                    RemoveCommand = this.RemoveCommand,
                    OpenFolderCommand = this.OpenFolderCommand,
                    OpenTerminalCommand = this.OpenTerminalCommand,
                    OpenVisualStudioCodeCommand = this.OpenVisualStudioCodeCommand,
                    OpenVisualStudioCommand = this.OpenVisualStudioCommand
                });

            // Local branches without worktree
            var localBranchVms = branches.LocalBranches
                .Where(branch => worktrees.ContainsKey(branch) is false)
                .Select(branch => new LocalBranchViewModel
                {
                    Name = branch,
                    CreateWorktreeForBranch = this.CreateWorktreeForBranchCommand,
                    CreateWorktreeFromBranch = this.CreateWorktreeFromBranchCommand
                });

            // Remote head
            var remoteHeadVm = new RemoteBranchViewModel
            {
                Name = branches.RemoteHead,
                CreateWorktreeForBranch = this.CreateWorktreeForBranchCommand,
                CreateWorktreeFromBranch = this.CreateWorktreeFromBranchCommand
            };

            // Remote branches
            var remoteBranchVms = branches.RemoteBranches
                .Select(branch => new RemoteBranchViewModel
                {
                    Name = branch,
                    CreateWorktreeForBranch = this.CreateWorktreeForBranchCommand,
                    CreateWorktreeFromBranch = this.CreateWorktreeFromBranchCommand
                });

            this.Worktrees = Enumerable.Empty<BranchViewModel>()
                .Append(localHeadVm)
                .Concat(worktreeVms)
                .Concat(localBranchVms)
                .Append(remoteHeadVm)
                .Concat(remoteBranchVms)
                .ToImmutableList();
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenFolder(BranchViewModel vm)
    {
        try
        {
            var path = vm switch
            {
                LocalBranchWithWorktreeViewModel x => x.Path,
                LocalHeadBranchWithWorkTreeViewModel x => x.Path,
                _ => null
            };

            if (path is null)
            {
                return;
            }

            await Launcher.LaunchFolderPathAsync(Path.GetFullPath(path));
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenTerminal(BranchViewModel vm)
    {
        try
        {
            var path = vm switch
            {
                LocalBranchWithWorktreeViewModel x => x.Path,
                LocalHeadBranchWithWorkTreeViewModel x => x.Path,
                _ => null
            };

            if (path is null)
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "powershell",
                WorkingDirectory = Path.GetFullPath(path)
            });
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenVisualStudioCode(BranchViewModel vm)
    {
        try
        {
            var path = vm switch
            {
                LocalBranchWithWorktreeViewModel x => x.Path,
                LocalHeadBranchWithWorkTreeViewModel x => x.Path,
                _ => null
            };

            if (path is null)
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "code",
                Arguments = ".",
                WorkingDirectory = Path.GetFullPath(path)
            });
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenVisualStudio(BranchViewModel vm)
    {
        try
        {
            var path = vm switch
            {
                LocalBranchWithWorktreeViewModel x => x.Path,
                LocalHeadBranchWithWorkTreeViewModel x => x.Path,
                _ => null
            };

            if (path is null)
            {
                return;
            }

            var sln = Directory.EnumerateFiles(Path.GetFullPath(path), "*.sln").FirstOrDefault();
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
    private async Task CreateWorktreeForBranch(BranchViewModel vm)
    {
        try
        {
            if (vm is LocalBranchViewModel)
            {
                await this.gitClient.AddWorktreeForLocalBranch(vm.Name);
                await this.Refresh();
            }
            else if (vm is RemoteBranchViewModel)
            {
                await this.gitClient.AddWorktreeForRemoteBranch(vm.Name["origin/".Length..]);
                await this.Refresh();
            }
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task CreateWorktreeFromBranch(BranchViewModel vm)
    {
        try
        {
            await DialogHelper.ShowErrorAsync(new Exception($"Name of new branch based on '{vm.Name}'"));
            var newBranchName = "test-123";

            if (vm is LocalBranchViewModel or LocalBranchWithWorktreeViewModel or LocalHeadBranchWithWorkTreeViewModel)
            {
                await this.gitClient.AddWorktreeForNewBranch(newBranchName, vm.Name);
                await this.Refresh();
            }
            else if (vm is RemoteBranchViewModel)
            {
                await this.gitClient.AddWorktreeForNewBranch(newBranchName, vm.Name["origin/".Length..]);
                await this.Refresh();
            }
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }
}
