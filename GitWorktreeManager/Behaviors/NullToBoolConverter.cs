namespace GitWorktreeManager.Behaviors;

internal sealed partial class NullToBoolConverter : NullConverter<bool>
{
    protected override bool NullValue => false;
    protected override bool NotNullValue => true;
}
