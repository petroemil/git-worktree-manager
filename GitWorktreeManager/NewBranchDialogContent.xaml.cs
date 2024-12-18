namespace GitWorktreeManager;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using Windows.System;

internal sealed partial class NewBranchDialogContent : UserControl
{
    public string? BaseBranchName { get; set; }
    
    public string? BranchName { get; set; }

    public event Action<VirtualKey>? CloseKeyPressed;

    public NewBranchDialogContent()
    {
        this.InitializeComponent();
    }

    private void OnKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.Enter or VirtualKey.Escape)
        {
            this.CloseKeyPressed?.Invoke(e.Key);
        }
    }
}
