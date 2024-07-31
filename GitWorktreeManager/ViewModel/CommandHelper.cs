namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Behaviors;
using System.Threading.Tasks;
using System;

internal static class CommandHelper
{
    private static async Task CommandWrapper(Func<Task> asyncFunc)
    {
        try
        {
            await asyncFunc();
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    private static async Task CommandWrapper(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            await DialogHelper.ShowErrorAsync(e);
        }
    }

    public static AsyncRelayCommand CreateCommand(Func<Task> asyncFunc)
        => new(() => CommandWrapper(asyncFunc));

    public static AsyncRelayCommand<T> CreateCommand<T>(Func<T, Task> asyncFunc)
        => new(arg => CommandWrapper(() => asyncFunc(arg)));

    public static AsyncRelayCommand CreateCommand(Action action)
        => new(() => CommandWrapper(action));

    public static AsyncRelayCommand<T> CreateCommand<T>(Action<T> action)
        => new(arg => CommandWrapper(() => action(arg)));
}
