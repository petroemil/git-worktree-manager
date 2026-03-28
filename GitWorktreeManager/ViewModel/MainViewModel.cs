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
    public partial RepoViewModel? Repo { get; private set; }

    [ObservableProperty]
    public partial RepoInfo[] RecentlyOpenedRepos { get; private set; }

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
        repo.RefreshCommand.Execute(null);

        this.Repo = repo;

        AppSettingsHelper.SaveRecentlyOpenedRepo(repoInfo);
    }
}
