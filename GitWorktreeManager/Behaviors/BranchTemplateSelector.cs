﻿namespace GitWorktreeManager.Behaviors;

using GitWorktreeManager.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

internal sealed partial class BranchTemplateSelector : DataTemplateSelector
{
    public DataTemplate? LocalHeadBranchTemplate { get; set; }
    public DataTemplate? LocalBranchWithWorktreeTemplate { get; set; }
    public DataTemplate? LocalBranchTemplate { get; set; }
    public DataTemplate? RemoteBranchTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            HeadBranchWithWorktreeViewModel => LocalHeadBranchTemplate,
            LocalBranchWithWorktreeViewModel => LocalBranchWithWorktreeTemplate,
            LocalBranchWithoutWorktreeViewModel => LocalBranchTemplate,
            RemoteBranchWithoutWorktreeViewModel => RemoteBranchTemplate,
            _ => null
        };
    }
}
