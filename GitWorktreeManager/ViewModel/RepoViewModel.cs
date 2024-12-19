namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using GitWorktreeManager.Services;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

internal sealed class RepoInfo(string path)
{
    public string Name { get; } = System.IO.Path.GetFileName(path);
    public string Path { get; } = System.IO.Path.GetFullPath(path);
}

internal sealed partial class RepoViewModel : ObservableObject
{
    private readonly IRepoService repoService;
    private readonly IDialogService dialogService;

    public RepoInfo RepoInfo { get; }

    private ImmutableList<BranchViewModel>? branches;

    public ObservableCollection<BranchViewModel> FilteredBranches { get; } = [];

    private string mostRecentQuery = string.Empty;

    public IAsyncRelayCommand RefreshCommand => CommandHelper.CreateCommand(Refresh);
    public IAsyncRelayCommand<string> QueryChangedCommand => CommandHelper.CreateCommand<string>(QueryChanged);

    public RepoViewModel(RepoInfo repoInfo, IRepoService repoService, IDialogService dialogService)
    {   
        this.RepoInfo = repoInfo;
        this.repoService = repoService;
        this.dialogService = dialogService;

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
        await this.repoService.Fetch();

        var branches = await this.repoService.ListBranchesAsync();

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

    public async Task CreateWorktreeForBranch(BranchWithoutWorktreeViewModel vm)
    {
        if (vm is LocalBranchWithoutWorktreeViewModel)
        {
            await this.repoService.AddWorktreeForLocalBranch(vm.Name);
            await this.Refresh();
        }
        else if (vm is RemoteBranchWithoutWorktreeViewModel)
        {
            await this.repoService.AddWorktreeForRemoteBranch(vm.Name);
            await this.Refresh();
        }
    }

    public async Task CreateWorktreeFromBranch(BranchViewModel vm)
    {
        var newBranchName = await this.dialogService.ShowNewBranchDialogAsync(vm.Name);

        if (string.IsNullOrWhiteSpace(newBranchName))
        {
            return;
        }

        if (vm is RemoteBranchWithoutWorktreeViewModel)
        {
            await this.repoService.AddWorktreeBasedOnRemoteBranch(newBranchName, vm.Name);
            await this.Refresh();
        }
        else
        {
            await this.repoService.AddWorktreeBasedOnLocalBranch(newBranchName, vm.Name);
            await this.Refresh();
        }
    }

    public async Task Remove(LocalBranchWithWorktreeViewModel worktree)
    {
        await this.repoService.RemoveWorktree(worktree.Path);
        await this.Refresh();
    }

    public async Task OpenFolder(BranchWithWorktreeViewModel vm)
    {
        var path = Helpers.GetFolderPathForBranch(vm);
        await this.repoService.OpenFolder(path);
    }

    public async Task OpenTerminal(BranchWithWorktreeViewModel vm)
    {
        var path = Helpers.GetFolderPathForBranch(vm);
        await this.repoService.OpenTerminal(path);
    }

    public async Task OpenVisualStudioCode(BranchWithWorktreeViewModel vm)
    {
        var path = Helpers.GetFolderPathForBranch(vm);
        await this.repoService.OpenVisualStudioCode(path);
    }

    public async Task OpenVisualStudio(BranchWithWorktreeViewModel vm)
    {
        var path = Helpers.GetFolderPathForBranch(vm);
        await this.repoService.OpenVisualStudio(path);
    }
}
