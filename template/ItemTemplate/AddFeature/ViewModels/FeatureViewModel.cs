using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.ViewModels;

using $defaultnamespace$.Core.Models;
using $defaultnamespace$.Core.Services;


// Don't forget to register the ViewModel and Page with the service in MauiProgram.cs.
/* : MauiProgram.cs
...
public static class MauiProgram
{
    ...
   
    // Feature: $fileinputname$
    builder.Services.AddTransient<$rootnamespace$.ViewModels.$fileinputname$ViewModel>();
    builder.Services.AddTransient<$rootnamespace$.Views.$fileinputname$Page>();

    ...
*/

// If you want to add a FlyoutItem, refer to the following code.
/* : AppShell.xaml : Modify 'someViews' to suit your project.
<shell:ShellBase ...
    xmlns:someViews="clr-namespace:$rootnamespace$.Views"
    ...>
    ...
    
    <FlyoutItem Title="$fileinputname$">
        <ShellContent Route="$fileinputname$" ContentTemplate="{DataTemplate someViews:$fileinputname$Page}" />
    </FlyoutItem>

    ...
</shell:ShellBase>
*/

// If you want to navigate to this page, refer to the following code.
/*
    try
    {
        var parameter = new $fileinputname$Parameter { };
        await _navigationService.PushAsync(typeof($fileinputname$Page), parameter);
    }
    catch (Exception ex)
    {
        await _dialogService.DisplayAlert("Navigate: Error", ex.Message, "OK");
    }
*/

namespace $rootnamespace$.ViewModels;

public partial class $fileinputname$ViewModel : ViewModelBase<$fileinputname$Parameter>
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public $fileinputname$ViewModel(
	          INavigationParameterStore parameterStore
		    , INavigationService navigationService
		    , IDialogService dialogService
        ) : base(
            parameterStore
        )
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    public override Task InitializeAsync($fileinputname$Parameter parameter, bool isInitialized)
    {
        // This function is called from the Page's OnAppearing.
        // The parameter may be NULL.
        return Task.CompletedTask;
    }
    
    public override Task FinalizeAsync(bool isFinalized)
    {
        // This function is called from the Page's OnDisappearing.
        return Task.CompletedTask;
    }
	
	[RelayCommand]
	private Task Search()
	{
		return Task.CompletedTask;
	}
	
	[RelayCommand]
	private async Task Navigate()
	{
		try
		{
            // Implement [NavigateTo]ViewModel,Parameter,Page as needed.
			//var parameter = new NavigateToParameter { };
			//await _navigationService.PushAsync(typeof(NavigateToPage), parameter);
		}
		catch (Exception ex)
		{
			await _dialogService.DisplayAlert("Navigate: Error", ex.Message, "OK");
		}
	}
	
	[RelayCommand]
	private async Task Back()
	{
        try
        {
            await _navigationService.GoBackAsync();
        }
		catch (Exception ex)
		{
			await _dialogService.DisplayAlert("Back: Error", ex.Message, "OK");
		}
	}
}

