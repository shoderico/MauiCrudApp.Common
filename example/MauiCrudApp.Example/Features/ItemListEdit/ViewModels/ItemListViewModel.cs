using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.ViewModels;

using MauiCrudApp.Example.Core.Models;
using MauiCrudApp.Example.Features.ItemListEdit.Views;
using MauiCrudApp.Example.Core.Interfaces;


namespace MauiCrudApp.Example.Features.ItemListEdit.ViewModels;

public partial class ItemListViewModel : ViewModelBase<ItemListParameter>
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IItemService _itemService;

    public ItemListViewModel(
          INavigationParameterStore parameterStore
        , IDialogService dialogService
        , INavigationService navigationService
        , IItemService itemService) : base(parameterStore)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _itemService = itemService;

        items = new ObservableCollection<Item>();
    }





    [ObservableProperty]
    private ObservableCollection<Item> items;

    [ObservableProperty]
    private string? searchText;

    public override async Task InitializeAsync(ItemListParameter parameter, bool isInitialized)
    {
        if (parameter != null)
        {
            SearchText = parameter.SearchText;
        }
        await LoadItemsAsync();
    }





    [RelayCommand]
    private async Task Search()
    {
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task EditItem(Item item)
    {
        try
        {
            var parameter = new ItemEditParameter { ItemId = item.Id, IsNew = false };
            await _navigationService.PushAsync(typeof(ItemEditPage), parameter);
        }
        catch (Exception ex)
        {
            await _dialogService.DisplayAlert("EditItem: Error", ex.Message, "OK");
            Console.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteItem(Item item)
    {
        await _itemService.DeleteItemAsync(item.Id);
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task AddItem()
    {
        try
        {
            var parameter = new ItemEditParameter { IsNew = true, ListItems = Items };
            await _navigationService.PushAsync(typeof(ItemEditPage), parameter);
        }
        catch (Exception ex)
        {
            await _dialogService.DisplayAlert("AddItem: Error", ex.Message, "OK");
            Console.WriteLine(ex.Message);
        }
    }


    private async Task LoadItemsAsync()
    {
        var items = await _itemService.GetItemsAsync(SearchText);
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }    
}
