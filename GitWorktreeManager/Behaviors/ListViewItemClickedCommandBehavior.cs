namespace GitWorktreeManager.Behaviors;

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Windows.Input;

internal sealed class ListViewItemClickedCommandBehavior
{
    public static IRelayCommand GetClickedCommand(DependencyObject target)
        => (IRelayCommand)target.GetValue(ClickedCommandProperty);

    public static void SetClickedCommand(DependencyObject target, IRelayCommand value)
        => target.SetValue(ClickedCommandProperty, value);

    public static readonly DependencyProperty ClickedCommandProperty =
        DependencyProperty.RegisterAttached(
            "ClickedCommand",
            typeof(IRelayCommand),
            typeof(ListViewItemClickedCommandBehavior),
            new PropertyMetadata(null, OnClickedCommandPropertyChanged));

    static void OnClickedCommandPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not ICommand command || target is not ListViewBase listView)
        {
            return;
        }

        listView.ItemClick += (s, e) => command.Execute(e.ClickedItem);
    }
}
