namespace MauiCrudApp.Common.Navigation;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly INavigationParameterStore _parameterStore;

    public NavigationService(IServiceProvider services, INavigationParameterStore parameterStore)
    {
        _services = services;
        _parameterStore = parameterStore;
    }


    public async Task PushAsync<TParameter>(Type viewType, TParameter parameter = default)
    {
        _parameterStore.PushParameter(parameter);

        var page = _services.GetService(viewType) as Page
            ?? throw new InvalidOperationException($"Page {viewType.Name} not registered in DI.");

        await Microsoft.Maui.Controls.Shell.Current.Navigation.PushAsync(page, true);
    }

    public async Task GoToAsync<TParameter>(Type viewType, TParameter parameter = default)
    {
        _parameterStore.PushParameter(parameter);

        var route = $"//{viewType.Name}";

        await Microsoft.Maui.Controls.Shell.Current.GoToAsync(route);
    }


    public async Task GoBackAsync()
    {
        await Microsoft.Maui.Controls.Shell.Current.Navigation.PopAsync();
    }
}