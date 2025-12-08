namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Xaml;
using Windows.Graphics;

internal static class WindowExtensions
{
    public static void SetAppIcon(this Window window)
    {
        window.AppWindow.SetIcon("Assets/Icon.ico");
    }

    public static void SetSize(this Window window, int width, int height)
    {
        window.AppWindow.Resize(new SizeInt32(width, height));
    }
}
