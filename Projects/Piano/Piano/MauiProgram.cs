using Microsoft.Extensions.Logging;
using Piano.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Piano;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register platform-specific audio service
#if ANDROID
		builder.Services.AddSingleton<IAudioService>(AndroidAudioService.Instance);
#elif WINDOWS
		builder.Services.AddSingleton<IAudioService>(WindowsAudioService.Instance);
#endif

		return builder.Build();
	}
}
