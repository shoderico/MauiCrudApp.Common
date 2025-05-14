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

            // Explicitly check if the parameter can be cast to TParameter
            if (parameter != null && !typeof(TParameter).IsAssignableFrom(parameter.GetType()))
            {
                throw new InvalidCastException($"Cannot cast stored parameter of type {parameter.GetType().Name} to {typeof(TParameter).Name}.");
            }
            return (TParameter)parameter;
        }
        throw new InvalidOperationException($"No parameter of type {typeof(TParameter).Name} found.");
    }
}