namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Services;
using GitWorktreeManager.Services.Abstractions;
using System.Threading.Tasks;

internal sealed partial class MainViewModel : ViewModelBase
{
    public static MainViewModel Instance => field ??= new();

    private readonly IDialogService dialogService;

    [ObservableProperty]
    public partial RepoInfo[] RecentlyOpenedRepos { get; private set; }

    [ObservableProperty]
    public partial RepoViewModel? Repo { get; private set; }

    public IAsyncRelayCommand<RepoInfo?> OpenRepoCommand { get; }

    private MainViewModel()
    {
        this.dialogService = new DialogService();
        this.RecentlyOpenedRepos = AppSettingsHelper.GetRecentlyOpenedRepos();

        this.OpenRepoCommand = CreateCommand<RepoInfo?>(OpenRepo);
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

        // This operation will fail if the selected folder is not a git repository, but that's fine because the error will be handled and shown to the user in the UI.
        await repo.Refresh();

        this.Repo = repo;
        AppSettingsHelper.SaveRecentlyOpenedRepo(repoInfo);

        // Trigger a full refresh with Fetch to get msot up-to-date state of the repo.
        repo.RefreshCommand.Execute(null);
    }
}
