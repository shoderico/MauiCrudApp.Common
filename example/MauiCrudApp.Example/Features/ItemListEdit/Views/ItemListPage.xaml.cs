using MauiCrudApp.Common.Views;
using MauiCrudApp.Example.Features.ItemListEdit.ViewModels;

namespace MauiCrudApp.Example.Features.ItemListEdit.Views;

public partial class ItemListPage : PageBase
{
    public ItemListPage(ItemListViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}