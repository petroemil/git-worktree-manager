namespace GitWorktreeManager.Behaviors;

internal sealed class NullToBoolConverter : NullConverter<bool>
{
    protected override bool NullValue => false;
    protected override bool NotNullValue => true;
}
