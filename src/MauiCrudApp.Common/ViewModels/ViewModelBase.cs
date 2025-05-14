using CommunityToolkit.Mvvm.ComponentModel;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Navigation;

namespace MauiCrudApp.Common.ViewModels;

public abstract class ViewModelBase<TParameter> : ObservableObject, IInitialize
{
    private readonly INavigationParameterStore _parameterStore;
    private readonly TParameter _parameter;

    protected ViewModelBase(INavigationParameterStore parameterStore)
    {
        _parameterStore = parameterStore ?? throw new ArgumentNullException(nameof(parameterStore));
        _parameter = _parameterStore.PopParameter<TParameter>();
    }

    public async Task PerformInitializeAsync()
    {
        await InitializeAsync(_parameter);
    }

    public abstract Task InitializeAsync(TParameter parameter);

}
