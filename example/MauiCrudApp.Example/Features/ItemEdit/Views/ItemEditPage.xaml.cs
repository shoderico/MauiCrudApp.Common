using MauiCrudApp.Common.Views;
using MauiCrudApp.Example.Features.ItemEdit.ViewModels;

namespace MauiCrudApp.Example.Features.ItemEdit.Views;

public partial class ItemEditPage : PageBase
{
    public ItemEditPage(ItemEditViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}