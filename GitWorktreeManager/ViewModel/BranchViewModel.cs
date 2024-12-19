namespace GitWorktreeManager.ViewModel;

using System;
using System.Collections.Generic;
using System.Windows.Input;

internal abstract class BranchViewModel : IEquatable<BranchViewModel?>
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
        => Equals(obj as BranchViewModel);

    public bool Equals(BranchViewModel? other)
        => other is not null
            && this.Name == other.Name
            && this.Ahead == other.Ahead
            && this.Behind == other.Behind;

    public override int GetHashCode()
        => HashCode.Combine(Name, Ahead, Behind);

    public static bool operator ==(BranchViewModel? left, BranchViewModel? right)
        => EqualityComparer<BranchViewModel>.Default.Equals(left, right);

    public static bool operator !=(BranchViewModel? left, BranchViewModel? right)
        => !(left == right);
}

internal abstract class BranchWithWorktreeViewModel : BranchViewModel, IEquatable<BranchWithWorktreeViewModel?>
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
        => this.Equals(obj as BranchWithWorktreeViewModel);

    public bool Equals(BranchWithWorktreeViewModel? other) 
        => base.Equals(other) && this.Path == other.Path;

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), this.Path);

    public static bool operator ==(BranchWithWorktreeViewModel? left, BranchWithWorktreeViewModel? right) 
        => EqualityComparer<BranchWithWorktreeViewModel>.Default.Equals(left, right);

    public static bool operator !=(BranchWithWorktreeViewModel? left, BranchWithWorktreeViewModel? right) 
        => !(left == right);
}

internal sealed class HeadBranchWithWorktreeViewModel : BranchWithWorktreeViewModel, IEquatable<HeadBranchWithWorktreeViewModel?>
{
    public override string Label => "HEAD branch";

    public override bool Equals(object? obj)
        => this.Equals(obj as HeadBranchWithWorktreeViewModel);

    public bool Equals(HeadBranchWithWorktreeViewModel? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(HeadBranchWithWorktreeViewModel? left, HeadBranchWithWorktreeViewModel? right)
        => EqualityComparer<HeadBranchWithWorktreeViewModel>.Default.Equals(left, right);

    public static bool operator !=(HeadBranchWithWorktreeViewModel? left, HeadBranchWithWorktreeViewModel? right)
        => !(left == right);
}

internal sealed class LocalBranchWithWorktreeViewModel : BranchWithWorktreeViewModel, IEquatable<LocalBranchWithWorktreeViewModel?>
{
    public override string Label => "Local branch with worktree";

    public string RemoveLabel => "Remove worktree";
    public required ICommand RemoveCommand { get; init; }

    public override bool Equals(object? obj)
        => this.Equals(obj as LocalBranchWithWorktreeViewModel);

    public bool Equals(LocalBranchWithWorktreeViewModel? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(LocalBranchWithWorktreeViewModel? left, LocalBranchWithWorktreeViewModel? right)
        => EqualityComparer<LocalBranchWithWorktreeViewModel>.Default.Equals(left, right);

    public static bool operator !=(LocalBranchWithWorktreeViewModel? left, LocalBranchWithWorktreeViewModel? right)
        => !(left == right);
}

internal abstract class BranchWithoutWorktreeViewModel : BranchViewModel, IEquatable<BranchWithoutWorktreeViewModel?>
{
    public string CreateWorktreeForBranchLabel => $"Set up worktree for '{DisplayName}'";
    public required ICommand CreateWorktreeForBranchCommand { get; init; }

    public override bool Equals(object? obj) 
        => this.Equals(obj as BranchWithoutWorktreeViewModel);

    public bool Equals(BranchWithoutWorktreeViewModel? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(BranchWithoutWorktreeViewModel? left, BranchWithoutWorktreeViewModel? right) 
        => EqualityComparer<BranchWithoutWorktreeViewModel>.Default.Equals(left, right);

    public static bool operator !=(BranchWithoutWorktreeViewModel? left, BranchWithoutWorktreeViewModel? right)
        => !(left == right);
}

internal sealed class LocalBranchWithoutWorktreeViewModel : BranchWithoutWorktreeViewModel, IEquatable<LocalBranchWithoutWorktreeViewModel?>
{
    public override string Label => "Local branch";

    public override bool Equals(object? obj) 
        => this.Equals(obj as LocalBranchWithoutWorktreeViewModel);

    public bool Equals(LocalBranchWithoutWorktreeViewModel? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(LocalBranchWithoutWorktreeViewModel? left, LocalBranchWithoutWorktreeViewModel? right) 
        => EqualityComparer<LocalBranchWithoutWorktreeViewModel>.Default.Equals(left, right);

    public static bool operator !=(LocalBranchWithoutWorktreeViewModel? left, LocalBranchWithoutWorktreeViewModel? right)
        => !(left == right);
}

internal sealed class RemoteBranchWithoutWorktreeViewModel : BranchWithoutWorktreeViewModel, IEquatable<RemoteBranchWithoutWorktreeViewModel?>
{
    public override string Label => "Remote branch";

    public override bool Equals(object? obj) 
        => this.Equals(obj as RemoteBranchWithoutWorktreeViewModel);

    public bool Equals(RemoteBranchWithoutWorktreeViewModel? other)
        => base.Equals(other);

    public override int GetHashCode() 
        => base.GetHashCode();

    public static bool operator ==(RemoteBranchWithoutWorktreeViewModel? left, RemoteBranchWithoutWorktreeViewModel? right) 
        => EqualityComparer<RemoteBranchWithoutWorktreeViewModel>.Default.Equals(left, right);

    public static bool operator !=(RemoteBranchWithoutWorktreeViewModel? left, RemoteBranchWithoutWorktreeViewModel? right)
        => !(left == right);
}