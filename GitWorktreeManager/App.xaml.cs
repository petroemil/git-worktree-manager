namespace GitWorktreeManager;

using Microsoft.UI.Xaml;

public sealed partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Instance.Activate();
    }
}