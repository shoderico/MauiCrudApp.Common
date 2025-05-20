using CommunityToolkit.Mvvm.ComponentModel;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Navigation;

namespace MauiCrudApp.Common.ViewModels;

public abstract class ViewModelBase<TParameter> : ObservableObject, ILifecycle
{
    private readonly INavigationParameterStore _parameterStore;
    private readonly TParameter _parameter;

    private bool _isInitialized = false;
    private bool _isFinalized = false;

    protected ViewModelBase(INavigationParameterStore parameterStore)
    {
        _parameterStore = parameterStore ?? throw new ArgumentNullException(nameof(parameterStore));

        if (_parameterStore.HasParameter<TParameter>())
            _parameter = _parameterStore.PopParameter<TParameter>();
    }

    public async Task PerformInitializeAsync()
    {
        await InitializeAsync(_parameter, _isInitialized);
        _isInitialized = true;
    }

    public async Task PerformFinalizeAsync()
    {
        await FinalizeAsync(_isFinalized);
        _isFinalized = true;
    }

    public virtual Task InitializeAsync(TParameter parameter, bool isInitialized) { return Task.CompletedTask; }
    public virtual Task FinalizeAsync(bool isFinalized) {  return Task.CompletedTask; }

}
