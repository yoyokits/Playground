using TravelCamApp.Views;

namespace TravelCamApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SensorValueSettingsPage), typeof(SensorValueSettingsPage));
        }
    }
}
