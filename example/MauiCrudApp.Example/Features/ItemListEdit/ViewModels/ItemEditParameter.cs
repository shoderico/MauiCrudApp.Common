using System.Collections.ObjectModel;
using MauiCrudApp.Example.Core.Models;


namespace MauiCrudApp.Example.Features.ItemListEdit.ViewModels;

public class ItemEditParameter
{
    public int ItemId { get; set; }
    public bool IsNew { get; set; }

    public Collection<Item>? ListItems { get; set; }
}
