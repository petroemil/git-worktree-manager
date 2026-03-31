namespace GitWorktreeManager.ViewModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

internal abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    public partial ErrorInfo? Error { get; protected set; }

    [RelayCommand]
    private void DismissError()
    {
        this.Error = null;
    }
}
