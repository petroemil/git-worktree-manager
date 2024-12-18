namespace GitWorktreeManager.ViewModel;

using System;
using System.Collections.Generic;
using System.Windows.Input;

internal abstract class Branch : IEquatable<Branch?>
{
    public required string Name { get; init; }
    public string DisplayName => Name.Replace("/", " / ");

    public abstract string Label { get; }

    public string CreateWorktreeFromBranchLabel => $"Create new branch and set up worktree based on '{DisplayName}'";
    public required ICommand CreateWorktreeFromBranchCommand { get; init; }

    public required uint Ahead { get; init; }
    public required uint Behind { get; init; }

    public string AheadBehindCommitsLabel => $"{Ahead} outgoing, {Behind} incoming commits";
    public string AheadBehindCommitsDisplay => $"{Ahead}/{Behind}";

    public override bool Equals(object? obj) 
        => Equals(obj as Branch);

    public bool Equals(Branch? other) 
        => other is not null
            && this.Name == other.Name
            && this.Ahead == other.Ahead
            && this.Behind == other.Behind;

    public override int GetHashCode()
        => HashCode.Combine(Name, Ahead, Behind);

    public static bool operator ==(Branch? left, Branch? right)
        => EqualityComparer<Branch>.Default.Equals(left, right);

    public static bool operator !=(Branch? left, Branch? right)
        => !(left == right);
}

internal abstract class BranchWithWorktree : Branch, IEquatable<BranchWithWorktree?>
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

    public override bool Equals(object? obj)
        => this.Equals(obj as BranchWithWorktree);

    public bool Equals(BranchWithWorktree? other) 
        => base.Equals(other) && this.Path == other.Path;

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), this.Path);

    public static bool operator ==(BranchWithWorktree? left, BranchWithWorktree? right) 
        => EqualityComparer<BranchWithWorktree>.Default.Equals(left, right);

    public static bool operator !=(BranchWithWorktree? left, BranchWithWorktree? right) 
        => !(left == right);
}

internal sealed class HeadBranchWithWorktree : BranchWithWorktree, IEquatable<HeadBranchWithWorktree?>
{
    public override string Label => "HEAD branch";

    public override bool Equals(object? obj)
        => this.Equals(obj as HeadBranchWithWorktree);

    public bool Equals(HeadBranchWithWorktree? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(HeadBranchWithWorktree? left, HeadBranchWithWorktree? right)
        => EqualityComparer<HeadBranchWithWorktree>.Default.Equals(left, right);

    public static bool operator !=(HeadBranchWithWorktree? left, HeadBranchWithWorktree? right)
        => !(left == right);
}

internal sealed class LocalBranchWithWorktree : BranchWithWorktree, IEquatable<LocalBranchWithWorktree?>
{
    public override string Label => "Local branch with worktree";

    public string RemoveLabel => "Remove worktree";
    public required ICommand RemoveCommand { get; init; }

    public override bool Equals(object? obj)
        => this.Equals(obj as LocalBranchWithWorktree);

    public bool Equals(LocalBranchWithWorktree? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(LocalBranchWithWorktree? left, LocalBranchWithWorktree? right)
        => EqualityComparer<LocalBranchWithWorktree>.Default.Equals(left, right);

    public static bool operator !=(LocalBranchWithWorktree? left, LocalBranchWithWorktree? right)
        => !(left == right);
}

internal abstract class BranchWithoutWorktree : Branch, IEquatable<BranchWithoutWorktree?>
{
    public string CreateWorktreeForBranchLabel => $"Set up worktree for '{DisplayName}'";
    public required ICommand CreateWorktreeForBranchCommand { get; init; }

    public override bool Equals(object? obj) 
        => this.Equals(obj as BranchWithoutWorktree);

    public bool Equals(BranchWithoutWorktree? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(BranchWithoutWorktree? left, BranchWithoutWorktree? right) 
        => EqualityComparer<BranchWithoutWorktree>.Default.Equals(left, right);

    public static bool operator !=(BranchWithoutWorktree? left, BranchWithoutWorktree? right)
        => !(left == right);
}

internal sealed class LocalBranchWithoutWorktree : BranchWithoutWorktree, IEquatable<LocalBranchWithoutWorktree?>
{
    public override string Label => "Local branch";

    public override bool Equals(object? obj) 
        => this.Equals(obj as LocalBranchWithoutWorktree);

    public bool Equals(LocalBranchWithoutWorktree? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(LocalBranchWithoutWorktree? left, LocalBranchWithoutWorktree? right) 
        => EqualityComparer<LocalBranchWithoutWorktree>.Default.Equals(left, right);

    public static bool operator !=(LocalBranchWithoutWorktree? left, LocalBranchWithoutWorktree? right)
        => !(left == right);
}

internal sealed class RemoteBranchWithoutWorktree : BranchWithoutWorktree, IEquatable<RemoteBranchWithoutWorktree?>
{
    public override string Label => "Remote branch";

    public override bool Equals(object? obj) 
        => this.Equals(obj as RemoteBranchWithoutWorktree);

    public bool Equals(RemoteBranchWithoutWorktree? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(RemoteBranchWithoutWorktree? left, RemoteBranchWithoutWorktree? right) 
        => EqualityComparer<RemoteBranchWithoutWorktree>.Default.Equals(left, right);

    public static bool operator !=(RemoteBranchWithoutWorktree? left, RemoteBranchWithoutWorktree? right)
        => !(left == right);
}