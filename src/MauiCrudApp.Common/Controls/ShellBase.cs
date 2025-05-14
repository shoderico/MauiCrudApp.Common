using MauiCrudApp.Common.Interfaces;

namespace MauiCrudApp.Common.Controls;

public class ShellBase : Microsoft.Maui.Controls.Shell, IDisposable
{
    private readonly IDialogService _dialogService;
    private bool _disposed;

    public string DialogTitile { get; set; } = "";
    public string DialogMessage { get; set; } = "";
    public string DialogAccept { get; set; } = "";
    public string DialogCancel { get; set; } = "";


    public ShellBase(IDialogService dialogService)
    {
        _dialogService = dialogService;

        this.Navigating += OnNavigating;

        DialogTitile = "Warning";
        DialogMessage = "Changes have not been saved. Do you want to discard them?";
        DialogAccept = "Discard";
        DialogCancel = "Cancel";
    }

    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (_disposed)
            return;

        var deferral = e.GetDeferral();

        try
        {
            if (e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot)
            {
                var currentPage = Microsoft.Maui.Controls.Shell.Current?.CurrentPage;
                if (currentPage?.BindingContext is IEditableViewModel viewModel && viewModel.IsEditing)
                {
                    var result = await _dialogService.DisplayAlert(DialogTitile, DialogMessage, DialogAccept, DialogCancel);

                    if (!result)
                    {
                        e.Cancel();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            e.Cancel();
        }

        deferral?.Complete();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Navigating -= OnNavigating;
        _disposed = true;
    }
}
