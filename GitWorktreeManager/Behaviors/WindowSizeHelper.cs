namespace GitWorktreeManager.Behaviors;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;

internal static class WindowSizeHelper
{
    private static AppWindow GetAppWindow(Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var winId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(winId);
    }

    public static void SetSize(Window window, int width, int height)
    {
        // Get AppWindow
        var appWindow = GetAppWindow(window);

        // Resize App
        appWindow.Resize(new SizeInt32(width, height));
    }
}
