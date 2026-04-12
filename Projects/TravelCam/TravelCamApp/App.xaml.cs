using Microsoft.Extensions.DependencyInjection;
using TravelCamApp.Helpers;
using TravelCamApp.Views;
using TravelCamApp.ViewModels;

namespace TravelCamApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Global crash diagnostics — logs to debug output so logcat shows the root cause
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine(
                    $"[FATAL] UnhandledException: {ex?.GetType().Name} — {ex?.Message}\n{ex?.StackTrace}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FATAL] UnobservedTaskException: {e.Exception.GetType().Name} — {e.Exception.Message}\n{e.Exception.StackTrace}");
                e.SetObserved(); // prevent process kill so we can see the log
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[App] CreateWindow called");
                var window = new Window(new AppShell());

                // ✅ CRITICAL: Stop camera and sensors when app is backgrounded or destroyed
                // This prevents ObjectDisposedException on reopen (known .NET 10 issue fixed in SR5)
                // Grox solution: proper lifecycle cleanup for camera + sensors
                // CRITICAL FIX: Make cleanup blocking to prevent race condition with OS killing camera

                window.Stopped += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("[App] Window.Stopped — cleaning up camera and sensors");
                    StopCameraAndSensorsSync();
                };

                window.Destroying += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("[App] Window.Destroying — final cleanup");
                    StopCameraAndSensorsSync();
                };

                return window;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FATAL] CreateWindow failed: {ex.GetType().Name} — {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// SYNCHRONOUS cleanup of camera and sensors.
        /// Must complete before process termination to prevent ObjectDisposedException on reopen.
        /// Blocks the thread to ensure cleanup finishes before OS kills the app.
        /// </summary>
        private void StopCameraAndSensorsSync()
        {
            try
            {
                // ✅ CRITICAL: Stop all sensors (GPS, compass, accelerometer, gyroscope)
                // Must be synchronous to block process termination
                var sensorHelper = Handler?.MauiContext?.Services?.GetService<SensorHelper>();
                if (sensorHelper != null)
                {
                    System.Diagnostics.Debug.WriteLine("[App] Stopping SensorHelper (SYNC)");
                    sensorHelper.Stop();
                    System.Diagnostics.Debug.WriteLine("[App] SensorHelper stopped (SYNC)");
                }

                // ✅ CRITICAL: Force camera cleanup through MainPageViewModel if it exists
                // Get the MainPageViewModel directly
                var mainPageViewModel = Handler?.MauiContext?.Services?.GetService<MainPageViewModel>();
                if (mainPageViewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine("[App] Stopping MainPageViewModel camera (SYNC)");
                    // Call the synchronous stop method
                    mainPageViewModel.StopCameraPreviewSync();
                    System.Diagnostics.Debug.WriteLine("[App] MainPageViewModel camera stopped (SYNC)");
                }

                // ✅ CRITICAL: Force garbage collection to ensure disposed objects are finalized
                // This helps prevent ObjectDisposedException on reopen
                System.Diagnostics.Debug.WriteLine("[App] Forcing GC and finalization");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                System.Diagnostics.Debug.WriteLine("[App] GC complete");

                // ✅ Give the system a moment to finish cleanup operations
                System.Threading.Thread.Sleep(500);
                System.Diagnostics.Debug.WriteLine("[App] Cleanup block complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[App] Cleanup error (attempting to continue): {ex.GetType().Name} — {ex.Message}");
            }
        }
    }
}