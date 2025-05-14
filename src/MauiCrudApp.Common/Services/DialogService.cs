using MauiCrudApp.Common.Interfaces;

namespace MauiCrudApp.Common.Services;

public class DialogService : IDialogService
{
    public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
    {
        var currentPage = Microsoft.Maui.Controls.Shell.Current?.CurrentPage;
        if (currentPage != null)
        {
            return await currentPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }
}
