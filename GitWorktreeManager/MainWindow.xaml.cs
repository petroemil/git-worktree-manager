namespace GitWorktreeManager;

using GitWorktreeManager.Behaviors;
using Microsoft.UI.Xaml;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        MicaBackgroundHelper.SetMicaBackdrop(this);
        TitleBarHelper.SetTransparentTitlebar(this);
        WindowSizeHelper.SetSize(this, 750, 1000);
    }
}
