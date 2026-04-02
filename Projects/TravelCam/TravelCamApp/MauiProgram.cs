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
                .UseMauiCommunityToolkitCamera()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Singletons — one instance shared across the entire app lifetime
            builder.Services.AddSingleton<SensorHelper>();
            // CameraSettingsViewModel persists Preferences-backed settings;
            // a single instance avoids reloading prefs on every navigation.
            builder.Services.AddSingleton<CameraSettingsViewModel>();

            // Transient — fresh instance per injection (each page/navigation)
            builder.Services.AddTransient<SensorValueViewModel>();
            builder.Services.AddTransient<SensorValueSettingsViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();

            // Pages / views
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
