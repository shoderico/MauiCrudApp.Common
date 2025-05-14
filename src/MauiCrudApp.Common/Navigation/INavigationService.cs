namespace MauiCrudApp.Common.Navigation;

public interface INavigationService
{
    Task PushAsync<TParameter>(Type viewType, TParameter parameter = default);
    Task GoToAsync<TParameter>(Type viewType, TParameter parameter = default);
    Task GoBackAsync();
}