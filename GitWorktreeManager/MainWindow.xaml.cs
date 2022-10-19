namespace GitWorktreeManager;

using GitWorktreeManager.Behaviors;
using Microsoft.UI.Xaml;

public sealed partial class MainWindow : Window
{
    private static MainWindow instance;
    public static MainWindow Instance => instance ??= new MainWindow { Title = "Worktree Manager" };

    private MainWindow()
    {
        this.InitializeComponent();

        this.SetMicaBackdrop();
        this.SetTransparentTitlebar();
        this.SetAppIcon();
        this.SetSize(750, 1000);
    }
}
