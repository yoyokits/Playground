using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class SensorValueView : ContentView
    {
        public event EventHandler<SensorValueSettingsRequestedEventArgs>? SensorValueSettingsRequested;

        public SensorValueView()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("[SensorValueView] Initialized");
        }

        private void OnSensorOverlayTapped(object? sender, TappedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueView] OnSensorOverlayTapped called");
            RaiseSettingsRequestedEvent();
        }

        private void OnSensorOverlayClicked(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueView] OnSensorOverlayClicked called (Button)");
            RaiseSettingsRequestedEvent();
        }

        private void RaiseSettingsRequestedEvent()
        {
            // Raise the event to signal that settings should be shown
            if (SensorValueSettingsRequested != null)
            {
                System.Diagnostics.Debug.WriteLine("[SensorValueView] Raising SensorValueSettingsRequested event");
                SensorValueSettingsRequested.Invoke(this, new SensorValueSettingsRequestedEventArgs());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[SensorValueView] WARNING: No handlers for SensorValueSettingsRequested event");
            }
        }

        private void OnSensorOverlayPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            // Pan gesture is disabled - fixed position at bottom-right
            // Keep this empty to prevent any movement
        }
    }

    public class SensorValueSettingsRequestedEventArgs : EventArgs
    {
        // Empty class for now, can be extended if needed
    }
}
