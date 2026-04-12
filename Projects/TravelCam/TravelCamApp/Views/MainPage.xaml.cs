// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// MainPage code-behind:
// - Passes the CameraView reference to the ViewModel on OnAppearing.
// - Routes CameraView MediaCaptured / MediaCaptureFailed events.
// - Plays the shutter press animation (scale bounce).
// - Shows / hides the sensor settings overlay.
// - Shows / hides the camera settings overlay.

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel ViewModel => (MainPageViewModel)BindingContext;

        private readonly OverlaySettingsViewModel _sensorSettingsVm;
        private readonly CameraSettingsViewModel _cameraSettingsVm;

        public MainPage(
            MainPageViewModel viewModel,
            OverlaySettingsViewModel sensorSettingsVm,
            CameraSettingsViewModel cameraSettingsVm)
        {
            InitializeComponent();
            BindingContext = viewModel;

            _sensorSettingsVm = sensorSettingsVm;
            _cameraSettingsVm = cameraSettingsVm;

            // ── Sensor settings overlay ──────────────────────────────────────
            SensorSettingsOverlay.BindingContext = _sensorSettingsVm;

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainPageViewModel.IsSettingsVisible)
                    && viewModel.IsSettingsVisible)
                {
                    // Load current items + sync font size into settings panel
                    _sensorSettingsVm.LoadFromOverlayItems(viewModel.OverlayItems);
                    _sensorSettingsVm.FontSize = viewModel.DataOverlayViewModel.FontSize;
                }
            };

            SensorSettingsOverlay.CloseRequested += async (s, e) =>
            {
                try { await HideSensorSettingsAsync(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPage] HideSensorSettings error: {ex.Message}");
                }
            };

            // ── Camera settings overlay ──────────────────────────────────────
            CameraSettingsOverlay.BindingContext = _cameraSettingsVm;

            CameraSettingsOverlay.CloseRequested +=
                (s, e) => ViewModel.IsCameraSettingsVisible = false;

            // ── Camera media events ──────────────────────────────────────────
            CameraView.MediaCaptured += OnMediaCaptured;
            CameraView.MediaCaptureFailed += OnMediaCaptureFailed;
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("[MainPage] OnAppearing");
            try
            {
                await ViewModel.OnViewReady(CameraView);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPage] OnAppearing error: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("[MainPage] OnDisappearing — cleaning up camera (SYNC/BLOCKING)");
            try
            {
                // CRITICAL: Stop preview SYNCHRONOUSLY and BLOCKING to prevent race conditions
                // Do NOT use MainThread.BeginInvokeOnMainThread — it's async and won't block page destruction
                if (CameraView != null)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("[MainPage] Stopping camera preview on page disappear (SYNC)");
                        CameraView.StopCameraPreview();
                        System.Diagnostics.Debug.WriteLine("[MainPage] Camera preview stopped on page disappear (SYNC)");

                        // Force garbage collection to release camera resources
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        System.Diagnostics.Debug.WriteLine("[MainPage] GC after camera stop");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[MainPage] Error stopping preview on disappear: {ex.GetType().Name} — {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPage] OnDisappearing error: {ex.GetType().Name} — {ex.Message}");
            }
        }

        // ── Shutter button ─────────────────────────────────────────────────────

        private async void OnShutterTapped(object? sender, TappedEventArgs e)
        {
            try
            {
                if (ViewModel.CaptureCommand.CanExecute(null))
                    ViewModel.CaptureCommand.Execute(null);

                await ShutterButton.ScaleToAsync(0.86, 80, Easing.CubicOut);
                await ShutterButton.ScaleToAsync(1.00, 90, Easing.SpringOut);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPage] OnShutterTapped error: {ex.Message}");
            }
        }

        // ── Camera media events ────────────────────────────────────────────────

        private async void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
        {
            try
            {
                await ViewModel.OnMediaCaptured(e.Media);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPage] OnMediaCaptured error: {ex.Message}");
            }
        }

        private void OnMediaCaptureFailed(object? sender, MediaCaptureFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPage] Capture failed: {e.FailureReason}");
            ViewModel.OnMediaCaptureFailed();
        }

        // ── Sensor settings overlay ────────────────────────────────────────────

        private async Task HideSensorSettingsAsync()
        {
            // Sync font size back to the live display VM before closing
            ViewModel.DataOverlayViewModel.FontSize = _sensorSettingsVm.FontSize;

            await _sensorSettingsVm.SaveSettingsAsync();
            _sensorSettingsVm.ApplyToOverlayItems(ViewModel.OverlayItems);
            ViewModel.IsSettingsVisible = false;
        }
    }
}