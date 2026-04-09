using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace TravelCamApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTask, ResizeableActivity = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Opt into edge-to-edge on API 29–34.
            // API 35+ enforces edge-to-edge automatically when targetSdkVersion=35.
            // This ensures a consistent full-bleed camera layout on all supported devices.
            WindowCompat.SetDecorFitsSystemWindows(Window!, false);
        }
    }
}
