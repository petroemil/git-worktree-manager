namespace GitWorktreeManager;

using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;

public sealed partial class NewBranchDialogContent : UserControl
{
    public string BaseBranchName { get; set; }
    
    public string BranchName { get; set; }

    public event Action<VirtualKey> CloseKeyPressed;

    public NewBranchDialogContent()
    {
        this.InitializeComponent();
    }

    private void OnKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.Enter or VirtualKey.Escape)
        {
            this.CloseKeyPressed(e.Key);
        }
    }
}
