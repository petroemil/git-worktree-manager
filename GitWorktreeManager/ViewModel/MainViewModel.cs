namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

[INotifyPropertyChanged]
public partial class MainViewModel
{
    public string Title => "Git Worktree Manager";

    [ObservableProperty]
    private RepoViewModel repo;

    public MainViewModel()
    {
        if (JumpListHelper.TryGetLaunchArgs(out var repoPath))
        {
            _ = this.InitRepo(repoPath);
        }
    }

    [RelayCommand]
    private async Task OpenRepo()
    {
        var picker = new FolderPicker();
        picker.InteropInitialize();

        var folder = await picker.PickSingleFolderAsync();

        if (folder is null)
        {
            return;
        }

        await this.InitRepo(folder.Path);
        await JumpListHelper.AddItem(folder.Path);
    }

    private async Task InitRepo(string repoPath)
    {
        this.Repo = await RepoViewModel.Create(repoPath);
    }
}
