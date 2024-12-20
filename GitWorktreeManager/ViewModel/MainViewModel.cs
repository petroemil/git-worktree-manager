namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Services;
using GitWorktreeManager.Services.Abstractions;
using System.Threading.Tasks;

internal partial class MainViewModel : ObservableObject
{
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private RepoViewModel? repo;

    [ObservableProperty]
    private RepoInfo[] recentlyOpenedRepos;

    public IAsyncRelayCommand<RepoInfo?> OpenRepoCommand => CommandHelper.CreateCommand<RepoInfo?>(OpenRepo);

    public MainViewModel()
    {
        this.dialogService = new DialogService();
        this.RecentlyOpenedRepos = AppSettingsHelper.GetRecentlyOpenedRepos();
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
        await repo.Refresh();

        this.Repo = repo;

        AppSettingsHelper.SaveRecentlyOpenedRepo(repoInfo);
    }
}
