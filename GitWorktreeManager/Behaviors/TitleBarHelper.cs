namespace GitWorktreeManager.Behaviors;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

internal static class TitleBarHelper
{
    private static AppWindow GetAppWindow(Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var winId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(winId);
    }

    public static void SetTransparentTitlebar(Window window)
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
}
