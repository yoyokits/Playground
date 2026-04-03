// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// MainPageViewModel: Single coordinator for all camera, sensor,
// and UI state.
//
// Camera operations use CommunityToolkit.Maui.Camera (CameraView).
// Photo capture results arrive via the MediaCaptured event routed
// from MainPage.xaml.cs.  Video stream is returned synchronously
// from StopVideoRecording().
//
// Sensor display is delegated to DataOverlayViewModel, which
// subscribes to SensorHelper independently.  OverlayItems is
// exposed here as a passthrough so XAML bindings are unchanged.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using TravelCamApp.Helpers;
using TravelCamApp.Models;

namespace TravelCamApp.ViewModels
{
    public enum CaptureMode { Photo, Video }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly SensorHelper _sensorHelper;
        private readonly DataOverlayViewModel _sensorValueViewModel;
        private readonly CameraSettingsViewModel _cameraSettings;

        // Camera
        private CameraView? _cameraView;
        private bool _isPreviewRunning;
        private bool _isRecording;
        private CaptureMode _selectedMode = CaptureMode.Photo;
        private string _recordingTimeText = "00:00";
        private ImageSource? _lastCaptureImage;
        private string? _lastGalleryPath;     // content:// URI — for gallery open
        private string? _lastThumbPath;       // plain file path — for thumbnail display
        private bool _isCapturing;
        private bool _isTogglingRecording;
        private string _permissionStatus = "Initializing...";
        private bool _hasCameraPermission;

        // Overlay visibility
        private bool _isSensorOverlayVisible = true;
        private bool _isSettingsVisible;
        private bool _isCameraSettingsVisible;

        // Flash
        private bool _isFlashOn;

        // Recording display timer
        private System.Timers.Timer? _recordingTimer;
        private DateTime _recordingStart;

        // Lifecycle guards
        private bool _lifecycleSubscribed;
        private bool _isDestroyed;

        // Preference key for last thumbnail (plain file path — never content://)
        private const string PrefLastThumbPath = "LastThumbPath";

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        public CameraView? CameraView
        {
            get => _cameraView;
            set { _cameraView = value; OnPropertyChanged(); }
        }

        public bool IsPreviewRunning
        {
            get => _isPreviewRunning;
            set { _isPreviewRunning = value; OnPropertyChanged(); }
        }

        public bool IsRecording
        {
            get => _isRecording;
            set { _isRecording = value; OnPropertyChanged(); }
        }

        public CaptureMode SelectedMode
        {
            get => _selectedMode;
            set { _selectedMode = value; OnPropertyChanged(); }
        }

        public string RecordingTimeText
        {
            get => _recordingTimeText;
            set { _recordingTimeText = value; OnPropertyChanged(); }
        }

        public ImageSource? LastCaptureImage
        {
            get => _lastCaptureImage;
            set { _lastCaptureImage = value; OnPropertyChanged(); }
        }

        public string PermissionStatus
        {
            get => _permissionStatus;
            set { _permissionStatus = value; OnPropertyChanged(); }
        }

        public bool HasCameraPermission
        {
            get => _hasCameraPermission;
            set { _hasCameraPermission = value; OnPropertyChanged(); }
        }

