namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

internal abstract class BranchViewModel : IEquatable<BranchViewModel?>
{
    protected readonly RepoViewModel repoVm;
    protected readonly IRepoService repoService;
    protected readonly IDialogService dialogService;

    public abstract string Label { get; }

    public required string Name { get; init; }
    public string DisplayName => this.Name.Replace("/", " / ");

    public string AheadBehindCommitsLabel => $"{this.Ahead} outgoing, {this.Behind} incoming commits";
    public string AheadBehindCommitsDisplay => $"{this.Ahead}/{this.Behind}";
    public required uint Ahead { get; init; }
    public required uint Behind { get; init; }

    public string CreateWorktreeFromBranchLabel => $"Create new branch and set up worktree based on '{this.DisplayName}'";
    public IAsyncRelayCommand CreateWorktreeFromBranchCommand { get; }

    public BranchViewModel(RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
    {
        this.repoVm = repoVm;
        this.repoService = repoService;
        this.dialogService = dialogService;

        this.CreateWorktreeFromBranchCommand = CommandHelper.CreateCommand(this.CreateWorktreeFromBranch);
    }

    protected virtual async Task CreateWorktreeFromBranch()
    {
        var newBranchName = await this.dialogService.ShowNewBranchDialogAsync(this.Name);

        if (string.IsNullOrWhiteSpace(newBranchName))
        {
            return;
        }

        await this.repoService.AddWorktreeBasedOnLocalBranch(newBranchName, this.Name);
        await this.repoVm.Refresh();
    }

    public override bool Equals(object? obj)
        => Equals(obj as BranchViewModel);

    public bool Equals(BranchViewModel? other)
        => other is not null
            && this.Name == other.Name
            && this.Ahead == other.Ahead
            && this.Behind == other.Behind;

    public override int GetHashCode()
        => HashCode.Combine(this.Name, this.Ahead, this.Behind);

    public static bool operator ==(BranchViewModel? left, BranchViewModel? right)
        => EqualityComparer<BranchViewModel>.Default.Equals(left, right);

    public static bool operator !=(BranchViewModel? left, BranchViewModel? right)
        => !(left == right);
}

internal abstract class BranchWithWorktreeViewModel : BranchViewModel, IEquatable<BranchWithWorktreeViewModel?>
{
    public required string Path { get; init; }

    public string OpenFileExplorerLabel => "Open in File Explorer";
    public IAsyncRelayCommand OpenFolderCommand { get; }

    public string OpenTerminalLabel => "Open in Terminal";
    public IAsyncRelayCommand OpenTerminalCommand { get; }

    public string OpenVisualStudioCodeLabel => "Open in Visual Studio Code";
    public IAsyncRelayCommand OpenVisualStudioCodeCommand { get; }

    public string OpenVisualStudioLabel => "Open in Visual Studio";
    public IAsyncRelayCommand OpenVisualStudioCommand { get; }

    protected BranchWithWorktreeViewModel(RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
        : base(repoVm, repoService, dialogService)
    {
        this.OpenFolderCommand = CommandHelper.CreateCommand(this.OpenFolder);
        this.OpenTerminalCommand = CommandHelper.CreateCommand(this.OpenTerminal);
        this.OpenVisualStudioCodeCommand = CommandHelper.CreateCommand(this.OpenVisualStudioCode);
        this.OpenVisualStudioCommand = CommandHelper.CreateCommand(this.OpenVisualStudio);
    }

    private async Task OpenFolder()
    {
        await this.repoService.OpenFolder(this.Path);
    }

    private async Task OpenTerminal()
    {
        await this.repoService.OpenTerminal(this.Path);
    }

    private async Task OpenVisualStudioCode()
    {
        await this.repoService.OpenVisualStudioCode(this.Path);
    }

    private async Task OpenVisualStudio()
    {
        await this.repoService.OpenVisualStudio(this.Path);
    }

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

    public HeadBranchWithWorktreeViewModel(RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
        : base(repoVm, repoService, dialogService)
    {
    }

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
    public IAsyncRelayCommand RemoveCommand { get; }

    public LocalBranchWithWorktreeViewModel(RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
    : base(repoVm, repoService, dialogService)
    {
        this.RemoveCommand = CommandHelper.CreateCommand(this.Remove);
    }

    private async Task Remove()
    {
        await this.repoService.RemoveWorktree(this.Path);
        await this.repoVm.Refresh();
    }

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
    public string CreateWorktreeForBranchLabel => $"Set up worktree for '{this.DisplayName}'";
    public IAsyncRelayCommand CreateWorktreeForBranchCommand { get; }

    public BranchWithoutWorktreeViewModel(RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
        : base(repoVm, repoService, dialogService)
    {
        CreateWorktreeForBranchCommand = CommandHelper.CreateCommand(this.CreateWorktreeForBranch);
    }

    protected abstract Task CreateWorktreeForBranch();

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

    public LocalBranchWithoutWorktreeViewModel(RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
        : base(repoVm, repoService, dialogService)
    {
    }

    protected override async Task CreateWorktreeForBranch()
    {
        await this.repoService.AddWorktreeForLocalBranch(this.Name);
        await this.repoVm.Refresh();
    }

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
    public RemoteBranchWithoutWorktreeViewModel(RepoViewModel repoVm, IRepoService repoService, IDialogService dialogService)
        : base(repoVm, repoService, dialogService)
    {
    }

    public override string Label => "Remote branch";

    protected override async Task CreateWorktreeForBranch()
    {
        await this.repoService.AddWorktreeForRemoteBranch(this.Name);
        await this.repoVm.Refresh();
    }

    protected override async Task CreateWorktreeFromBranch()
    {
        var newBranchName = await this.dialogService.ShowNewBranchDialogAsync(this.Name);

        if (string.IsNullOrWhiteSpace(newBranchName))
        {
            return;
        }

        await this.repoService.AddWorktreeBasedOnRemoteBranch(newBranchName, this.Name);
        await this.repoVm.Refresh();
    }

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