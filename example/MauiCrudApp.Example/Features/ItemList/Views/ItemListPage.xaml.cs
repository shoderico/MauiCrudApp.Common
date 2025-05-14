using MauiCrudApp.Common.Views;
using MauiCrudApp.Example.Features.ItemList.ViewModels;

namespace MauiCrudApp.Example.Features.ItemList.Views;

public partial class ItemListPage : PageBase
{
    public ItemListPage(ItemListViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}