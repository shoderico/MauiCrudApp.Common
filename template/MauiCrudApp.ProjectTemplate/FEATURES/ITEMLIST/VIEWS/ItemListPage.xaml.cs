using MauiCrudApp.Common.Views;
using $safeprojectname$.Features.ItemList.ViewModels;

namespace $safeprojectname$.Features.ItemList.Views;

public partial class ItemListPage : PageBase
{
    public ItemListPage(ItemListViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}