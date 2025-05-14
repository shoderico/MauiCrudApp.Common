using System.Collections.ObjectModel;
using $safeprojectname$.Core.Models;


namespace $safeprojectname$.Features.ItemEdit.ViewModels;

public class ItemEditParameter
{
    public int ItemId { get; set; }
    public bool IsNew { get; set; }

    public Collection<Item>? ListItems { get; set; }
}
