namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

internal sealed partial class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isVisible = value is true;

        if (parameter is "Invert")
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
