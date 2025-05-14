using MauiCrudApp.Common.Views;
using $rootnamespace$.$fileinputname$.ViewModels;

namespace $rootnamespace$.$fileinputname$.Views;

public partial class $fileinputname$Page : PageBase
{
    public $fileinputname$Page($fileinputname$ViewModel viewModel) : base(viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}