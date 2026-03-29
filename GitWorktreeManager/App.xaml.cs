namespace GitWorktreeManager;

using GitWorktreeManager.ViewModel;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Linq;

public sealed partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Handle command line activation.
        // First argument is the path of the executable, so we need to get the second one
        var cmdArgs = Environment.GetCommandLineArgs().ElementAtOrDefault(1);
        if (!string.IsNullOrWhiteSpace(cmdArgs))
        {
            var path = Path.GetFullPath(cmdArgs);
            var repoInfo = new RepoInfo(path);
            MainViewModel.Instance.OpenRepoCommand.Execute(repoInfo);
        }

        MainWindow.Instance.Activate();
    }
}