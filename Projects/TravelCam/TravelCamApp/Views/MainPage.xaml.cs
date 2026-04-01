// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// MainPage code-behind:
// - Receives the CameraView and passes it to the ViewModel
// - Shows/hides the settings overlay as a modal
// - Does NOT dispose the camera when overlays appear or
//   OnDisappearing fires. Camera lifecycle is managed by the
//   ViewModel via Application.Activated/Deactivated events.

using Camera.MAUI;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel ViewModel => (MainPageViewModel)BindingContext;
        private readonly SensorValueSettingsViewModel _settingsViewModel;

        public MainPage(MainPageViewModel viewModel, SensorValueSettingsViewModel settingsViewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _settingsViewModel = settingsViewModel;
            SettingsOverlay.BindingContext = _settingsViewModel;

            // React to settings overlay visibility changes
            viewModel.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(MainPageViewModel.IsSettingsVisible)
                    && viewModel.IsSettingsVisible)
                {
                    _settingsViewModel.LoadFromSensorItems(viewModel.SensorItems);
                }
            };
        }

        /// <summary>
        /// Called by Camera.MAUI when available cameras are loaded.
        /// This is the earliest reliable signal that the camera control is ready.
        /// </summary>
        private async void OnCameraViewCamerasLoaded(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[MainPage] Cameras loaded, notifying ViewModel");
            await ViewModel.OnViewReady(CameraView);
        }

        /// <summary>
        /// Hide the settings overlay. Saves settings and applies back to the main list.
        /// </summary>
        internal async Task HideSettingsAsync()
        {
            await _settingsViewModel.SaveSettingsAsync();
            _settingsViewModel.ApplyToSensorItems(ViewModel.SensorItems);
            ViewModel.IsSettingsVisible = false;
        }
    }
}
