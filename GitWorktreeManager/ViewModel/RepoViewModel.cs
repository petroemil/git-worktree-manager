namespace GitWorktreeManager.ViewModel;

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
using System.Windows.Input;
using Windows.System;

public class RepoInfo
{
    public string Name { get; init; }
    public string Path { get; init; }
}

public class WorktreeInfo
{
    public string Branch { get; init; }
    public string Path { get; init; }
}

public class WorktreeViewModel
{
    public WorktreeInfo Info { get; init; }
    public string DisplayName => Info.Branch.Replace("/", " / ");
    public ICommand RemoveCommand { get; init; }
    public ICommand OpenFolderCommand { get; init; }
    public ICommand OpenTerminalCommand { get; init; }
    public ICommand OpenVisualStudioCodeCommand { get; init; }
    public ICommand OpenVisualStudioCommand { get; init; }
}

[INotifyPropertyChanged]
public partial class RepoViewModel
{
    private readonly GitApi gitClient;

    public RepoInfo RepoInfo { get; }

    [ObservableProperty]
    private ImmutableList<WorktreeViewModel> worktrees;

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
    private async Task Add(string worktree)
    {
        try
        {
            await this.gitClient.AddWorktree(worktree);
            await this.Refresh();
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task Remove(WorktreeInfo worktree)
    {
        try
        {
            await this.gitClient.RemoveWorktree(worktree.Branch);
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
            var worktrees = await this.gitClient.ListWorktrees();

            this.Worktrees = worktrees
                .OrderBy(wt => wt.Key)
                .Select(wt => new WorktreeViewModel
                {
                    Info = new WorktreeInfo { Branch = wt.Key, Path = wt.Value },
                    RemoveCommand = this.RemoveCommand,
                    OpenFolderCommand = this.OpenFolderCommand,
                    OpenTerminalCommand = this.OpenTerminalCommand,
                    OpenVisualStudioCodeCommand = this.OpenVisualStudioCodeCommand,
                    OpenVisualStudioCommand = this.OpenVisualStudioCommand
                })
                .ToImmutableList();
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenFolder(WorktreeInfo worktree)
    {
        try
        {
            await Launcher.LaunchFolderPathAsync(worktree.Path);
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenTerminal(WorktreeInfo worktree)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "powershell",
                WorkingDirectory = worktree.Path
            });
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenVisualStudioCode(WorktreeInfo worktree)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "code",
                Arguments = ".",
                WorkingDirectory = worktree.Path
            });
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    [RelayCommand]
    private async Task OpenVisualStudio(WorktreeInfo worktree)
    {
        try
        {
            var sln = Directory.EnumerateFiles(worktree.Path, "*.sln").FirstOrDefault();
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
}
