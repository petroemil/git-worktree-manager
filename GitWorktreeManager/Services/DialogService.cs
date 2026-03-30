namespace GitWorktreeManager.Services;

using GitWorktreeManager.Behaviors;
using GitWorktreeManager.Services.Abstractions;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

internal class DialogService : IDialogService
{
    public async Task<string?> ShowNewBranchDialogAsync(string baseBranch)
    {
        var dialog = new NewBranchContentDialog(baseBranch);

        if (await dialog.ShowAsync() is ContentDialogResult.Primary)
        {
            return dialog.NewBranch;
        }
        else
        {
            return null;
        }
    }

    public async Task<string?> OpenFolderAsync()
    {
        var picker = new FolderPicker();
        picker.InteropInitialize();

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}