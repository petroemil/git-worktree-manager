using System;
using System.Threading.Tasks;

namespace GitWorktreeManager.Services.Abstractions;

internal interface IDialogService
{
    Task ShowErrorAsync(Exception exception);
    Task<string?> ShowNewBranchDialogAsync(string baseBranch);
    Task<string?> OpenFolderAsync();
}
