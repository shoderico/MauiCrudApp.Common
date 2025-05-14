using MauiCrudApp.Example.Core.Models;

namespace MauiCrudApp.Example.Core.Services;

public interface IItemService
{
    Task<List<Item>> GetItemsAsync(string searchText = null);
    Task<Item> GetItemByIdAsync(int id);
    Task AddItemAsync(Item item);
    Task UpdateItemAsync(Item item);
    Task DeleteItemAsync(int id);
}
