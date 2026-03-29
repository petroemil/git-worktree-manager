namespace GitWorktreeManager;

using GitWorktreeManager.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

internal sealed partial class MainWindow : Window
{
    public static MainWindow Instance => field ??= new MainWindow { Title = "Branch Manager" };

    private MainWindow()
    {
        this.InitializeComponent();

        this.SystemBackdrop = new MicaBackdrop();

        this.SetTransparentTitlebar();
        this.SetAppIcon();
        this.SetSize(1000, 1500);
    }
}
