using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TravelCamApp.Helpers;
using TravelCamApp.ViewModels;
using TravelCamApp.Views;

namespace TravelCamApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCameraView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register application services as singletons
            builder.Services.AddSingleton<SensorHelper>();

            // Register view models
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<SensorValueSettingsViewModel>();
            builder.Services.AddTransient<SensorValueViewModel>();

            // Register pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SensorValueSettingsView>();
            builder.Services.AddTransient<SensorValueView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
