namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using GitWorktreeManager.Services;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private RepoViewModel? repo;

    [ObservableProperty]
    private RepoInfo[] recentlyOpenedRepos;

    public IAsyncRelayCommand<RepoInfo?> OpenRepoCommand => CommandHelper.CreateCommand<RepoInfo?>(OpenRepo);

    public MainViewModel()
    {
        this.RecentlyOpenedRepos = AppSettingsHelper.GetRecentlyOpenedRepos();
    }

    public async Task OpenRepo(RepoInfo? repoInfo)
    {
        if (repoInfo is null)
        {
            var picker = new FolderPicker();
            picker.InteropInitialize();

            var folder = await picker.PickSingleFolderAsync();

            if (folder is null)
            {
                return;
            }

            repoInfo = new RepoInfo(folder.Path);
        }

        var repo = new RepoViewModel(repoInfo);
        await repo.Refresh();

        this.Repo = repo;

        AppSettingsHelper.SaveRecentlyOpenedRepo(repoInfo);
    }
}
