namespace MauiCrudApp.Common.Navigation;

public interface INavigationParameterStore
{
    void PushParameter<TParameter>(TParameter parameter);
    TParameter PopParameter<TParameter>();
}