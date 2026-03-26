using System.Windows.Input;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GitWorktreeManager.Behaviors;

internal sealed class TabViewCloseTabCommandBehavior : BehaviorBase<TabView>
{
    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(TabViewCloseTabCommandBehavior), null);

    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.TabCloseRequested -= OnTabCloseRequested;
        AssociatedObject.TabCloseRequested += OnTabCloseRequested;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.TabCloseRequested -= OnTabCloseRequested;
    }

    private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        CloseCommand.Execute(args.Item);
    }
}