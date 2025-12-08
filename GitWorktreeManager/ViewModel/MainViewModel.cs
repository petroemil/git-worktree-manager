namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Services;
using GitWorktreeManager.Services.Abstractions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

[ObservableObject]
internal partial class MainViewModel
{
    private readonly IDialogService dialogService;

    public ObservableCollection<RepoViewModel> Repos { get; } = [];

    [ObservableProperty]
    public partial RepoViewModel? SelectedRepo { get; set; }

    public IAsyncRelayCommand<RepoInfo?> OpenRepoCommand => CommandHelper.CreateCommand<RepoInfo?>(OpenRepo);

    public MainViewModel()
    {
        this.dialogService = new DialogService();
        //this.RecentlyOpenedRepos = AppSettingsHelper.GetRecentlyOpenedRepos();
    }

    public async Task OpenRepo(RepoInfo? repoInfo)
    {
        if (repoInfo is null)
        {
            var path = await this.dialogService.OpenFolderAsync();

            if (path is null)
            {
                return;
            }

            repoInfo = new RepoInfo(path);
        }

        var repo = new RepoViewModel(repoInfo, new RepoService(repoInfo.Path), this.dialogService);
        _ = repo.RefreshWithFetch();

        this.Repos.Add(repo);
        this.SelectedRepo = repo;

        //AppSettingsHelper.SaveRecentlyOpenedRepo(repoInfo);
    }
}
