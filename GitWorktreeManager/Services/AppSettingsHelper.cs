namespace GitWorktreeManager.Services;

using GitWorktreeManager.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;
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

    public static async Task<RepoInfo[]> GetRecentlyOpenedRepos()
    {
        return await Task.Run(() => RecentlyOpenedRepos
            .Select(static x => new RepoInfo(x))
            .ToArray());

    }

    public static async Task SaveRecentlyOpenedRepos(RepoInfo[] repos)
    {
        await Task.Run(() => RecentlyOpenedRepos = repos
            .Select(static x => x.Path)
            .ToArray());
    }
}
