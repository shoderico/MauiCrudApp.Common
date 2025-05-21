using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.Utilities;
using MauiCrudApp.Common.ViewModels;
using MauiCrudApp.Example.Core.Interfaces;
using MauiCrudApp.Example.Core.Models;


namespace MauiCrudApp.Example.Features.ItemListEdit.ViewModels;

public partial class ItemEditViewModel : ViewModelBase<ItemEditParameter>, IEditableViewModel, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IItemService _itemService;

    private bool _disposed;

    public ItemEditViewModel(
          INavigationParameterStore parameterStore
        , INavigationService navigationService
        , IDialogService dialogService
        , IItemService itemService
        ) : base(parameterStore)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _itemService = itemService;

        _disposed = false;
        isNew = false;
    }





    private Item? _originalItem;

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private bool isNew;

    private Collection<Item>? _listItems;

    private ChangeTracker? _itemChangeTracker;

    public bool IsEditing
    {
        get
        {
           return _itemChangeTracker?.HasChanges ?? false;
        }
    }

    public override async Task InitializeAsync(ItemEditParameter parameter, bool isInitialized)
    {
        if (!isInitialized)
        {
            _listItems = parameter.ListItems;

            IsNew = parameter.IsNew;

            _originalItem = IsNew ? new Item()
                                  : await _itemService.GetItemByIdAsync(parameter.ItemId);

            Item = new Item()
            {
                Id = _originalItem?.Id ?? 0,
                Name = _originalItem?.Name ?? "",
                Description = _originalItem?.Description ?? ""
            };

            _itemChangeTracker = new ChangeTracker(typeof(Item), Item);
            _itemChangeTracker.TrackProperty(this, nameof(Item));
        }

        await Task.CompletedTask;
    }





    [RelayCommand]
    private async Task Save()
    {
        if (Item != null)
        {
            if (IsNew)
            {
                await _itemService.AddItemAsync(Item);
                _listItems?.Add(Item);
            }
            else
            {
                await _itemService.UpdateItemAsync(Item);
            }
        }

        _itemChangeTracker?.Save();
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        _itemChangeTracker?.Save();
        await _navigationService.GoBackAsync();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _itemChangeTracker?.Dispose();
        _disposed = true;
    }
}
