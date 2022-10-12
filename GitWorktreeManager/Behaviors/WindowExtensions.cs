namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Xaml;
using Microsoft.UI;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Windows.Graphics;

internal static class WindowExtensions
{
    private static AppWindow GetAppWindow(this Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var winId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(winId);
    }

    public static void SetAppIcon(this Window window)
    {
        // Get AppWindow
        var appWindow = GetAppWindow(window);
        
        // Set App Icon
        appWindow.SetIcon("Assets/Icon.ico");
    }

    public static void SetTransparentTitlebar(this Window window)
    {
        // Get AppWindow
        var appWindow = GetAppWindow(window);

        // Get Titlebar
        var titleBar = appWindow.TitleBar;

        // Set up TitleBar
        titleBar.ExtendsContentIntoTitleBar = true;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
    }

    public static void SetSize(this Window window, int width, int height)
    {
        // Get AppWindow
        var appWindow = GetAppWindow(window);

        // Resize App
        appWindow.Resize(new SizeInt32(width, height));
    }

    public static void SetMicaBackdrop(this Window window)
    {
        var helper = new MicaBackgroundHelper(window);
        helper.TrySetMicaBackdrop();
    }
}
