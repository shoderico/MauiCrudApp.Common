using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.ViewModels;

namespace MauiCrudApp.Common.Views
{
    public abstract class PageBase : ContentPage
    {
        protected readonly IInitialize _viewModel;

        public PageBase(IInitialize viewModel)
        {
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.PerformInitializeAsync();
        }
    }
}
