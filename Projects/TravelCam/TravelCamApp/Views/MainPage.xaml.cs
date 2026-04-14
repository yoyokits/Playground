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

            // ── Aspect ratio changes → recompute letterbox bars ──────────────
            _cameraSettingsVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TravelCamApp.ViewModels.CameraSettingsViewModel.SelectedAspectRatio))
                    UpdateAspectRatioBars(CameraView.Width, CameraView.Height);
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
            if (CameraView == null) return;
            ApplyCameraLayout(CameraView.Width, CameraView.Height, CameraView.SelectedCamera);
        }

        // ── Camera view alignment ──────────────────────────────────────────────────

        private void OnCameraViewSizeChanged(object? sender, EventArgs e)
        {
            if (sender is not CameraView cameraView) return;
            try { ApplyCameraLayout(cameraView.Width, cameraView.Height, cameraView.SelectedCamera); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[MainPage] OnCameraViewSizeChanged error: {ex.Message}"); }
        }

        // Called when aspect ratio setting changes — recalculate without a new camera size event.
        private void UpdateAspectRatioBars(double _w, double _h)
        {
            if (CameraView != null)
                ApplyCameraLayout(CameraView.Width, CameraView.Height, CameraView.SelectedCamera);
        }

        /// <summary>
        /// Single unified method that:
        ///   1. Calculates the natural visible container size (AspectFit of camera native resolution)
        ///   2. Applies the selected aspect ratio crop using the phone's orientation:
        ///      - "Full"  → no crop (container = natural size)
        ///      - "4:3"   → portrait h/w = 4/3  (3:4 output, fills frame on a 4:3 sensor)
        ///      - "16:9"  → portrait h/w = 16/9 (9:16 output, pillarboxed on a 4:3 sensor)
        ///      - "1:1"   → h/w = 1 (square, letterboxed)
        ///   3. Sets CameraViewChildrenContainer to the cropped size
        ///   4. Updates ViewModel bar heights/widths for letterbox (top/bottom) or pillarbox (left/right)
        /// </summary>
        private void ApplyCameraLayout(double cameraViewWidth, double cameraViewHeight, CameraInfo? selectedCamera)
        {
            if (cameraViewWidth <= 0 || cameraViewHeight <= 0) return;
            if (selectedCamera == null) return;
            if (selectedCamera.SupportedResolutions == null || selectedCamera.SupportedResolutions.Count == 0) return;
            if (CameraViewChildrenContainer == null) return;

            try
            {
                // ── Step 1: natural visible area (AspectFit of sensor resolution) ──────────
                var res = selectedCamera.SupportedResolutions[selectedCamera.SupportedResolutions.Count - 1];
                // Normalize to landscape-first so portrait/landscape reporting differences don't matter
                double camLong  = Math.Max(res.Width, res.Height);
                double camShort = Math.Min(res.Width, res.Height);

                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
                bool isPortrait = displayInfo.Orientation == DisplayOrientation.Portrait;
                double camW = isPortrait ? camShort : camLong;
                double camH = isPortrait ? camLong  : camShort;

                double camAspect  = camW / camH;
                double viewAspect = cameraViewWidth / cameraViewHeight;

                double naturalW, naturalH;
                if (camAspect > viewAspect)
                {
                    naturalW = cameraViewWidth;
                    naturalH = cameraViewWidth / camAspect;
                }
                else
                {
                    naturalH = cameraViewHeight;
                    naturalW = cameraViewHeight * camAspect;
                }

                // ── Step 2: aspect ratio crop ─────────────────────────────────────────
                // r = desired container h/w.  Portrait: tall ratios (r > 1); landscape: wide (r < 1).
                // "4:3" and "16:9" labels are the output portrait ratios (3:4 and 9:16).
                double r = _cameraSettingsVm.SelectedAspectRatio switch
                {
                    TravelCamApp.ViewModels.AspectRatioOption.FourThree   => isPortrait ? 4.0 / 3.0 : 3.0 / 4.0,
                    TravelCamApp.ViewModels.AspectRatioOption.SixteenNine => isPortrait ? 16.0 / 9.0 : 9.0 / 16.0,
                    TravelCamApp.ViewModels.AspectRatioOption.OneOne       => 1.0,
                    _                                                       => 0.0   // FullScreen
                };

                double croppedW, croppedH;
                if (r > 0)
                {
                    double desiredH = naturalW * r;
                    if (desiredH <= naturalH)
                    {
                        // Letterbox: crop top/bottom — desired height fits within the natural frame
                        croppedW = naturalW;
                        croppedH = desiredH;
                    }
                    else
                    {
                        // Pillarbox: desired height exceeds natural frame → reduce width instead
                        // (e.g. 16:9 / 9:16 on a 4:3 sensor in portrait)
                        croppedH = naturalH;
                        croppedW = naturalH / r;
                    }
                }
                else
                {
                    croppedW = naturalW;
                    croppedH = naturalH;
                }

                // ── Step 3: resize container to the cropped area ─────────────────────────────
                CameraViewChildrenContainer.WidthRequest  = croppedW;
                CameraViewChildrenContainer.HeightRequest = croppedH;

                // ── Step 4: letterbox bars (top/bottom) ──────────────────────────────────────
                bool hasTopBottomBars = r > 0 && croppedH < naturalH;
                if (hasTopBottomBars)
                {
                    double barH = (cameraViewHeight - croppedH) / 2.0;
                    ViewModel.AspectTopBarHeight    = barH;
                    ViewModel.AspectBottomBarHeight = barH;
                }
                else
                {
                    ViewModel.AspectTopBarHeight    = 0;
                    ViewModel.AspectBottomBarHeight = 0;
                }

                // ── Step 5: pillarbox bars (left/right) ──────────────────────────────────────
                bool hasSideBars = r > 0 && croppedW < naturalW;
                if (hasSideBars)
                {
                    double barW = (cameraViewWidth - croppedW) / 2.0;
                    ViewModel.AspectLeftBarWidth  = barW;
                    ViewModel.AspectRightBarWidth = barW;
                }
                else
                {
                    ViewModel.AspectLeftBarWidth  = 0;
                    ViewModel.AspectRightBarWidth = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPage] ApplyCameraLayout error: {ex.Message}");
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