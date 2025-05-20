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
        protected readonly ILifecycle _viewModel;

        public PageBase(ILifecycle viewModel)
        {
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.PerformInitializeAsync();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _viewModel.PerformFinalizeAsync();
        }
    }
}
