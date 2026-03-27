using CommunityToolkit.Mvvm.Input;
using GitWorktreeManager.Services;
using System;
using System.Threading.Tasks;

namespace GitWorktreeManager.ViewModel;

partial class RepoViewModel
{
    private async Task CommandWrapper(Func<Task> asyncFunc)
    {
        try
        {
            await asyncFunc();
        }
        catch (Exception e)
        {
            SetError(e);
        }
    }

    private Task CommandWrapper(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            SetError(e);
        }

        return Task.CompletedTask;
    }

    private void SetError(Exception exception)
    {
        if (exception is GitException gitException)
        {
            Error = new ErrorInfo
            {
                Title = $"Git Error ({gitException.ExitCode})",
                Description = $"git {gitException.Command}"
                + Environment.NewLine
                + Environment.NewLine
                + gitException.Error
            };
        }
        else
        {
            Error = new ErrorInfo
            {
                Title = exception.GetType().Name,
                Description = exception.Message
            };
        }
    }

    public AsyncRelayCommand CreateCommand(Func<Task> asyncFunc)
        => new(() => CommandWrapper(asyncFunc));

    public AsyncRelayCommand<T> CreateCommand<T>(Func<T, Task> asyncFunc)
        => new(arg => CommandWrapper(() => asyncFunc(arg)));

    public AsyncRelayCommand CreateCommand(Action action)
        => new(() => CommandWrapper(action));

    public AsyncRelayCommand<T> CreateCommand<T>(Action<T> action)
        => new(arg => CommandWrapper(() => action(arg)));

}
