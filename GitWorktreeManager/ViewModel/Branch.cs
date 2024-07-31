namespace GitWorktreeManager.ViewModel;

using System.Windows.Input;

internal abstract class Branch
{
    public required string Name { get; init; }
    public string DisplayName => Name.Replace("/", " / ");

    public abstract string Label { get; }

    public string CreateWorktreeFromBranchLabel => $"Create new branch and set up worktree based on '{DisplayName}'";
    public required ICommand CreateWorktreeFromBranchCommand { get; init; }
}

internal abstract class BranchWithWorktree : Branch
{
    public required string Path { get; init; }

    public string OpenFileExplorerLabel => "Open in File Explorer";
    public required ICommand OpenFolderCommand { get; init; }

    public string OpenTerminalLabel => "Open in Terminal";
    public required ICommand OpenTerminalCommand { get; init; }

    public string OpenVisualStudioCodeLabel => "Open in Visual Studio Code";
    public required ICommand OpenVisualStudioCodeCommand { get; init; }

    public string OpenVisualStudioLabel => "Open in Visual Studio";
    public required ICommand OpenVisualStudioCommand { get; init; }
}

internal sealed class HeadBranchWithWorktree : BranchWithWorktree
{
    public override string Label => "HEAD branch";
}

internal sealed class LocalBranchWithWorktree : BranchWithWorktree
{
    public override string Label => "Local branch with worktree";

    public string RemoveLabel => "Remove worktree";
    public required ICommand RemoveCommand { get; init; }
}

internal abstract class BranchWithoutWorktree : Branch
{
    public string CreateWorktreeForBranchLabel => $"Set up worktree for '{DisplayName}'";
    public required ICommand CreateWorktreeForBranchCommand { get; init; }
}

internal sealed class LocalBranchWithoutWorktree : BranchWithoutWorktree
{
    public override string Label => "Local branch";
}

internal sealed class RemoteBranchWithoutWorktree : BranchWithoutWorktree
{
    public override string Label => "Remote branch";
}