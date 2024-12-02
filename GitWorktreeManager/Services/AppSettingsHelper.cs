namespace GitWorktreeManager.Services;

using GitWorktreeManager.ViewModel;
using System;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.Storage;

internal static class AppSettingsHelper
{
    private static IPropertySet AppSettings => ApplicationData.Current.LocalSettings.Values;

    private static string[] RecentlyOpenedRepos
    {
        get => (AppSettings[nameof(RecentlyOpenedRepos)] as string)?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? [];
        set => AppSettings[nameof(RecentlyOpenedRepos)] = string.Join(",", value);
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
