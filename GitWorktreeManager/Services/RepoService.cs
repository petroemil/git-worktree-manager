using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace GitWorktreeManager.Services;

internal sealed class RepoService : IRepoService
{
    private readonly GitApi gitApi;

    public RepoService(string workingDir)
    {
        this.gitApi = new(workingDir);
    }

    public async Task Fetch()
    {
        await this.gitApi.Fetch();
    }

    public async Task AddWorktreeBasedOnLocalBranch(string newBranch, string baseBranch)
    {
        await this.gitApi.AddWorktreeBasedOnLocalBranch(newBranch, baseBranch);
    }

    public async Task AddWorktreeBasedOnRemoteBranch(string newBranch, string baseBranch)
    {
        await this.gitApi.AddWorktreeBasedOnRemoteBranch(newBranch, baseBranch);
    }

    public async Task AddWorktreeForLocalBranch(string branch)
    {
        await this.gitApi.AddWorktreeForLocalBranch(branch);
    }

    public async Task AddWorktreeForRemoteBranch(string branch)
    {
        await this.gitApi.AddWorktreeForRemoteBranch(branch);
    }

    public async Task RemoveWorktree(string branch)
    {
        await this.gitApi.RemoveWorktree(branch);
    }

    public async Task<ListBranchesResult> ListBranchesAsync()
    {
        return await this.gitApi.ListBranchesAsync();
    }

    public async Task OpenFolder(string path)
    {
        await Launcher.LaunchFolderPathAsync(path);
    }

    public async Task OpenTerminal(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = false,
            FileName = "wt",
            Arguments = $"-d {path}"
        });
    }

    public async Task OpenVisualStudioCode(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "code",
            Arguments = ".",
            WorkingDirectory = path
        });
    }

    public async Task OpenVisualStudio(string path)
    {
        var sln = Directory.EnumerateFiles(path, "*.sln").FirstOrDefault();
        if (sln is not null)
        {
            await Launcher.LaunchUriAsync(new Uri(sln));
        }
    }
}
