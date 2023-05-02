using Microsoft.UI.Xaml.Controls;

namespace GitWorktreeManager
{
    public sealed partial class NewBranchDialogContent : UserControl
    {
        public string BaseBranchName { get; set; }
        
        public string BranchName { get; set; }

        public NewBranchDialogContent()
        {
            this.InitializeComponent();
        }
    }
}
