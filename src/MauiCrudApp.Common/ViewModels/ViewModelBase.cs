using CommunityToolkit.Mvvm.ComponentModel;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Navigation;

namespace MauiCrudApp.Common.ViewModels;

public abstract class ViewModelBase<TParameter> : ObservableObject, IInitialize
{
    private readonly INavigationParameterStore _parameterStore;
    private readonly TParameter _parameter;

    private bool _isInitialized = false;

    protected ViewModelBase(INavigationParameterStore parameterStore)
    {
        _parameterStore = parameterStore ?? throw new ArgumentNullException(nameof(parameterStore));

        if (_parameterStore.HasParameter<TParameter>())
            _parameter = _parameterStore.PopParameter<TParameter>();
    }

    public async Task PerformInitializeAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync(_parameter);
            _isInitialized = true;
        }
    }

    public abstract Task InitializeAsync(TParameter parameter);

}
