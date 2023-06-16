namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.System;

public class NewBranchContentDialog : ContentDialog
{
    private readonly NewBranchDialogContent content;

    private ContentDialogResult result;

    public string? NewBranch => this.content.BranchName;

    public NewBranchContentDialog(string baseBranch)
    {
        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        XamlRoot = MainWindow.Instance.Content.XamlRoot;

        this.content = new NewBranchDialogContent
        {
            BaseBranchName = $"Based on '{baseBranch}'"
        };

        Title = "New branch";
        Content = content;
        PrimaryButtonText = "Ok";
        SecondaryButtonText = "Cancel";

        content.CloseKeyPressed += key =>
        {
            if (key is VirtualKey.Enter)
            {
                this.result = ContentDialogResult.Primary;
                this.Hide(); // Will cause the base.ShowAsync() call to return with ContentDialogResult.None
            }
            else if (key is VirtualKey.Escape)
            {
                this.result = ContentDialogResult.Secondary;
                this.Hide(); // Will cause the base.ShowAsync() call to return with ContentDialogResult.None
            }
        };
    }

    public new async Task<ContentDialogResult> ShowAsync()
    {
        var baseResult = await base.ShowAsync();
        if (baseResult is not ContentDialogResult.None)
        {
            return baseResult;
        }
        else
        {
            return this.result;
        }
    }
}
