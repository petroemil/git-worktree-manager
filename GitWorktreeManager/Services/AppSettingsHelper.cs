namespace GitWorktreeManager.Services;

using GitWorktreeManager.ViewModel;
using System;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.Storage;

public static class AppSettingsHelper
{
    private static IPropertySet AppSettings => ApplicationData.Current.LocalSettings.Values;

    private static string[] RecentlyOpenedRepos
    {
        get => AppSettings.TryGetValue(nameof(RecentlyOpenedRepos), out var value) && value is string[] array
            ? array
            : Array.Empty<string>();
        set => AppSettings[nameof(RecentlyOpenedRepos)] = value;
    }

    public static RepoInfo[] GetRecentlyOpenedRepos()
    {
        return RecentlyOpenedRepos
            .Select(x => new RepoInfo(x))
            .ToArray();
    }

    public static void SaveRecentlyOpenedRepo(RepoInfo repoInfo)
    {
        RecentlyOpenedRepos = RecentlyOpenedRepos
            .Prepend(repoInfo.Path)
            .Take(5)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
