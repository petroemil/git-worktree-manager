namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using GitWorktreeManager.Services;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

internal sealed class RepoInfo
{
    public string Name { get; }
    public string Path { get; }

    public RepoInfo(string path)
    {
        this.Path = System.IO.Path.GetFullPath(path);
        this.Name = System.IO.Path.GetFileName(path);
    }
}

internal sealed partial class RepoViewModel : ObservableObject
{
    private readonly GitApi gitClient;

    public RepoInfo RepoInfo { get; }

    private ImmutableList<Branch>? branches;

    public ObservableCollection<Branch> FilteredBranches { get; } = [];

    private string mostRecentQuery = string.Empty;

    public IAsyncRelayCommand RefreshCommand => CommandHelper.CreateCommand(Refresh);
    public IAsyncRelayCommand<string> QueryChangedCommand => CommandHelper.CreateCommand<string>(QueryChanged);

    public RepoViewModel(RepoInfo repoInfo)
    {
        this.RepoInfo = repoInfo;
        this.gitClient = new GitApi(this.RepoInfo.Path);

        MainWindow.Instance.Title = this.RepoInfo.Name;
    }

    public void QueryChanged(string query)
    {
        this.mostRecentQuery = query;

        var filteredBranches = Helpers.FilterBranches(this.branches, query);

        if (filteredBranches is not null)
        {
            this.FilteredBranches.Update(filteredBranches);
        }
    }

    public async Task Refresh()
    {
        await this.gitClient.Fetch();

        var branches = await this.gitClient.ListBranchesAsync();

        this.branches = Helpers.CreateBranchVms(branches,
            this.CreateWorktreeForBranch,
            this.CreateWorktreeFromBranch,
            this.Remove,
            this.OpenFolder,
            this.OpenTerminal,
            this.OpenVisualStudioCode,
            this.OpenVisualStudio);

        QueryChanged(this.mostRecentQuery);
    }

    public async Task CreateWorktreeForBranch(BranchWithoutWorktree vm)
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

    public async Task CreateWorktreeFromBranch(Branch vm)
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

    public async Task Remove(LocalBranchWithWorktree worktree)
    {
        await this.gitClient.RemoveWorktree(worktree.Path);
        await this.Refresh();
    }

    public async Task OpenFolder(BranchWithWorktree vm)
    {
        var path = Helpers.GetFolderPathForBranch(vm);

        await Launcher.LaunchFolderPathAsync(path);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task OpenTerminal(BranchWithWorktree vm)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        var path = Helpers.GetFolderPathForBranch(vm);

        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = false,
            FileName = "wt",
            Arguments = $"-d {path}"
        });
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task OpenVisualStudioCode(BranchWithWorktree vm)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

    public async Task OpenVisualStudio(BranchWithWorktree vm)
    {
        var path = Helpers.GetFolderPathForBranch(vm);

        var sln = Directory.EnumerateFiles(path, "*.sln").FirstOrDefault();
        if (sln is not null)
        {
            await Launcher.LaunchUriAsync(new Uri(sln));
        }
    }
}
