namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Xaml;

internal class NullToVisibilityConverter : NullConverter<Visibility>
{
    protected override Visibility NullValue => Visibility.Collapsed;
    protected override Visibility NotNullValue => Visibility.Visible;
}
