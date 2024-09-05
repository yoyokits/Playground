// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using Camera.MAUI;

namespace AdventureCamApp;

public static class MauiProgram
{
    #region Methods

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("FontAwesome6FreeBrands.otf", "FontAwesomeBrands");
                fonts.AddFont("FontAwesome6FreeRegular.otf", "FontAwesomeRegular");
                fonts.AddFont("FontAwesome6FreeSolid.otf", "FontAwesomeSolid");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseMauiCameraView();

        builder.Services.AddSingleton<MainViewModel>();

        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddSingleton<PreviewViewModel>();

        builder.Services.AddSingleton<PreviewPage>();

        builder.Services.AddSingleton<GalleryViewModel>();

        builder.Services.AddSingleton<GalleryPage>();

        builder.Services.AddTransient<SampleDataService>();
        builder.Services.AddTransient<EditorDetailViewModel>();
        builder.Services.AddTransient<EditorDetailPage>();

        builder.Services.AddSingleton<EditorViewModel>();

        builder.Services.AddSingleton<EditorPage>();

        builder.Services.AddSingleton<AboutViewModel>();

        builder.Services.AddSingleton<AboutPage>();

        return builder.Build();
    }

    #endregion Methods
}