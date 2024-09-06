namespace GitWorktreeManager;

using GitWorktreeManager.ViewModel;
using Microsoft.UI.Xaml.Controls;

internal sealed partial class MainView : UserControl
{
    public MainViewModel VM { get; } = new();

    public MainView()
    {
        this.InitializeComponent();

        // This should be an attached property but for some weird reason I can't make it work, so hack it is for the time being.
        this.RecentlyOpenedReposListView.IsEnabledChanged += (_, _) =>
        {
            this.RecentlyOpenedReposListView.Opacity = this.RecentlyOpenedReposListView.IsEnabled ? 1 : 0.75;
        };
    }
}
