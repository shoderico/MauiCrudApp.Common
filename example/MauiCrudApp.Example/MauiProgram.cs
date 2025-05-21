using Microsoft.Extensions.Logging;
using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Services;

namespace MauiCrudApp.Example
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
            builder.Services.AddSingleton<Core.Interfaces.IItemService, Core.Services.ItemService>();



            // Feature/ItemListEdit : ItemList
            builder.Services.AddTransient<Features.ItemListEdit.ViewModels.ItemListViewModel>();
            builder.Services.AddTransient<Features.ItemListEdit.Views.ItemListPage>();
            // Feature/ItemListEdit : ItemEdit
            builder.Services.AddTransient<Features.ItemListEdit.ViewModels.ItemEditViewModel>();
            builder.Services.AddTransient<Features.ItemListEdit.Views.ItemEditPage>();



            // Register AppShell and App
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<App>();

            return builder.Build();
        }
    }
}
