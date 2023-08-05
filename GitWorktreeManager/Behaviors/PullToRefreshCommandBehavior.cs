namespace GitWorktreeManager.Behaviors;

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Windows.Input;

internal sealed class PullToRefreshCommandBehavior
{
    public static ICommand GetRefreshCommand(DependencyObject target)
        => (ICommand)target.GetValue(RefreshCommandProperty);

    public static void SetRefreshCommand(DependencyObject target, ICommand value)
        => target.SetValue(RefreshCommandProperty, value);

    public static readonly DependencyProperty RefreshCommandProperty =
        DependencyProperty.RegisterAttached(
            "RefreshCommand",
            typeof(ICommand),
            typeof(PullToRefreshCommandBehavior),
            new PropertyMetadata(null, OnRefreshCommandChanged));

    static void OnRefreshCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not ICommand command || target is not RefreshContainer refreshContainer)
        {
            return;
        }

        refreshContainer.RefreshRequested += async (_, args) =>
        {
            if (command is IAsyncRelayCommand asyncCommand)
            {
                using (args.GetDeferral())
                {
                    await asyncCommand.ExecuteAsync(null);
                }
            }
            else 
            {
                command.Execute(null);
            }
        };
    }
}
