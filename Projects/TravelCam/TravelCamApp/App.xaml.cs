using Microsoft.Extensions.DependencyInjection;

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
                return new Window(new AppShell());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FATAL] CreateWindow failed: {ex.GetType().Name} — {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}