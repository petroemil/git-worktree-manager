namespace GitWorktreeManager;

using Microsoft.UI.Xaml.Controls;

public sealed partial class NewBranchDialogContent : UserControl
{
    public string BaseBranchName { get; set; }
    
    public string BranchName { get; set; }

    public NewBranchDialogContent()
    {
        this.InitializeComponent();
    }
}
