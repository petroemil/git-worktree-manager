namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using GitWorktreeManager.Services.Abstractions;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RepoInfo(string path)
{
    public string Name { get; } = System.IO.Path.GetFileName(path);
    public string Path { get; } = System.IO.Path.GetFullPath(path);
}

internal sealed class ErrorInfo
{
    public required string Title { get; init; }
    public required string Description { get; init; }
}

internal sealed partial class RepoViewModel : ObservableObject
{
    private readonly IRepoService repoService;
    private readonly IDialogService dialogService;

    public RepoInfo RepoInfo { get; }

    private ImmutableList<BranchViewModel>? branches;

    public ObservableCollection<BranchViewModel> FilteredBranches { get; } = [];

    private string mostRecentQuery = string.Empty;

    public IAsyncRelayCommand RefreshCommand { get; }

    public IAsyncRelayCommand<string> QueryChangedCommand { get; }

    [ObservableProperty]
    public partial ErrorInfo? Error { get; private set; }

    public RepoViewModel(RepoInfo repoInfo, IRepoService repoService, IDialogService dialogService)
    {   
        this.RepoInfo = repoInfo;
        this.repoService = repoService;
        this.dialogService = dialogService;

        this.RefreshCommand = CreateCommand(RefreshWithFetch);
        this.QueryChangedCommand = CreateCommand<string>(QueryChanged);

        MainWindow.Instance.Title = this.RepoInfo.Name;
    }

    private void QueryChanged(string query)
    {
        this.mostRecentQuery = query;

        var filteredBranches = Helpers.FilterBranches(this.branches, query);

        if (filteredBranches is not null)
        {
            this.FilteredBranches.Update(filteredBranches);
        }
    }

    public async Task RefreshWithFetch()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await this.Refresh();
        await this.repoService.Fetch(cts.Token);
        await this.Refresh();
    }

    public async Task Refresh()
    {
        var branches = await this.repoService.ListBranchesAsync();

        this.branches = Helpers.CreateBranchVms(branches, this, this.repoService, this.dialogService);

        QueryChanged(this.mostRecentQuery);
    }

    [RelayCommand]
    private void DismissError()
    {
        this.Error = null;
    }
}
