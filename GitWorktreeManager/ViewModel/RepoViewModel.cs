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

public abstract class Branch
{
    public string Name { get; init; }
    public string DisplayName => Name.Replace("/", " / ");

    public abstract string Label { get; }

    public string CreateWorktreeFromBranchLabel => $"Create new worktree based on '{DisplayName}'";
    public ICommand CreateWorktreeFromBranchCommand { get; init; }
}

public abstract class BranchWithWorktree : Branch
{
    public string Path { get; init; }

    public string OpenFileExplorerLabel => "Open File Explorer";
    public ICommand OpenFolderCommand { get; init; }

    public string OpenTerminalLabel => "Open Terminal";
    public ICommand OpenTerminalCommand { get; init; }

    public string OpenVisualStudioCodeLabel => "Open Visual Studio Code";
    public ICommand OpenVisualStudioCodeCommand { get; init; }

    public string OpenVisualStudioLabel => "Open Visual Studio";
    public ICommand OpenVisualStudioCommand { get; init; }
}

public sealed class HeadBranchWithWorktree : BranchWithWorktree
{
    public override string Label => "HEAD branch";
}

public sealed class LocalBranchWithWorktree : BranchWithWorktree
{
    public override string Label => "Local branch with worktree";

    public string RemoveLabel => "Remove worktree";
    public ICommand RemoveCommand { get; init; }
}

public abstract class BranchWithoutWorktree : Branch
{
    public string CreateWorktreeForBranchLabel => $"Create worktree for '{DisplayName}'";
    public ICommand CreateWorktreeForBranchCommand { get; init; }
}

public sealed class LocalBranchWithoutWorktree : BranchWithoutWorktree
{
    public override string Label => "Local branch";
}

public sealed class RemoteBranchWithoutWorktree : BranchWithoutWorktree
{
    public override string Label => "Remote branch";
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
            HeadBranchWithWorktree => LocalHeadBranchTemplate,
            LocalBranchWithWorktree => LocalBranchWithWorktreeTemplate,
            LocalBranchWithoutWorktree => LocalBranchTemplate,
            RemoteBranchWithoutWorktree => RemoteBranchTemplate,
            _ => null
        };
    }
}

[INotifyPropertyChanged]
public partial class RepoViewModel
{
    private readonly GitApi gitClient;

    public RepoInfo RepoInfo { get; }

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

    private ImmutableList<Branch> branches;

    [ObservableProperty]
    private ImmutableList<Branch> filteredBranches;

    private string mostRecentQuery = string.Empty;

    [RelayCommand]
    private void QueryChanged(string query)
    {
        this.mostRecentQuery = query;
        this.FilteredBranches = ViewModelHelpers.FilterBranches(this.branches, query);
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

            this.branches = ViewModelHelpers.CreateBranchVms(branches, worktrees,
                this.CreateWorktreeForBranchCommand,
                this.CreateWorktreeFromBranchCommand,
                this.RemoveCommand,
                this.OpenFolderCommand,
                this.OpenTerminalCommand,
                this.openVisualStudioCodeCommand,
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
            var path = ViewModelHelpers.GetFolderPathForBranch(vm);

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
            var path = ViewModelHelpers.GetFolderPathForBranch(vm);

            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "powershell",
                WorkingDirectory = path
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
            var path = ViewModelHelpers.GetFolderPathForBranch(vm);

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
            var path = ViewModelHelpers.GetFolderPathForBranch(vm);

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
                throw new ArgumentException("Branch name should not be empty.");
            }

            if (vm is RemoteBranchWithoutWorktree)
            {
                await this.gitClient.AddWorktreeForNewBranch(newBranchName, $"origin/{vm.Name}");
                await this.Refresh();
            }
            else
            {
                await this.gitClient.AddWorktreeForNewBranch(newBranchName, vm.Name);
                await this.Refresh();
            }
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }
}
