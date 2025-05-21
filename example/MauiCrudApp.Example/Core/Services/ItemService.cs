using MauiCrudApp.Example.Core.Interfaces;
using MauiCrudApp.Example.Core.Models;

namespace MauiCrudApp.Example.Core.Services;

public class ItemService : IItemService
{
    private readonly List<Item> _items = new()
    {
        new Item {Id = 1, Name="John Doe", Description="placeholder male names that are used in the British and American legal system."},
        new Item {Id=2, Name="Jane Roe", Description="placeholder female names that are used in the British and American legal system" }
    };

    public async Task<List<Item>> GetItemsAsync(string? searchText = null)
    {
        var items = _items.AsEnumerable();
        if (!string.IsNullOrEmpty(searchText))
        {
            items = items.Where(i => i.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }
        return await Task.FromResult(items.ToList());
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
    }

    public async Task AddItemAsync(Item item)
    {
        if (_items.Count == 0)
            item.Id = 0;
        else
            item.Id = _items.Max(i => i.Id) + 1;

        _items.Add(item);
        await Task.CompletedTask;
    }

    public async Task UpdateItemAsync(Item item)
    {
        var existing = _items.FirstOrDefault(i => i.Id == item.Id);
        if (existing != null)
        {
            existing.Name = item.Name;
            existing.Description = item.Description;
        }
        await Task.CompletedTask;
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            _items.Remove(item);
        }
        await Task.CompletedTask;
    }
}
