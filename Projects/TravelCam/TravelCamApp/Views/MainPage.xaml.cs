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

            // ── Camera ready event ───────────────────────────────────────────
            // Subscribe to ViewModel's CameraReady event to trigger overlay container sizing
            viewModel.CameraReady += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[MainPage] CameraReady event received, calling OnCameraReady()");
                OnCameraReady();
            };
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

        /// <summary>
        /// Public method called by ViewModel when camera is selected and ready.
        /// Triggers container sizing calculation with the selected camera.
        /// </summary>
        public void OnCameraReady()
        {
            System.Diagnostics.Debug.WriteLine("[MainPage] OnCameraReady called");

            if (CameraView == null)
            {
                System.Diagnostics.Debug.WriteLine("[MainPage] OnCameraReady: CameraView is null");
                return;
            }

            CalculateAndPositionCameraViewChildrenContainer(
                CameraView.Width,
                CameraView.Height,
                CameraView.SelectedCamera);
        }

        // ── Camera view alignment ──────────────────────────────────────────────────

        /// <summary>
        /// Event handler called when CameraView size changes or camera is selected.
        /// Recalculates CameraViewChildrenContainer to align with visible camera feed.
        /// </summary>
        private void OnCameraViewSizeChanged(object? sender, EventArgs e)
        {
            if (sender is not CameraView cameraView) return;

            try
            {
                CalculateAndPositionCameraViewChildrenContainer(
                    cameraView.Width,
                    cameraView.Height,
                    cameraView.SelectedCamera);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPage] OnCameraViewSizeChanged error: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the visible camera feed bounds (accounting for AspectFit letterboxing/pillarboxing)
        /// and sizes/positions CameraViewChildrenContainer to match.
        ///
        /// Logic:
        /// 1. Get camera view dimensions and selected camera resolution
        /// 2. Handle orientation by swapping resolution dimensions for portrait
        /// 3. Calculate aspect ratios (camera vs viewport)
        /// 4. Determine visible dimensions based on AspectFit scaling
        /// 5. Set CameraViewChildrenContainer size
        /// 6. HorizontalOptions/VerticalOptions="Center" automatically centers it
        /// </summary>
        private void CalculateAndPositionCameraViewChildrenContainer(
            double cameraViewWidth,
            double cameraViewHeight,
            CameraInfo? selectedCamera)
        {
            // 1. SAFETY CHECKS
            if (cameraViewWidth <= 0 || cameraViewHeight <= 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPage] CameraView has invalid dimensions, skipping container calculation");
                return;
            }

            if (selectedCamera == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPage] No camera selected, skipping container calculation");
                return;
            }

            if (selectedCamera.SupportedResolutions == null || selectedCamera.SupportedResolutions.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPage] Camera has no supported resolutions, skipping container calculation");
                return;
            }

            if (CameraViewChildrenContainer == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPage] CameraViewChildrenContainer not found, skipping calculation");
                return;
            }

            try
            {
                // 2. GET CAMERA RESOLUTION
                // Use highest resolution (most likely what's being rendered)
                var cameraResolution = selectedCamera.SupportedResolutions[selectedCamera.SupportedResolutions.Count - 1];
                double cameraWidth = cameraResolution.Width;
                double cameraHeight = cameraResolution.Height;

                // 3. HANDLE ORIENTATION
                // Device is in portrait, so swap camera dimensions to match portrait coordinate system
                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
                if (displayInfo.Orientation == DisplayOrientation.Portrait)
                {
                    (cameraWidth, cameraHeight) = (cameraHeight, cameraWidth);
                }

                // 4. CALCULATE ASPECT RATIOS
                double cameraAspect = cameraWidth / cameraHeight;
                double viewAspect = cameraViewWidth / cameraViewHeight;

                // 5. CALCULATE VISIBLE DIMENSIONS (with AspectFit scaling)
                double visibleWidth, visibleHeight;

                if (cameraAspect > viewAspect)
                {
                    // Camera is wider relative to view → letterbox top/bottom
                    visibleWidth = cameraViewWidth;
                    visibleHeight = cameraViewWidth / cameraAspect;
                }
                else
                {
                    // Camera is taller relative to view → pillarbox left/right
                    visibleHeight = cameraViewHeight;
                    visibleWidth = cameraViewHeight * cameraAspect;
                }

                // 6. SET CONTAINER SIZE
                // HorizontalOptions="Center" and VerticalOptions="Center" automatically center it
                CameraViewChildrenContainer.WidthRequest = visibleWidth;
                CameraViewChildrenContainer.HeightRequest = visibleHeight;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPage] CalculateAndPositionCameraViewChildrenContainer error: {ex.Message}");
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