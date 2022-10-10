namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;
using Windows.System;

public static class EnterKeyHelper
{
    public static ICommand GetEnterKeyCommand(DependencyObject target)
        => (ICommand)target.GetValue(EnterKeyCommandProperty);

    public static void SetEnterKeyCommand(DependencyObject target, ICommand value)
        => target.SetValue(EnterKeyCommandProperty, value);

    public static readonly DependencyProperty EnterKeyCommandProperty =
        DependencyProperty.RegisterAttached(
            "EnterKeyCommand",
            typeof(ICommand),
            typeof(EnterKeyHelper),
            new PropertyMetadata(null, OnEnterKeyCommandChanged));

    static void OnEnterKeyCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not ICommand command || target is not TextBox textBox)
        {
            return;
        }

        command.CanExecuteChanged += (s, e) => textBox.IsEnabled = command.CanExecute(textBox.Text);

        textBox.KeyDown += (s, e) =>
        {
            if (e.Key is VirtualKey.Enter)
            {
                // Make sure the textbox binding updates its source first
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();

                command.Execute(textBox.Text);
            }
        };
    }
}
