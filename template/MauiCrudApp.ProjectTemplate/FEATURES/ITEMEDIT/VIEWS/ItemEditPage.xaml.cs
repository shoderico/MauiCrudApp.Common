using MauiCrudApp.Common.Views;
using $safeprojectname$.Features.ItemEdit.ViewModels;

namespace $safeprojectname$.Features.ItemEdit.Views;

public partial class ItemEditPage : PageBase
{
    public ItemEditPage(ItemEditViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}