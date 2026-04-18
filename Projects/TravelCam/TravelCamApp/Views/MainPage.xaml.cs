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

            // ── Gallery sensor settings overlay ──────────────────────────────
            GalleryView.WireSensorSettings(_sensorSettingsVm, viewModel);

            // ── Sensor settings overlay ──────────────────────────────────────
            DataOverlaySettingsPanel.BindingContext = _sensorSettingsVm;

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

            DataOverlaySettingsPanel.CloseRequested += async (s, e) =>
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

            // ── Aspect ratio or resolution changes → recompute letterbox bars ─
            _cameraSettingsVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TravelCamApp.ViewModels.CameraSettingsViewModel.SelectedAspectRatio) ||
                    e.PropertyName == nameof(TravelCamApp.ViewModels.CameraSettingsViewModel.SelectedResolutionIndex))
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
            System.Diagnostics.Debug.WriteLine("[MainPage] OnDisappearing");

            // Cancel any pending layout debounce
            _layoutDebounce?.Cancel();

            try
            {
                if (CameraView != null)
                {
                    System.Diagnostics.Debug.WriteLine("[MainPage] Stopping camera preview on page disappear");
                    CameraView.StopCameraPreview();
                    System.Diagnostics.Debug.WriteLine("[MainPage] Camera preview stopped");
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

            // Populate resolution picker from the newly selected camera's supported sizes
            if (CameraView.SelectedCamera?.SupportedResolutions is { Count: > 0 } resolutions)
                _cameraSettingsVm.SetAvailableResolutions(resolutions);

            // Invalidate cache and schedule debounced layout — SetAvailableResolutions may
            // also fire PropertyChanged→ScheduleLayoutUpdate, so debouncing avoids double work.
            _lastAppliedW = 0;
            ScheduleLayoutUpdate();
        }

        // ── Camera view alignment ──────────────────────────────────────────────────

        // Cache last applied dimensions to skip redundant layout passes.
        private double _lastAppliedW;
        private double _lastAppliedH;
        private CancellationTokenSource? _layoutDebounce;

        private void OnCameraViewSizeChanged(object? sender, EventArgs e)
        {
            if (sender is not CameraView cameraView) return;
            // Skip if dimensions haven't changed by more than 1dp — avoids sub-pixel thrashing.
            if (Math.Abs(cameraView.Width  - _lastAppliedW) < 1 &&
                Math.Abs(cameraView.Height - _lastAppliedH) < 1) return;
            _lastAppliedW = cameraView.Width;
            _lastAppliedH = cameraView.Height;
            ScheduleLayoutUpdate();
        }

        // Called when aspect ratio or resolution setting changes — force recalculate.
        private void UpdateAspectRatioBars(double _w, double _h)
        {
            // Invalidate cache so the next SizeChanged always runs.
            _lastAppliedW = 0;
            ScheduleLayoutUpdate();
        }

        /// <summary>
        /// Debounces layout recalculation — cancels any pending call and schedules a new one.
        /// Prevents cascading layout passes from blocking the main thread.
        /// </summary>
        private async void ScheduleLayoutUpdate()
        {
            _layoutDebounce?.Cancel();
            var cts = new CancellationTokenSource();
            _layoutDebounce = cts;
            try
            {
                // Yield to let the layout pass settle before recalculating.
                await Task.Delay(16, cts.Token); // ~1 frame at 60fps
                if (cts.Token.IsCancellationRequested) return;
                if (CameraView == null) return;
                ApplyCameraLayout(CameraView.Width, CameraView.Height, CameraView.SelectedCamera);
            }
            catch (OperationCanceledException) { /* debounce: newer call superseded this one */ }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPage] ScheduleLayoutUpdate error: {ex.Message}");
            }
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
                // The CameraView control may fill more space than the camera feed itself.
                // Use the sensor's native resolution to compute where the feed actually lands
                // within the CameraView bounds, so overlays align with real camera pixels.
                bool isPortrait = cameraViewHeight > cameraViewWidth;
                var res = selectedCamera.SupportedResolutions
                    .OrderByDescending(s => (long)s.Width * (long)s.Height)
                    .First();
                double camLong  = Math.Max(res.Width, res.Height);
                double camShort = Math.Min(res.Width, res.Height);
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
                double topBarH = 0, sideBarW = 0;
                if (r > 0)
                {
                    double desiredH = naturalW * r;
                    if (desiredH <= naturalH)
                    {
                        // Letterbox: crop top/bottom — desired height fits within the natural frame
                        croppedW = naturalW;
                        croppedH = desiredH;
                        topBarH = (naturalH - croppedH) / 2;
                    }
                    else
                    {
                        // Pillarbox: desired height exceeds natural frame → reduce width instead
                        // (e.g. 16:9 / 9:16 on a 4:3 sensor in portrait)
                        croppedH = naturalH;
                        croppedW = naturalH / r;
                        sideBarW = (naturalW - croppedW) / 2;
                    }
                }
                else
                {
                    croppedW = naturalW;
                    croppedH = naturalH;
                }

                // ── Step 3: feed offset within CameraView ─────────────────────────────────
                // The native camera renderer centers the feed (AspectFit) within the
                // CameraView control, which may be taller/wider than the feed itself.
                // Compute the feed's origin so overlays and bars align with actual pixels.
                double feedOffsetX = (cameraViewWidth  - naturalW) / 2;
                double feedOffsetY = (cameraViewHeight - naturalH) / 2;

                // ── Step 4: resize container + position crop bars ────────────────────────────
                CameraViewChildrenContainer.Margin = new Thickness(
                    feedOffsetX + sideBarW, feedOffsetY + topBarH, 0, 0);
                CameraViewChildrenContainer.WidthRequest  = croppedW;
                CameraViewChildrenContainer.HeightRequest = croppedH;

                // Letterbox bars (top / bottom) — span full width, positioned within feed area
                CropTopBar.Margin = new Thickness(0, feedOffsetY, 0, 0);
                CropTopBar.HeightRequest = topBarH;
                CropTopBar.IsVisible = topBarH > 0.5;
                CropBottomBar.Margin = new Thickness(0, feedOffsetY + topBarH + croppedH, 0, 0);
                CropBottomBar.HeightRequest = topBarH;
                CropBottomBar.IsVisible = topBarH > 0.5;

                // Pillarbox bars (left / right) — positioned at feed edges
                CropLeftBar.Margin = new Thickness(feedOffsetX, feedOffsetY, 0, 0);
                CropLeftBar.WidthRequest  = sideBarW;
                CropLeftBar.HeightRequest = naturalH;
                CropLeftBar.IsVisible = sideBarW > 0.5;
                CropRightBar.Margin = new Thickness(0, feedOffsetY, feedOffsetX, 0);
                CropRightBar.WidthRequest  = sideBarW;
                CropRightBar.HeightRequest = naturalH;
                CropRightBar.IsVisible = sideBarW > 0.5;
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

            // Apply visibility flags + reorder OverlayItems to match settings panel order
            _sensorSettingsVm.ApplyToOverlayItems(ViewModel.OverlayItems);

            // Rebuild VisibleOverlayItems in the new order so the camera overlay updates
            ViewModel.DataOverlayViewModel.RefreshVisibleItems();

            ViewModel.IsSettingsVisible = false;
        }
    }
}