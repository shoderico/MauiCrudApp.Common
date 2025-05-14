namespace MauiCrudApp.Common.Navigation;

public class NavigationParameterStore : INavigationParameterStore
{
    private readonly Dictionary<Type, object> _parameters = new();

    public void PushParameter<TParameter>(TParameter parameter)
    {
        _parameters[typeof(TParameter)] = parameter;
    }

    public TParameter PopParameter<TParameter>()
    {
        if (_parameters.TryGetValue(typeof(TParameter), out var parameter))
        {
            _parameters.Remove(typeof(TParameter));
            return (TParameter)parameter;
        }

        return default;
    }
}