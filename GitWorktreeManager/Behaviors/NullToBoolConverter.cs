namespace GitWorktreeManager.Behaviors;

using Microsoft.UI.Xaml.Data;
using System;

internal sealed class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is not null;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
