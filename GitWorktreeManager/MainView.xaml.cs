namespace GitWorktreeManager;

using GitWorktreeManager.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

internal sealed partial class MainView : UserControl
{
    public MainViewModel VM { get; } = new();

    public MainView()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.SetTitleBar(this.TitleBarDragRegion);
    }
}
