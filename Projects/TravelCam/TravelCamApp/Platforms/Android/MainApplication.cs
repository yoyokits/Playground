using Android.App;
using Android.Runtime;

namespace TravelCamApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            // Catch unobserved Task exceptions — prevents crash from fire-and-forget tasks.
            // Safe to SetObserved: these are truly orphaned background tasks.
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[TravelCam] UnobservedTaskException: {e.Exception.Flatten().Message}");
                e.SetObserved();
            };

            // Log unhandled exceptions for diagnostics — do NOT set e.Handled=true
            // as swallowing real exceptions leaves the app in a frozen/broken state.
            AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[TravelCam] UnhandledException: {e.Exception.Message}\n{e.Exception.StackTrace}");
            };

            return MauiProgram.CreateMauiApp();
        }
    }
}
