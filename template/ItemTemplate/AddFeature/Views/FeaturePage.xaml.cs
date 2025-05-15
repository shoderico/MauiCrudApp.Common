using MauiCrudApp.Common.Views;
using $rootnamespace$.ViewModels;

namespace $rootnamespace$.Views;

public partial class $fileinputname$Page : PageBase
{
    public $fileinputname$Page($fileinputname$ViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}