        public bool IsSensorOverlayVisible
        {
            get => _isSensorOverlayVisible;
            set { _isSensorOverlayVisible = value; OnPropertyChanged(); }
        }

        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set { _isSettingsVisible = value; OnPropertyChanged(); }
        }

        public bool IsCameraSettingsVisible
        {
            get => _isCameraSettingsVisible;
            set { _isCameraSettingsVisible = value; OnPropertyChanged(); }
        }

        public bool IsFlashOn
        {
            get => _isFlashOn;
            set { _isFlashOn = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Dynamic zoom presets generated from the active camera's min/max zoom range.
        /// Each ZoomPreset carries its own SelectCommand and IsSelected state.
        /// Bound to the zoom pill strip via BindableLayout.
        /// </summary>
        public ObservableCollection<ZoomPreset> ZoomPresets { get; } = new();

        /// <summary>Passthrough to DataOverlayViewModel.OverlayItems.</summary>
        public ObservableCollection<OverlayItem> OverlayItems => _sensorValueViewModel.OverlayItems;

        /// <summary>Exposes the sensor sub-ViewModel (font size, items).</summary>
        public DataOverlayViewModel DataOverlayViewModel => _sensorValueViewModel;

        /// <summary>Exposes the camera display settings (grid, overlay toggles).</summary>
        public CameraSettingsViewModel CameraSettings => _cameraSettings;

        #endregion

        #region Commands

        public ICommand ToggleCameraCommand { get; }
        public ICommand CaptureCommand { get; }
        public ICommand SetPhotoModeCommand { get; }
        public ICommand SetVideoModeCommand { get; }
        public ICommand ToggleFlashCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenCameraSettingsCommand { get; }
        public ICommand CloseSettingsCommand { get; }
        public ICommand OpenGalleryCommand { get; }

        #endregion

        #region Constructor

        public MainPageViewModel(
            SensorHelper sensorHelper,
            DataOverlayViewModel sensorValueViewModel,
            CameraSettingsViewModel cameraSettings)
        {
            _sensorHelper = sensorHelper;
            _sensorValueViewModel = sensorValueViewModel;
            _cameraSettings = cameraSettings;

            ToggleCameraCommand = new Command(async () => await ToggleCameraAsync());
            CaptureCommand = new Command(async () => await CaptureAsync());
            SetPhotoModeCommand = new Command(() => SelectedMode = CaptureMode.Photo);
            SetVideoModeCommand = new Command(() => SelectedMode = CaptureMode.Video);
            ToggleFlashCommand = new Command(ToggleFlash);
            OpenSettingsCommand = new Command(() => IsSettingsVisible = true);
            OpenCameraSettingsCommand = new Command(() => IsCameraSettingsVisible = true);
            CloseSettingsCommand = new Command(async () => await CloseSettingsAsync());
            OpenGalleryCommand = new Command(async () => await OpenGalleryAsync());

            _ = SafeInitializeAsync();

            if (Application.Current is Application app)
            {
                app.PageAppearing += OnPageAppearing;
                app.PageDisappearing += OnPageDisappearing;
            }
        }

        #endregion

        #region Initialization

        private async Task SafeInitializeAsync()
        {
            try { await InitializeAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] InitializeAsync failed: {ex.Message}");
                PermissionStatus = "Initialization error";
            }
        }

        private async Task InitializeAsync()
        {
            // Yield main thread immediately — prevents ANR. Allows UI to render
            // before permission dialogs and sensor initialization block.
            await Task.Yield();

            if (_isDestroyed) return;

            // Restore last thumbnail immediately so it's never blank at startup
            LoadLastCaptureImage();

            PermissionStatus = "Requesting permissions...";

            bool cameraOk = false;
            try { cameraOk = await RequestCameraPermissionAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Camera permission error: {ex.Message}");
            }

            if (_isDestroyed) return;
            HasCameraPermission = cameraOk;
            PermissionStatus = cameraOk ? "Camera ready" : "Camera permission denied";

            // If camera permission just granted and view is ready, start preview immediately.
            // Handles race where OnAppearing ran before permissions were set.
            if (cameraOk && _cameraView != null && !IsPreviewRunning)
                await StartCameraPreviewAsync();

            try { await RequestLocationPermissionAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Location permission error: {ex.Message}");
            }

            if (_isDestroyed) return;

#if ANDROID
            try { await RequestStoragePermissionsAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Storage permission error: {ex.Message}");
            }
#endif

            if (_isDestroyed) return;

            await _sensorValueViewModel.ApplyPersistedSettingsAsync();

            if (_isDestroyed) return;

            await _sensorHelper.StartAsync();
        }

        /// <summary>
        /// Restores the thumbnail of the last captured photo from persisted preferences.
        /// Always uses a plain file path (ImageSource.FromFile) — never content://.
        /// </summary>
        private void LoadLastCaptureImage()
        {
            try
            {
                // Prefer the thumb path key; fall back to legacy key for existing installs
                var path = Preferences.Get(PrefLastThumbPath, string.Empty);
                if (string.IsNullOrEmpty(path))
                {
                    // Migrate: check if FileHelper.ThumbPath exists from a previous session
                    var defaultThumb = FileHelper.ThumbPath;
                    if (File.Exists(defaultThumb))
                        path = defaultThumb;
                }
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

                _lastThumbPath = path;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try { LastCaptureImage = ImageSource.FromFile(path); }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[MainPageViewModel] LoadLastCaptureImage display error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] LoadLastCaptureImage error: {ex.Message}");
            }
        }

        private async Task<bool> RequestCameraPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Camera>();

            var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (micStatus != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

            return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
        }

        private async Task<bool> RequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
        }

