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
    [ObservableProperty]
    private RepoViewModel repo;

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

        this.Repo = new RepoViewModel(folder.Path);
        await this.Repo.RefreshCommand.ExecuteAsync(null);
    }
}
