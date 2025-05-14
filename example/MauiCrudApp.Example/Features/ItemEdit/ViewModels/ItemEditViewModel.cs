using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.Utilities;
using MauiCrudApp.Common.ViewModels;

using MauiCrudApp.Example.Core.Models;
using MauiCrudApp.Example.Core.Services;

namespace MauiCrudApp.Example.Features.ItemEdit.ViewModels;

public partial class ItemEditViewModel : ViewModelBase<ItemEditParameter>, IEditableViewModel, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly IItemService _itemService;

    private bool _disposed;

    public ItemEditViewModel(INavigationParameterStore parameterStore, INavigationService navigationService, IItemService itemService) : base(parameterStore)
    {
        _navigationService = navigationService;
        _itemService = itemService;
    }





    private Item _originalItem;

    [ObservableProperty]
    private Item item;

    [ObservableProperty]
    private bool isNew;

    private Collection<Item> _listItems;

    private ChangeTracker _itemChangeTracker;

    private bool _hasChanges;
    private bool _changesHandled;

    public bool IsEditing => _hasChanges == true && _changesHandled == false;

    public override async Task InitializeAsync(ItemEditParameter parameter)
    {
        _listItems = parameter.ListItems;

        IsNew = parameter.IsNew;

        if (IsNew == true)
        {
            _originalItem = new Item();
        }
        else
        {
            _originalItem = await _itemService.GetItemByIdAsync(parameter.ItemId);
        }

        Item = new Item()
        {
            Id = _originalItem.Id,
            Name = _originalItem.Name,
            Description = _originalItem.Description
        };

        _itemChangeTracker = new ChangeTracker(typeof(Item), Item);
        _itemChangeTracker.TrackProperty(this, nameof(Item), () => _hasChanges = _itemChangeTracker.HasChanges);

        await Task.CompletedTask;
    }





    [RelayCommand]
    private async Task Save()
    {
        if (IsNew)
        {
            await _itemService.AddItemAsync(Item);

            if (_listItems != null)
            {
                _listItems.Add(Item);
            }
        }
        else
        {
            await _itemService.UpdateItemAsync(Item);
        }

        _changesHandled = true;
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        _changesHandled = true;
        await _navigationService.GoBackAsync();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _itemChangeTracker.Dispose();
        _disposed = true;
    }
}
