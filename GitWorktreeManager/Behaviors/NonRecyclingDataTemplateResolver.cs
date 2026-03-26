using System;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace GitWorktreeManager.Behaviors;

/// <summary>
/// This converter is a hacky workaround to side-step the built-in recycling of DataTemplates in a TabView.
/// We create and use an explicit set of views for each tab instead of recycling the same view across multiple tabs.
/// This is needed because recycling casuses all sorts of weird behavior with bindings and TreeView expansion states.
/// </summary>
internal sealed partial class NonRecyclingDataTemplateResolver : DependencyObject, IValueConverter
{
    public static readonly DependencyProperty DataTemplateProperty =
        DependencyProperty.Register(nameof(DataTemplate), typeof(DataTemplate), typeof(NonRecyclingDataTemplateResolver), new PropertyMetadata(null));

    private readonly ConditionalWeakTable<object, FrameworkElement> views = [];

    public DataTemplate DataTemplate
    {
        get => (DataTemplate)GetValue(DataTemplateProperty);
        set => SetValue(DataTemplateProperty, value);
    }

    /// <inheritdoc/>
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not null)
        {
            if (this.views.TryGetValue(value, out FrameworkElement? existingView) && existingView is not null)
            {
                return existingView;
            }
            else if (DataTemplate?.LoadContent() is FrameworkElement newView)
            {
                newView.DataContext = value;

                this.views.Add(value, newView);
                return newView;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}