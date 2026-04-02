// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// MainPage code-behind:
// - Passes the CameraView reference to the ViewModel on OnAppearing.
// - Routes CameraView MediaCaptured / MediaCaptureFailed events.
// - Plays the shutter press animation (scale bounce).
// - Shows / hides the settings overlay.

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
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

            // Populate settings list when the overlay opens
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainPageViewModel.IsSettingsVisible)
                    && viewModel.IsSettingsVisible)
                {
                    _settingsViewModel.LoadFromSensorItems(viewModel.SensorItems);
                }
            };

            // Save + close settings overlay
            SettingsOverlay.CloseRequested += async (s, e) => await HideSettingsAsync();

            // Route camera media events to ViewModel
            CameraView.MediaCaptured += OnMediaCaptured;
            CameraView.MediaCaptureFailed += OnMediaCaptureFailed;
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("[MainPage] OnAppearing");
            await ViewModel.OnViewReady(CameraView);
        }

        // ── Shutter button ─────────────────────────────────────────────────────

        /// <summary>
        /// Tap handler for the shutter Grid.
        /// Fires the command then plays a quick scale-bounce animation.
        /// </summary>
        private async void OnShutterTapped(object? sender, TappedEventArgs e)
        {
            // Execute capture immediately so latency is minimised
            if (ViewModel.CaptureCommand.CanExecute(null))
                ViewModel.CaptureCommand.Execute(null);

            // Scale-bounce: shrink → normal
            await ShutterButton.ScaleTo(0.86, 80, Easing.CubicOut);
            await ShutterButton.ScaleTo(1.00, 90, Easing.SpringOut);
        }

        // ── Camera media events ────────────────────────────────────────────────

        private async void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
        {
            await ViewModel.OnMediaCaptured(e.Media);
        }

        private void OnMediaCaptureFailed(object? sender, MediaCaptureFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPage] Capture failed: {e.FailureReason}");
            ViewModel.OnMediaCaptureFailed();
        }

        // ── Settings overlay ───────────────────────────────────────────────────

        private async Task HideSettingsAsync()
        {
            await _settingsViewModel.SaveSettingsAsync();
            _settingsViewModel.ApplyToSensorItems(ViewModel.SensorItems);
            ViewModel.IsSettingsVisible = false;
        }
    }
}
