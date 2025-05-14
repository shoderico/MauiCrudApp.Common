using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.ViewModels;

using $defaultnamespace$.Core.Models;
using $defaultnamespace$.Core.Services;


namespace $rootnamespace$.$fileinputname$.ViewModels;

public partial class $fileinputname$ViewModel : ViewModelBase<$fileinputname$Parameter>
{
    private readonly INavigationService _navigationService;
    //private readonly IItemService _itemService;

    public $fileinputname$ViewModel(INavigationParameterStore parameterStore, INavigationService navigationService
        //, IItemService itemService
        ) : base(parameterStore)
    {
        _navigationService = navigationService;
        //_itemService = itemService;
    }

    public override Task InitializeAsync($fileinputname$Parameter parameter)
    {
        throw new NotImplementedException();
    }
}

