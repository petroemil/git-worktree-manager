namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;
using GitWorktreeManager.Services;

internal static class CommandHelper
{
    private static readonly IDialogService dialogService = new DialogService();

    private static async Task CommandWrapper(Func<Task> asyncFunc)
    {
        try
        {
            await asyncFunc();
        }
        catch (Exception e)
        {
            await dialogService.ShowErrorAsync(e);
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
            await dialogService.ShowErrorAsync(e);
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
