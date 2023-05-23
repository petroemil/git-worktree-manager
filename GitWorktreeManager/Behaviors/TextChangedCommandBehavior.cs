namespace GitWorktreeManager.Behaviors;

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

public static class TextChangedCommandBehavior
{
    public static IRelayCommand<string> GetTextChangedCommand(DependencyObject target)
        => (IRelayCommand<string>)target.GetValue(TextChangedCommandProperty);

    public static void SetTextChangedCommand(DependencyObject target, IRelayCommand<string> value)
        => target.SetValue(TextChangedCommandProperty, value);

    public static readonly DependencyProperty TextChangedCommandProperty =
        DependencyProperty.RegisterAttached(
            "TextChangedCommand",
            typeof(IRelayCommand<string>),
            typeof(TextChangedCommandBehavior),
            new PropertyMetadata(null, OnEnterKeyCommandChanged));

    static void OnEnterKeyCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not ICommand command || target is not TextBox textBox)
        {
            return;
        }

        command.CanExecuteChanged += (s, e) => textBox.IsEnabled = command.CanExecute(textBox.Text);

        textBox.TextChanged += (s, e) =>
        {
            command.Execute(textBox.Text);
        };
    }
}
