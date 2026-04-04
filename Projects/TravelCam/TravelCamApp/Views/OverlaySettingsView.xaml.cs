using TravelCamApp.ViewModels;
using TravelCamApp.Models;

namespace TravelCamApp.Views
{
    public partial class OverlaySettingsView : ContentView
    {
        private OverlaySettingsViewModel ViewModel =>
            (BindingContext as OverlaySettingsViewModel)
            ?? throw new InvalidOperationException("OverlaySettingsView BindingContext not set");

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

        private void OnItemRemoveClicked(object? sender, TappedEventArgs e)
        {
            if (sender is View v && v.BindingContext is OverlayItem item)
                ViewModel.MoveToAvailable(item);
        }

        private void OnItemAddClicked(object? sender, TappedEventArgs e)
        {
            if (sender is View v && v.BindingContext is OverlayItem item)
                ViewModel.MoveToVisible(item);
        }

        private void OnCloseClicked(object? sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
