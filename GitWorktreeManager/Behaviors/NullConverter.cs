using Microsoft.UI.Xaml.Data;
using System;

namespace GitWorktreeManager.Behaviors
{
    internal abstract class NullConverter<T> : IValueConverter
    {
        protected abstract T? NullValue { get; }
        protected abstract T? NotNullValue { get; }

        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            var isNull = value is null;
            var isInverted = parameter is "Invert";

            return isNull ^ isInverted ? NullValue : NotNullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) 
            => throw new NotImplementedException();
    }
}
