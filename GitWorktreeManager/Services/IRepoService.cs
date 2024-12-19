using System.Threading.Tasks;

namespace GitWorktreeManager.Services;

internal interface IRepoService
{
    /// <inheritdoc cref="GitApi.Fetch" />
    Task Fetch();

    /// <inheritdoc cref="GitApi.ListBranchesAsync" />
    Task<ListBranchesResult> ListBranchesAsync();

    /// <inheritdoc cref="GitApi.AddWorktreeForLocalBranch" />
    Task AddWorktreeForLocalBranch(string branch);

    /// <inheritdoc cref="GitApi.AddWorktreeForRemoteBranch" />
    Task AddWorktreeForRemoteBranch(string branch);

    /// <inheritdoc cref="GitApi.AddWorktreeBasedOnLocalBranch" />
    Task AddWorktreeBasedOnLocalBranch(string newBranch, string baseBranch);

    /// <inheritdoc cref="GitApi.AddWorktreeBasedOnRemoteBranch" />
    Task AddWorktreeBasedOnRemoteBranch(string newBranch, string baseBranch);

    /// <inheritdoc cref="GitApi.RemoveWorktree" />
    Task RemoveWorktree(string branch);

    Task OpenFolder(string path);
    Task OpenTerminal(string path);
    Task OpenVisualStudioCode(string path);
    Task OpenVisualStudio(string solutionFilePath);
}
