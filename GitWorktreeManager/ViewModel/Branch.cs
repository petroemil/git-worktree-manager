namespace GitWorktreeManager.ViewModel;

using System;
using System.Collections.Generic;
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

internal sealed class HeadBranchWithWorktree : BranchWithWorktree, IEquatable<HeadBranchWithWorktree?>
{
    public override string Label => "HEAD branch";

    public override bool Equals(object? obj) => Equals(obj as HeadBranchWithWorktree);
    public bool Equals(HeadBranchWithWorktree? other) 
        => other is not null
        && this.Name == other.Name
        && this.Path == other.Path;
    public override int GetHashCode() => HashCode.Combine(this.Name, this.Path);
    public static bool operator ==(HeadBranchWithWorktree? left, HeadBranchWithWorktree? right) => EqualityComparer<HeadBranchWithWorktree>.Default.Equals(left, right);
    public static bool operator !=(HeadBranchWithWorktree? left, HeadBranchWithWorktree? right) => !(left == right);
}

internal sealed class LocalBranchWithWorktree : BranchWithWorktree, IEquatable<LocalBranchWithWorktree?>
{
    public override string Label => "Local branch with worktree";

    public string RemoveLabel => "Remove worktree";
    public required ICommand RemoveCommand { get; init; }

    public override bool Equals(object? obj) => Equals(obj as LocalBranchWithWorktree);
    public bool Equals(LocalBranchWithWorktree? other) 
        => other is not null
        && this.Name == other.Name
        && this.Path == other.Path;
    public override int GetHashCode() => HashCode.Combine(this.Name, this.Path);
    public static bool operator ==(LocalBranchWithWorktree? left, LocalBranchWithWorktree? right) => EqualityComparer<LocalBranchWithWorktree>.Default.Equals(left, right);
    public static bool operator !=(LocalBranchWithWorktree? left, LocalBranchWithWorktree? right) => !(left == right);
}

internal abstract class BranchWithoutWorktree : Branch
{
    public string CreateWorktreeForBranchLabel => $"Set up worktree for '{DisplayName}'";
    public required ICommand CreateWorktreeForBranchCommand { get; init; }
}

internal sealed class LocalBranchWithoutWorktree : BranchWithoutWorktree, IEquatable<LocalBranchWithoutWorktree?>
{
    public override string Label => "Local branch";

    public override bool Equals(object? obj) => Equals(obj as LocalBranchWithoutWorktree);
    public bool Equals(LocalBranchWithoutWorktree? other) 
        => other is not null 
        && this.Name == other.Name;
    public override int GetHashCode() => HashCode.Combine(this.Name);
    public static bool operator ==(LocalBranchWithoutWorktree? left, LocalBranchWithoutWorktree? right) => EqualityComparer<LocalBranchWithoutWorktree>.Default.Equals(left, right);
    public static bool operator !=(LocalBranchWithoutWorktree? left, LocalBranchWithoutWorktree? right) => !(left == right);
}

internal sealed class RemoteBranchWithoutWorktree : BranchWithoutWorktree, IEquatable<RemoteBranchWithoutWorktree?>
{
    public override string Label => "Remote branch";

    public override bool Equals(object? obj) => Equals(obj as RemoteBranchWithoutWorktree);
    public bool Equals(RemoteBranchWithoutWorktree? other) 
        => other is not null 
        && this.Name == other.Name;
    public override int GetHashCode() => HashCode.Combine(this.Name);
    public static bool operator ==(RemoteBranchWithoutWorktree? left, RemoteBranchWithoutWorktree? right) => EqualityComparer<RemoteBranchWithoutWorktree>.Default.Equals(left, right);
    public static bool operator !=(RemoteBranchWithoutWorktree? left, RemoteBranchWithoutWorktree? right) => !(left == right);
}