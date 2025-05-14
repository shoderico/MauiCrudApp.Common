using Microsoft.Extensions.Logging;
using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Services;

using $safeprojectname$.Core.Services;
using $safeprojectname$.Features;
using $safeprojectname$.Features.ItemList.ViewModels;
using $safeprojectname$.Features.ItemList.Views;
using $safeprojectname$.Features.ItemEdit.ViewModels;
using $safeprojectname$.Features.ItemEdit.Views;

namespace $safeprojectname$
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Common Services
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<INavigationParameterStore, NavigationParameterStore>();
            builder.Services.AddSingleton<IDialogService, DialogService>();



            // Services
            builder.Services.AddSingleton<IItemService, ItemService>();

            // ViewModels
            builder.Services.AddTransient<ItemListViewModel>();
            builder.Services.AddTransient<ItemEditViewModel>();

            // Views
            builder.Services.AddTransient<ItemListPage>();
            builder.Services.AddTransient<ItemEditPage>();

            // Register AppShell and App
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<App>();

            return builder.Build();
        }
    }
}
