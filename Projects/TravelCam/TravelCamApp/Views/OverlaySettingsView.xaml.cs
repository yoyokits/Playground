using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class OverlaySettingsView : ContentView
    {
        /// <summary>
        /// Raised when the user closes the settings overlay.
        /// The parent page should handle saving and hiding.
        /// </summary>
        public event EventHandler? CloseRequested;

        public OverlaySettingsView()
        {
            InitializeComponent();
        }

        private void OnVisibleListReorderCompleted(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[OverlaySettingsView] Reorder completed");
        }

        private void OnCloseClicked(object? sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
