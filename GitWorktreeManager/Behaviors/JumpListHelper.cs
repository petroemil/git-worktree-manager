using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace GitWorktreeManager.Behaviors;

internal static class JumpListHelper
{
    const string RepoArgsPrefix = "repo=";

    public static bool TryGetLaunchArgs(out string repoPath)
    {
        var args = Environment.GetCommandLineArgs();
        var jumpListArgs = args.FirstOrDefault(arg => arg.StartsWith(RepoArgsPrefix));
        if (jumpListArgs is not null)
        {
            repoPath = jumpListArgs[RepoArgsPrefix.Length..];
            return true;
        }

        repoPath = null;
        return false;
    }

    public static async Task AddItem(string path)
    {
        var jumpList = await JumpList.LoadCurrentAsync();

        if (jumpList.SystemGroupKind is JumpListSystemGroupKind.Recent or JumpListSystemGroupKind.Frequent)
        {
            var displayName = Path.GetFileName(path);
            var item = JumpListItem.CreateWithArguments($"{RepoArgsPrefix}{path}", displayName);
            item.GroupName = "Recent";
            item.Logo = new Uri("ms-appx:///Assets/StoreAppList.png");
            
            // Add to the end
            jumpList.Items.Add(item);

            // Remove from the front
            if (jumpList.Items.Count > 5)
            {
                jumpList.Items.RemoveAt(0);
            }
        }

        await jumpList.SaveAsync();
    }
}
