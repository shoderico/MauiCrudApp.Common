using MauiCrudApp.Common.Views;
using MauiCrudApp.Example.Features.ItemListEdit.ViewModels;

namespace MauiCrudApp.Example.Features.ItemListEdit.Views;

public partial class ItemEditPage : PageBase
{
    public ItemEditPage(ItemEditViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}