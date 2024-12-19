namespace GitWorktreeManager.Services;

using GitWorktreeManager.Behaviors;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

internal class DialogService : IDialogService
{
    public async Task ShowErrorAsync(Exception exception)
    {
        static async Task ShowAsync(string title, string details)
        {
            var dialog = new ContentDialog
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = MainWindow.Instance.Content.XamlRoot,

                Title = title,
                Content = details,
                CloseButtonText = "Ok"
            };

            await dialog.ShowAsync();
        }

        if (exception is GitException gitException)
        {
            await ShowAsync(
                $"Git Error ({gitException.ExitCode})", 
                $"git {gitException.Command}"
                + Environment.NewLine
                + Environment.NewLine
                + gitException.Error);
        }
        else
        {
            await ShowAsync(exception.GetType().Name, exception.Message);
        }
    }

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