#if ANDROID
        private async Task RequestStoragePermissionsAsync()
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
                {
                    var imgStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
                    if (imgStatus != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                        await Permissions.RequestAsync<Permissions.Photos>();
                }
                else
                {
                    var readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                    if (readStatus != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                        await Permissions.RequestAsync<Permissions.StorageRead>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Storage permission error: {ex.Message}");
            }
        }
#endif

        #endregion

        #region Lifecycle

        private void EnsureWindowLifecycle()
        {
            if (_lifecycleSubscribed || _isDestroyed) return;
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window == null) return;

            window.Resumed += OnWindowResumed;
            window.Stopped += OnWindowStopped;
            window.Destroying += OnWindowDestroying;
            _lifecycleSubscribed = true;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window lifecycle subscribed");
        }

        private async void OnWindowResumed(object? sender, EventArgs e)
        {
            if (_isDestroyed) return;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Resumed");

            try
            {
                // Brief pause so the Activity fully re-enters foreground before touching camera
                await Task.Delay(400);
                if (_isDestroyed) return;

                await _sensorHelper.StartAsync();

                if (_cameraView != null && HasCameraPermission && !_isDestroyed)
                {
                    IsPreviewRunning = false;
                    bool ok = false;
                    try { ok = await CameraHelper.StartPreviewAsync(_cameraView); }
                    catch (Exception cex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[MainPageViewModel] Camera resume (attempt 1) error: {cex.Message}");
                        await Task.Delay(800);
                        if (!_isDestroyed && _cameraView != null)
                        {
                            try { ok = await CameraHelper.StartPreviewAsync(_cameraView); }
                            catch (Exception cex2)
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"[MainPageViewModel] Camera resume (attempt 2) error: {cex2.Message}");
                            }
                        }
                    }
                    IsPreviewRunning = ok;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Resume error: {ex.Message}");
            }
        }

        private async void OnWindowStopped(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Stopped");

            try
            {
                if (_isRecording && _cameraView != null)
                    await StopRecordingAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Stop recording on pause error: {ex.Message}");
            }

            _sensorHelper.Stop();

            if (_cameraView != null)
            {
                try { CameraHelper.StopPreview(_cameraView); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] StopPreview error: {ex.Message}");
                }
                IsPreviewRunning = false;
            }
        }

        private void OnWindowDestroying(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Destroying");
            _isDestroyed = true;

            if (sender is Window window)
            {
                window.Resumed -= OnWindowResumed;
                window.Stopped -= OnWindowStopped;
                window.Destroying -= OnWindowDestroying;
            }

            if (Application.Current is Application app)
            {
                app.PageAppearing -= OnPageAppearing;
                app.PageDisappearing -= OnPageDisappearing;
            }

            _sensorHelper.Stop();
            _sensorValueViewModel.Dispose();
            _recordingTimer?.Dispose();
            _recordingTimer = null;
            _cameraView = null;
        }

        private void OnPageAppearing(object? sender, Page page)
        {
            EnsureWindowLifecycle();
        }

        private void OnPageDisappearing(object? sender, Page page)
        {
            // Window.Stopped handles background transitions
        }

        #endregion

        #region Camera Operations

        /// <summary>
        /// Called by MainPage.OnAppearing after the CameraView is attached.
        /// Idempotent — safe to call on every appearance.
        /// </summary>
        public async Task OnViewReady(CameraView cameraView)
        {
            if (_isDestroyed) return;
            _cameraView = cameraView;

            if (!HasCameraPermission)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] No camera permission, skipping preview");
                return;
            }

            if (IsPreviewRunning) return;
            await StartCameraPreviewAsync();
        }

        private async Task StartCameraPreviewAsync()
        {
            if (_isDestroyed || _cameraView == null) return;
            if (IsPreviewRunning) return;

            var selected = await CameraHelper.SelectFirstAvailableCameraAsync(_cameraView);
            if (selected == null)
            {
                PermissionStatus = "No camera device found";
                IsPreviewRunning = false;
                return;
            }

            // Build zoom presets from actual hardware capabilities
            UpdateZoomPresets(selected);

            bool ok = await CameraHelper.StartPreviewAsync(_cameraView);
            IsPreviewRunning = ok;
            PermissionStatus = ok ? "Camera ready" : "Failed to start camera";
        }

        private async Task ToggleCameraAsync()
        {
            if (_cameraView == null || !IsPreviewRunning) return;

            CameraHelper.StopPreview(_cameraView);
            IsPreviewRunning = false;

            var newCamera = await CameraHelper.ToggleCameraDeviceAsync(_cameraView);
            if (newCamera != null)
                UpdateZoomPresets(newCamera);

            bool ok = await CameraHelper.StartPreviewAsync(_cameraView);
            IsPreviewRunning = ok;
        }

        #endregion

        #region Zoom

        /// <summary>
        /// Rebuilds <see cref="ZoomPresets"/> with standard zoom stops.
        /// </summary>
        private void UpdateZoomPresets(CameraInfo camera)
        {
            // CommunityToolkit.Maui CameraInfo doesn't expose min/max zoom bounds.
            // Use standard preset stops (1×, 2×, 3×, 5×) normalized to 0.0–1.0 range.
            float[] presetZooms = { 0.2f, 0.5f, 1.0f };

            System.Diagnostics.Debug.WriteLine(
                "[MainPageViewModel] Camera zoom presets: 0.2×, 0.5×, 1.0×");

            // Build collection — each ZoomPreset stores the normalized camera zoom factor
            ZoomPresets.Clear();
            foreach (var zoom in presetZooms)
            {
                var label = FormatZoomLabel(zoom);
                var preset = new ZoomPreset(label, zoom, SelectZoomPreset);
                ZoomPresets.Add(preset);
            }

            // Select 1.0 by default
            var defaultPreset = ZoomPresets
                .FirstOrDefault(p => Math.Abs(p.AbsoluteZoom - 1.0f) < 0.01f);
            if (defaultPreset != null)
                SelectZoomPreset(defaultPreset);

            System.Diagnostics.Debug.WriteLine(
                $"[MainPageViewModel] Built {ZoomPresets.Count} zoom presets");
        }

        private static string FormatZoomLabel(float zoom)
        {
            if (Math.Abs(zoom - MathF.Round(zoom)) < 0.05f)
                return zoom == 1f ? "1×" : $"{(int)MathF.Round(zoom)}";
            return $"{zoom:0.#}";
        }

        private void SelectZoomPreset(ZoomPreset? preset)
        {
            if (preset == null) return;

            foreach (var p in ZoomPresets)
                p.IsSelected = p == preset;

            if (_cameraView != null)
                CameraHelper.SetZoom(_cameraView, preset.AbsoluteZoom);

            System.Diagnostics.Debug.WriteLine(
                $"[MainPageViewModel] Zoom set to {preset.Label} ({preset.AbsoluteZoom:F1}x)");
        }

        #endregion

        #region Capture

        private async Task CaptureAsync()
        {
            if (_cameraView == null || !IsPreviewRunning) return;

            if (SelectedMode == CaptureMode.Photo)
                await CapturePhotoAsync();
            else
                await ToggleRecordingAsync();
        }

        private async Task CapturePhotoAsync()
        {
            if (_isCapturing) return;
            _isCapturing = true;

            try
            {
                await CameraHelper.TriggerCaptureAsync(_cameraView!);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] CapturePhotoAsync error: {ex.Message}");
                _isCapturing = false;
            }
        }

        /// <summary>
        /// Called by MainPage when the CameraView fires MediaCaptured.
        /// Saves the photo, updates the thumbnail (using the private file copy),
        /// and persists the thumb path so it survives restarts.
        /// </summary>
        public async Task OnMediaCaptured(Stream? stream)
        {
            if (_isDestroyed) return;

            try
            {
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[MainPageViewModel] OnMediaCaptured: null stream");
                    return;
                }

                var city = GetCityForFileName();
                var (galleryPath, thumbPath) = await FileHelper.SavePhotoAsync(stream, city);

                _lastGalleryPath = galleryPath;
                _lastThumbPath = thumbPath;

                // Persist thumb path (always a plain file — never content://)
                if (!string.IsNullOrEmpty(thumbPath))
                {
                    try { Preferences.Set(PrefLastThumbPath, thumbPath); }
                    catch (Exception pex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[MainPageViewModel] Preferences.Set error: {pex.Message}");
                    }
                }

                // Display thumbnail — always use the file copy, not content://
                if (!string.IsNullOrEmpty(thumbPath) && File.Exists(thumbPath))
                    LastCaptureImage = ImageSource.FromFile(thumbPath);

                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Photo saved. Gallery={galleryPath} Thumb={thumbPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] OnMediaCaptured error: {ex.Message}");
            }
            finally
            {
                _isCapturing = false;
            }
        }

        /// <summary>Called by MainPage when the CameraView fires MediaCaptureFailed.</summary>
        public void OnMediaCaptureFailed()
        {
            _isCapturing = false;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Photo capture failed");
        }

        #endregion

        #region Video Recording

        private async Task ToggleRecordingAsync()
        {
            if (_cameraView == null || !IsPreviewRunning) return;
            if (_isTogglingRecording) return;
            _isTogglingRecording = true;

            try
            {
                if (IsRecording)
                    await StopRecordingAsync();
                else
                    await StartRecordingAsync();
            }
            finally
            {
                _isTogglingRecording = false;
            }
        }

        private async Task StartRecordingAsync()
        {
            if (_cameraView == null) return;

            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Starting video recording...");
            bool ok = await CameraHelper.StartVideoRecordingAsync(_cameraView);
            if (ok)
            {
                IsRecording = true;
                _recordingStart = DateTime.Now;
                StartRecordingTimer();
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Recording started");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] StartVideoRecording returned false — check permissions");
            }
        }

        private async Task StopRecordingAsync()
        {
            if (_cameraView == null) return;

            StopRecordingTimer();

            Stream? videoStream = null;
            try
            {
                videoStream = await CameraHelper.StopVideoRecordingAsync(_cameraView);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] StopVideoRecording error: {ex.Message}");
            }
            finally
            {
                IsRecording = false;
            }

            // Save video if we got a stream
            if (videoStream != null)
            {
                try
                {
                    var city = GetCityForFileName();
                    var galleryPath = await FileHelper.SaveVideoAsync(videoStream, city);
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] Video published: {galleryPath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] SaveVideoAsync error: {ex.Message}");
                }
            }

            // Always restart preview after stopping recording
            if (!_isDestroyed && _cameraView != null)
            {
                try
                {
                    IsPreviewRunning = await CameraHelper.StartPreviewAsync(_cameraView);
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] Preview restarted after stop: {IsPreviewRunning}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] Restart preview after stop error: {ex.Message}");
                    IsPreviewRunning = false;
                }
            }
        }

        private void StartRecordingTimer()
        {
            _recordingTimer = new System.Timers.Timer(1000);
            _recordingTimer.Elapsed += OnRecordingTimerElapsed;
            _recordingTimer.AutoReset = true;
            _recordingTimer.Enabled = true;
        }

        private void StopRecordingTimer()
        {
            _recordingTimer?.Dispose();
            _recordingTimer = null;
            RecordingTimeText = "00:00";
        }

        private void OnRecordingTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var elapsed = DateTime.Now - _recordingStart;
            var text = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            MainThread.BeginInvokeOnMainThread(() => RecordingTimeText = text);
        }

        #endregion

        #region Flash

        private void ToggleFlash()
        {
            if (_cameraView == null) return;
            var mode = CameraHelper.CycleFlashMode(_cameraView);
            IsFlashOn = mode == CameraFlashMode.On;
        }

        #endregion

        #region Settings

        private async Task CloseSettingsAsync()
        {
            IsSettingsVisible = false;
            await SaveSensorSettingsAsync();
        }

        private async Task SaveSensorSettingsAsync()
        {
            try
            {
                await SettingsHelper.SaveOverlayItemsConfigurationAsync(
                    _sensorValueViewModel.OverlayItems);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Save settings error: {ex.Message}");
            }
        }

        #endregion

        #region Gallery

        private async Task OpenGalleryAsync()
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                Android.Content.Intent? intent = null;

                // Use the gallery URI if available; otherwise open gallery browser
                if (!string.IsNullOrEmpty(_lastGalleryPath)
                    && _lastGalleryPath.StartsWith("content://",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var uri = Android.Net.Uri.Parse(_lastGalleryPath);
                    intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                    intent.SetDataAndType(uri, "image/jpeg");
                    intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
                    intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                    intent.AddFlags(Android.Content.ActivityFlags.ClearTop);
                }
                else
                {
                    intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                    intent.SetType("image/*");
                    intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                }

                try
                {
                    context.StartActivity(intent);
                }
                catch (Android.Content.ActivityNotFoundException)
                {
                    var chooser = Android.Content.Intent.CreateChooser(intent, "Open with");
                    chooser?.AddFlags(Android.Content.ActivityFlags.NewTask);
                    if (chooser != null) context.StartActivity(chooser);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] OpenGallery error: {ex.Message}");
            }
#else
            if (!string.IsNullOrEmpty(_lastThumbPath))
            {
                try
                {
                    await Launcher.Default.OpenAsync(
                        new OpenFileRequest("Open Image",
                            new ReadOnlyFile(_lastThumbPath)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] OpenGallery error: {ex.Message}");
                }
            }
#endif
        }

        #endregion

        #region Helpers

        private string GetCityForFileName()
        {
            var cityItem = OverlayItems.FirstOrDefault(s => s.Name == "City");
            var city = cityItem?.Value;
            return string.IsNullOrWhiteSpace(city) || city == "Unknown" ? "CekliCam" : city;
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        #endregion
    }
}