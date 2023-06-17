using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using FileShareApp.Platforms.Android;
using Microsoft.Maui.Controls.Compatibility;

namespace FileShareApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public static Activity FormsActivity { get; set; }
    public static Context FormsContext { get; set; }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        MediaServiceImplementation.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        MediaServiceImplementation.OnActivityResult(requestCode, resultCode, data);
        base.OnActivityResult(requestCode, resultCode, data);
    }
}