using TravelCamApp.ViewModels;
using TravelCamApp.Models;

namespace TravelCamApp.Views
{
    public partial class SensorValueSettingsView : ContentView
    {
        private SensorValueSettingsViewModel ViewModel =>
            (BindingContext as SensorValueSettingsViewModel)
            ?? throw new InvalidOperationException("SensorValueSettingsView BindingContext not set");

        /// <summary>
        /// Raised when the user closes the settings overlay.
        /// The parent page should handle saving and hiding.
        /// </summary>
        public event EventHandler? CloseRequested;

        public SensorValueSettingsView()
        {
            InitializeComponent();
        }

        private void OnVisibleListReorderCompleted(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Reorder completed");
        }

        private void OnAddButtonClicked(object? sender, EventArgs e)
        {
            if (AvailableSensorsList.SelectedItem is SensorItem item)
            {
                ViewModel.MoveToVisible(item);
                AvailableSensorsList.SelectedItem = null;
            }
        }

        private void OnRemoveButtonClicked(object? sender, EventArgs e)
        {
            if (VisibleSensorsList.SelectedItem is SensorItem item)
            {
                ViewModel.MoveToAvailable(item);
                VisibleSensorsList.SelectedItem = null;
            }
        }

        private void OnCloseClicked(object? sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
