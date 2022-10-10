namespace GitWorktreeManager;

using GitWorktreeManager.ViewModel;
using Microsoft.UI.Xaml.Controls;

public sealed partial class MainView : UserControl
{
    public MainViewModel VM { get; } = new();

    public MainView()
    {
        this.InitializeComponent();
    }
}
