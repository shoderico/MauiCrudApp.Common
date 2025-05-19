using MauiCrudApp.Common.Interfaces;

namespace MauiCrudApp.Common.Services;

public class DialogService : IDialogService
{
    public async Task DisplayAlert(string title, string message, string cancel)
    {
        var currentPage = CurrentPage;
        if (currentPage != null)
        {
            await currentPage.DisplayAlert(title, message, cancel);
        }
    }

    public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
    {
        var currentPage = CurrentPage;
        if (currentPage != null)
        {
            return await currentPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }

    private Page? CurrentPage
    {
        get
        {
            // return Shell.Current?.CurrentPage; // On Android, doesn't work before page has been appeared
            return Shell.Current;
        }
    }

}
