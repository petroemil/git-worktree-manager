namespace GitWorktreeManager;

using GitWorktreeManager.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

public sealed partial class MainWindow : Window
{
    private static MainWindow? instance;
    public static MainWindow Instance => instance ??= new MainWindow { Title = "Branch Manager" };

    private MainWindow()
    {
        this.InitializeComponent();

        this.SystemBackdrop = new MicaBackdrop();

        this.SetTransparentTitlebar();
        this.SetAppIcon();
        this.SetSize(750, 1000);
    }
}
