namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using WinRT;

internal class MicaBackgroundHelper
{
    private readonly Window window;
    private MicaBackgroundHelper(Window window)
    {
        this.window = window;
    }

    public static void SetMicaBackdrop(Window window)
    {
        var helper = new MicaBackgroundHelper(window);
        helper.TrySetMicaBackdrop();
    }

    WindowsSystemDispatcherQueueHelper wsdqHelper;
    MicaController micaController;
    SystemBackdropConfiguration configurationSource;

    private bool TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            this.wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            this.wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object
            this.configurationSource = new SystemBackdropConfiguration();
            this.window.Activated += Window_Activated;
            this.window.Closed += Window_Closed;
            ((FrameworkElement)this.window.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            this.configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            this.micaController = new MicaController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            this.micaController.AddSystemBackdropTarget(this.window.As<ICompositionSupportsSystemBackdrop>());
            this.micaController.SetSystemBackdropConfiguration(this.configurationSource);
            return true; // succeeded
        }

        return false; // Mica is not supported on this system
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.configurationSource.IsInputActive = args.WindowActivationState is not WindowActivationState.Deactivated;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (this.micaController is not null)
        {
            this.micaController.Dispose();
            this.micaController = null;
        }

        this.window.Activated -= Window_Activated;
        this.configurationSource = null;
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (this.configurationSource is not null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme()
    {
        this.configurationSource.Theme = ((FrameworkElement)this.window.Content).ActualTheme switch
        {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            ElementTheme.Default or _ => SystemBackdropTheme.Default
        };
    }
}

internal class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

    object m_dispatcherQueueController = null;
    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() is not null)
        {
            // one already exists, so we'll just use it.
            return;
        }

        if (m_dispatcherQueueController is null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }
}
