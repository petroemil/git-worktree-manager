namespace GitWorktreeManager.Behaviors;

using WinRT.Interop;

internal static class WinRTInteropHelper
{
    /// <summary>
    /// Hack needed in WinUI 3 Desktop app to use certain popups
    /// </summary>
    public static void InteropInitialize(this object obj)
    {
        var window = MainWindow.Instance;

        // Get the current window's HWND by passing in the Window object
        var hwnd = WindowNative.GetWindowHandle(window);

        // Associate the HWND with the file picker
        InitializeWithWindow.Initialize(obj, hwnd);
    }
}
