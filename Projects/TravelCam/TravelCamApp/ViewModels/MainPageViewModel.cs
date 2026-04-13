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
using System.Threading;
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
        private string? _lastThumbPath;       // plain file path — for thumbnail display
        private bool _isCapturing;
        private bool _isTogglingRecording;
        private string _permissionStatus = "Initializing...";
        private bool _hasCameraPermission;

        // Overlay visibility
        private bool _isSensorOverlayVisible = true;
        private bool _isSettingsVisible;
        private bool _isCameraSettingsVisible;
        private bool _isImageViewerVisible;

        // Flash
        private bool _isFlashOn;

        // Camera count — used to show/hide zoom strip
        private int _cameraCount;

        // Aspect ratio crop bars
        private double _aspectTopBarHeight;
        private double _aspectBottomBarHeight;

        // Recording display timer
        private System.Timers.Timer? _recordingTimer;
        private DateTime _recordingStart;

        // Gallery viewer
        private ObservableCollection<string> _galleryImagePaths = new();
        private int _currentImageIndex;

        // Lifecycle guards — instance-level only (no static flag)
        private List<Window> _trackedWindows = new();  // track all windows for proper cleanup
        private bool _windowSubscribed;
        private bool _isDestroyed;
        private Window? _subscribedWindow; // stored so unsubscription is guaranteed

        // Concurrency guard — serializes camera start/stop/toggle to prevent crashes
        private readonly SemaphoreSlim _cameraLock = new(1, 1);

        #region Static State Management

        /// <summary>
        /// Static flag tracking whether the app has been fully initialized at least once.
        /// Initialized to false: on first app launch, full initialization MUST happen.
        /// Set to true only after successful initialization completes.
        /// Reset to false in OnWindowDestroying so that after Activity recreation,
        /// the next OnViewReady call triggers full reinitialization.
        /// </summary>
        private static bool _isAppInitialized = false;

        /// <summary>
        /// Static log file path for detailed crash diagnostics.
        /// </summary>
        private const string CrashLogPath = "crash_diagnostics.log";

        #endregion

        // Preference keys
        private const string PrefLastThumbPath = "LastThumbPath";

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Event fired when the camera is selected and ready for overlay container positioning.
        /// Subscribed to by MainPage to trigger CameraViewChildrenContainer sizing calculation.
        /// </summary>
        public event EventHandler? CameraReady;

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

        public bool IsImageViewerVisible
        {
            get => _isImageViewerVisible;
            set { _isImageViewerVisible = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> GalleryImagePaths
        {
            get => _galleryImagePaths;
            set { _galleryImagePaths = value; OnPropertyChanged(); OnPropertyChanged(nameof(ImagePositionText)); }
        }

        public int CurrentImageIndex
        {
            get => _currentImageIndex;
            set
            {
                if (_currentImageIndex == value) return;
                _currentImageIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImagePositionText));
                OnPropertyChanged(nameof(CurrentImageItem));
            }
        }

        // For two-way binding with CollectionView SelectedItem
        public string? CurrentImageItem
        {
            get => _galleryImagePaths.Count > 0 && _currentImageIndex >= 0 && _currentImageIndex < _galleryImagePaths.Count
                ? _galleryImagePaths[_currentImageIndex]
                : null;
            set
            {
                if (value != null && _galleryImagePaths.Contains(value))
                {
                    var index = _galleryImagePaths.IndexOf(value);
                    if (index != _currentImageIndex)
                        CurrentImageIndex = index;
                }
            }
        }

        public string ImagePositionText => _galleryImagePaths.Count > 0
            ? $"{CurrentImageIndex + 1} / {_galleryImagePaths.Count}"
            : string.Empty;

        public bool IsFlashOn
        {
            get => _isFlashOn;
            set { _isFlashOn = value; OnPropertyChanged(); }
        }

        /// <summary>True when the device has more than one camera (shows zoom preset strip).</summary>
        public bool ShowZoomPresets => _cameraCount > 1;

        /// <summary>Height of the top black letterbox bar for the selected aspect ratio.</summary>
        public double AspectTopBarHeight
        {
            get => _aspectTopBarHeight;
            set
            {
                if (Math.Abs(_aspectTopBarHeight - value) < 0.5) return;
                _aspectTopBarHeight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasAspectBars));
            }
        }

        /// <summary>Height of the bottom black letterbox bar for the selected aspect ratio.</summary>
        public double AspectBottomBarHeight
        {
            get => _aspectBottomBarHeight;
            set
            {
                if (Math.Abs(_aspectBottomBarHeight - value) < 0.5) return;
                _aspectBottomBarHeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>True when letterbox bars should be shown (any non-FullScreen aspect ratio with non-zero bars).</summary>
        public bool HasAspectBars => _aspectTopBarHeight > 1;

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
        public ICommand CloseImageViewerCommand { get; }
        public ICommand ShareImageCommand { get; }
        public ICommand DeleteImageCommand { get; }
        public ICommand PlayVideoCommand { get; }

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

            ToggleCameraCommand = new Command(async () => await SafeExecuteAsync(ToggleCameraAsync));
            CaptureCommand = new Command(async () => await SafeExecuteAsync(CaptureAsync));
            SetPhotoModeCommand = new Command(() => SelectedMode = CaptureMode.Photo);
            SetVideoModeCommand = new Command(() => SelectedMode = CaptureMode.Video);
            ToggleFlashCommand = new Command(ToggleFlash);
            OpenSettingsCommand = new Command(() => IsSettingsVisible = true);
            OpenCameraSettingsCommand = new Command(() => IsCameraSettingsVisible = true);
            CloseSettingsCommand = new Command(async () => await SafeExecuteAsync(CloseSettingsAsync));
            OpenGalleryCommand = new Command(() => OpenImageViewer());
            CloseImageViewerCommand = new Command(() => IsImageViewerVisible = false);
            ShareImageCommand = new Command(async () => await SafeExecuteAsync(ShareCurrentImageAsync));
            DeleteImageCommand = new Command(async () => await SafeExecuteAsync(DeleteCurrentImageAsync));
            PlayVideoCommand = new Command<string>(async (filePath) => await SafeExecuteAsync(() => PlayVideoAsync(filePath)));

            _ = SafeInitializeAsync();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Checks if the app needs full reinitialization (first launch or after being swiped away).
        /// Returns true if resources need to be recreated.
        /// </summary>
        public static bool NeedsReinitialization()
        {
            return !_isAppInitialized;
        }

        /// <summary>
        /// Resets the app initialization state, forcing full reinitialization on next launch.
        /// Called after successful camera setup or when explicitly requested for clean restart.
        /// </summary>
        public static void ResetInitializationState()
        {
            _isAppInitialized = false;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] App initialization state reset (needs reinitialization)");
        }

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

            if (_isDestroyed) return;

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

            // Mark app as successfully initialized — required for proper Activity recreation recovery
            _isAppInitialized = true;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] App fully initialized successfully");

            // If the camera view is already ready (OnAppearing → OnViewReady → waiting for init),
            // start the preview now. Otherwise, OnViewReady will start it when it detects initialization complete.
            if (!_isDestroyed && _cameraView != null && !IsPreviewRunning)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] InitializeAsync complete: camera view ready, starting preview");
                await StartCameraPreviewAsync();
            }
        }

        /// <summary>
        /// Restores the thumbnail of the last captured photo from persisted preferences.
        /// Uses ImageSource.FromStream() for reliable loading from app cache — never content://.
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
                    try
                    {
                        LastCaptureImage = ImageSource.FromStream(() =>
                            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                    }
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

        /// <summary>
        /// Check-before-request helper with Android ShouldShowRationale support and
        /// iOS one-shot denial guard. Follows the MAUI permissions best-practice pattern.
        /// </summary>
        private static async Task<Microsoft.Maui.ApplicationModel.PermissionStatus> CheckAndRequestAsync<T>(string rationale)
            where T : Permissions.BasePermission, new()
        {
            var status = await Permissions.CheckStatusAsync<T>();
            if (status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                return status;

            // iOS shows the system dialog only once — re-requesting after denial is a no-op.
            if (status == Microsoft.Maui.ApplicationModel.PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
                return status;

            // Android: show rationale if the user previously denied without "Don't ask again".
            if (Permissions.ShouldShowRationale<T>())
            {
                var page = Shell.Current ?? Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                    await page.DisplayAlertAsync("Permission needed", rationale, "OK");
            }

            return await Permissions.RequestAsync<T>();
        }

        private async Task<bool> RequestCameraPermissionAsync()
        {
            var status = await CheckAndRequestAsync<Permissions.Camera>(
                "Camera access is required to capture photos and video.");

            // Request microphone independently (needed for video recording).
            await CheckAndRequestAsync<Permissions.Microphone>(
                "Microphone access is required to record video with audio.");

            return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
        }

        private async Task<bool> RequestLocationPermissionAsync()
        {
            var status = await CheckAndRequestAsync<Permissions.LocationWhenInUse>(
                "Location access is used to tag your photos with GPS coordinates.");
            return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
        }

#if ANDROID
        private async Task RequestStoragePermissionsAsync()
        {
            try
            {
                // API 33+ (Android 13): granular media permissions replace the broad StorageRead.
                // StorageRead always returns Granted on API 33+ — use Photos/Media instead.
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
                {
                    await CheckAndRequestAsync<Permissions.Photos>(
                        "Photo library access is required to save captured images to the gallery.");
                }
                else
                {
                    await CheckAndRequestAsync<Permissions.StorageRead>(
                        "Storage access is required to save photos to the gallery.");
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

        private void SubscribeWindowLifecycle(CameraView cameraView)
        {
            if (_isDestroyed) return;

            // Prefer the visual-tree Window (reliable), fall back to first window in list.
            var window = cameraView.Window ?? Application.Current?.Windows.FirstOrDefault();
            if (window == null) return;

            // If already subscribed to this exact window instance, nothing to do.
            if (_windowSubscribed && ReferenceEquals(_subscribedWindow, window)) return;

            // Track this window for proper cleanup on app close/restart
            _trackedWindows.Add(window);

            // Unsubscribe from any previous window before subscribing to the new one.
            // This handles the edge case where the Window object is replaced (e.g. activity recreation).
            if (_windowSubscribed && _subscribedWindow != null)
            {
                _subscribedWindow.Resumed -= OnWindowResumed;
                _subscribedWindow.Stopped -= OnWindowStopped;
                _subscribedWindow.Destroying -= OnWindowDestroying;
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window lifecycle re-subscribing (window changed)");
            }

            window.Resumed += OnWindowResumed;
            window.Stopped += OnWindowStopped;
            window.Destroying += OnWindowDestroying;
            _subscribedWindow = window;
            _windowSubscribed = true;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window lifecycle subscribed");
        }

        private async void OnWindowResumed(object? sender, EventArgs e)
        {
            if (_isDestroyed) return;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Resumed");

            try
            {
                // Start sensors immediately — they don't need hardware warm-up time.
                await _sensorHelper.StartAsync();
                if (_isDestroyed) return;

                // Brief pause so the Activity fully re-enters foreground before touching camera.
                // During this delay, OnAppearing → OnViewReady may already start the camera.
                await Task.Delay(400);
                if (_isDestroyed) return;

                if (_cameraView != null && HasCameraPermission && !_isDestroyed)
                {
                    if (!await TryAcquireCameraLockAsync()) return;
                    try
                    {
                        if (_isDestroyed || _cameraView == null) return;

                        // OnViewReady (from OnAppearing) may have already started the camera
                        // during the 400 ms delay above. Skip restart to avoid a double-start
                        // which can crash Camera2 on some devices.
                        if (IsPreviewRunning)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "[MainPageViewModel] Camera already running on resume — skipping restart");
                            return;
                        }

                        bool ok = false;
                        try { ok = await CameraHelper.StartPreviewAsync(_cameraView); }
                        catch (Exception cex)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"[MainPageViewModel] Camera resume (attempt 1) error: {cex.Message}");
                            await Task.Delay(500);
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
                    finally
                    {
                        ReleaseCameraLock();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Resume error: {ex.Message}");
            }
        }

        private async void OnWindowStopped(object? sender, EventArgs e)
        {
            if (_isDestroyed) return;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Stopped");

            try
            {
                // Do NOT restart the preview here — we're going to background.
                if (_isRecording && _cameraView != null)
                    await StopRecordingAsync(restartPreview: false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Stop recording on pause error: {ex.Message}");
            }

            // If OnWindowDestroying fired during the awaits above, bail out — Destroying
            // already stopped the camera and cleaned up state.
            if (_isDestroyed) return;

            _sensorHelper.Stop();

            // Capture _cameraView before any await; OnWindowDestroying may null it concurrently.
            var cameraView = _cameraView;
            if (cameraView == null) return;

            // Stop the preview under the camera lock so it doesn't race with
            // any in-flight start operation (e.g. a late OnWindowResumed retry).
            if (!await TryAcquireCameraLockAsync(timeoutMs: 2000))
            {
                // Lock held too long — try stopping without the lock as last resort.
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] OnWindowStopped: lock timeout, forcing StopPreview");
                // Re-check; Destroying may have nulled _cameraView while we waited.
                var cv = _cameraView;
                if (cv != null)
                    try { CameraHelper.StopPreview(cv); } catch { }
                IsPreviewRunning = false;
                return;
            }
            try
            {
                // Double-check after acquiring the lock.
                var cv = _cameraView;
                if (cv != null)
                {
                    CameraHelper.StopPreview(cv);
                    IsPreviewRunning = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] StopPreview error: {ex.Message}");
                IsPreviewRunning = false;
            }
            finally
            {
                ReleaseCameraLock();
            }
        }

        private void OnWindowDestroying(object? sender, EventArgs e)
        {
            // Set destroyed flag FIRST to prevent any new operations from starting.
            _isDestroyed = true;

            // Clear recording/capture state so the VM is in a clean state.
            // Do NOT await StopRecordingAsync here — it would block the main thread (ANR risk).
            // StopPreview below will abort any active MediaRecorder session on Camera2.
            IsRecording = false;
            _isCapturing = false;
            _isTogglingRecording = false;
            IsPreviewRunning = false;
            StopRecordingTimer();

            // Unsubscribe from ALL tracked windows — critical for force-close scenario.
            // The sender cast is unreliable if the event fires from a different source,
            // so we use our stored list of window references.
            foreach (var window in _trackedWindows)
            {
                try
                {
                    window.Resumed -= OnWindowResumed;
                    window.Stopped -= OnWindowStopped;
                    window.Destroying -= OnWindowDestroying;
                }
                catch { /* ignore unsubscription errors */ }
            }
            _trackedWindows.Clear();

            // Clear the single tracked reference as well.
            if (_subscribedWindow != null)
            {
                _subscribedWindow.Resumed -= OnWindowResumed;
                _subscribedWindow.Stopped -= OnWindowStopped;
                _subscribedWindow.Destroying -= OnWindowDestroying;
            }

            _windowSubscribed = false;

            // Stop sensors and camera view.
            _sensorHelper.Stop();
            if (_cameraView != null)
            {
                try { CameraHelper.StopPreview(_cameraView); }
                catch { /* best effort */ }
            }
            _cameraView = null;

            // Reset the app initialization flag so next app launch performs full reinitialization.
            // This is critical for Activity recreation scenarios where the process survives.
            _isAppInitialized = false;
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] App initialization flag reset (will reinitialize on next launch)");

            // NOTE: Do NOT dispose _cameraLock here.
            // SemaphoreSlim(1,1) holds no native resources — the GC reclaims it if this
            // ViewModel is truly freed, and if the process survives Activity recreation
            // (Shell page cache keeps the ViewModel alive), OnViewReady will reuse the
            // same semaphore safely after resetting _isDestroyed = false.
        }

        /// <summary>
        /// SYNCHRONOUS camera preview stop.
        /// Called from App.xaml.cs during shutdown to ensure cleanup blocks process termination.
        /// Must NOT be async to prevent thread pool scheduling delays.
        /// </summary>
        public void StopCameraPreviewSync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] StopCameraPreviewSync called");

                // Stop sensors synchronously
                _sensorHelper.Stop();

                // Stop camera preview synchronously
                var cameraView = _cameraView;
                if (cameraView != null)
                {
                    try
                    {
                        CameraHelper.StopPreview(cameraView);
                        System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Camera preview stopped (SYNC)");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[MainPageViewModel] StopPreview error (SYNC): {ex.GetType().Name}");
                    }
                    finally
                    {
                        IsPreviewRunning = false;
                        _cameraView = null;
                    }
                }

                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] StopCameraPreviewSync complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] StopCameraPreviewSync error: {ex.GetType().Name} — {ex.Message}");
            }
        }

        #endregion

        #region Camera Operations

        /// <summary>
        /// Called by MainPage.OnAppearing after the CameraView is attached.
        /// Idempotent — safe to call on every appearance.
        /// </summary>
        public async Task OnViewReady(CameraView cameraView)
        {
            _cameraView = cameraView;

            // ============================================================
            // CRITICAL: Check if full app initialization is complete
            // ============================================================
            bool needsFullReinit = NeedsReinitialization();
            if (needsFullReinit)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] OnViewReady: Full initialization NOT complete yet — SafeInitializeAsync is running");

                // Just set the camera view and return immediately — DO NOT WAIT!
                // Waiting blocks the UI thread and causes frame lag/hangs.
                // SafeInitializeAsync() from constructor is running in background and will handle:
                // - Permission requests
                // - Sensor startup
                // - Camera preview start (when _cameraView is set and permissions granted)

                // Reset the destroyed flag since we have a fresh view
                _isDestroyed = false;

                // Clear any stale camera selection from previous session
                if (_cameraView.SelectedCamera != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[MainPageViewModel] OnViewReady: clearing stale SelectedCamera (full reinit pending)");
                    _cameraView.SelectedCamera = null;
                }

                // Subscribe to window lifecycle — this is needed immediately
                SubscribeWindowLifecycle(cameraView);

                // ✅ RETURN IMMEDIATELY — do not wait!
                // InitializeAsync will start the preview when it detects _cameraView is set
                return;
            }

            // ============================================================
            // Handle Activity recreation recovery (not full reinit)
            // ============================================================
            bool wasDestroyed = _isDestroyed;
            if (wasDestroyed && !needsFullReinit)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] OnViewReady: recovering after Activity recreation");
                _isDestroyed = false;
                IsPreviewRunning = false;
                IsRecording = false;
                _isCapturing = false;
                _isTogglingRecording = false;
                _windowSubscribed = false;
                _subscribedWindow = null;
                StopRecordingTimer();

                // Restart sensors — they were stopped in OnWindowDestroying and
                // OnWindowResumed may not fire on the Created→Activated path.
                try { await _sensorHelper.StartAsync(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] Sensor restart on recovery error: {ex.Message}");
                }

                if (_isDestroyed) return; // guard against concurrent destroy during sensor await
            }

            // Clear stale camera selection after activity recreation
            // (The CameraView.SelectedCamera might hold a reference to a destroyed Camera2 session)
            if ((wasDestroyed || needsFullReinit) && _cameraView.SelectedCamera != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] OnViewReady: clearing stale SelectedCamera after recreation");
                _cameraView.SelectedCamera = null;
            }

            // Subscribe to window lifecycle events now that the view is attached.
            // Uses the visual-tree Window so we get the correct instance even during
            // activity recreation in the same process.
            SubscribeWindowLifecycle(cameraView);

            // At this point, full initialization must be complete (either from SafeInitializeAsync
            // or this is Activity recreation recovery). Permissions should be granted.
            if (!HasCameraPermission)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] No camera permission, skipping preview");
                PermissionStatus = "Camera permission not granted";
                return;
            }

            if (IsPreviewRunning)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[MainPageViewModel] OnViewReady: Camera preview already running");
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                "[MainPageViewModel] OnViewReady: Starting camera preview");
            await StartCameraPreviewAsync();
        }

        private async Task StartCameraPreviewAsync()
        {
            if (_isDestroyed || _cameraView == null) return;
            if (IsPreviewRunning) return;

            if (!await TryAcquireCameraLockAsync()) return; // timeout: avoid deadlock
            try
            {
                if (_isDestroyed || _cameraView == null || IsPreviewRunning) return;

                // Only enumerate and select cameras when none is selected yet.
                // Re-running GetAvailableCameras() on every resume is an extra failure
                // point (hardware may not be ready), resets front/rear selection, and
                // is simply unnecessary when the camera was already chosen.
                if (_cameraView.SelectedCamera == null)
                {
                    var selected = await CameraHelper.SelectFirstAvailableCameraAsync(_cameraView);
                    if (selected == null)
                    {
                        PermissionStatus = "No camera device found";
                        IsPreviewRunning = false;
                        return;
                    }

                    // Initialize MediaStore resolver the first time only
#if ANDROID
                    try
                    {
                        var context = Android.App.Application.Context;
                        var resolver = context?.ContentResolver;
                        if (resolver != null)
                            Helpers.FileHelper.InitializeMediaStoreResolver(resolver);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[MainPageViewModel] Failed to initialize MediaStore resolver: {ex.Message}");
                    }
#endif

                    // Get total camera count to decide whether to show zoom strip
                    _cameraCount = await CameraHelper.GetCameraCountAsync(_cameraView);
                    OnPropertyChanged(nameof(ShowZoomPresets));

                    // Build zoom presets from actual hardware capabilities
                    UpdateZoomPresets(_cameraCount);
                }

                if (_isDestroyed || _cameraView == null) return;

                bool ok = await CameraHelper.StartPreviewAsync(_cameraView);
                IsPreviewRunning = ok;
                PermissionStatus = ok ? "Camera ready" : "Failed to start camera";

                // Re-apply flash state — StopCameraPreview resets hardware to Off.
                if (ok && _cameraView != null)
                    _cameraView.CameraFlashMode = _isFlashOn ? CameraFlashMode.On : CameraFlashMode.Off;

                // Fire CameraReady event so MainPage can size the CameraViewChildrenContainer
                if (ok && _cameraView?.SelectedCamera != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[MainPageViewModel] Camera ready, firing CameraReady event");
                    CameraReady?.Invoke(this, EventArgs.Empty);
                }
            }
            finally
            {
                ReleaseCameraLock();
            }
        }

        private async Task ToggleCameraAsync()
        {
            if (_isDestroyed || _cameraView == null || !IsPreviewRunning) return;

            if (!await TryAcquireCameraLockAsync()) return;
            try
            {
                if (_isDestroyed || _cameraView == null) return;

                CameraHelper.StopPreview(_cameraView);
                IsPreviewRunning = false;

                // Reinitialize MediaStore resolver after camera toggle
#if ANDROID
                try
                {
                    var context = Android.App.Application.Context;
                    var resolver = context?.ContentResolver;
                    if (resolver != null)
                    {
                        Helpers.FileHelper.InitializeMediaStoreResolver(resolver);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Failed to reinitialize MediaStore resolver: {ex.Message}");
                }
#endif

                var newCamera = await CameraHelper.ToggleCameraDeviceAsync(_cameraView);
                if (newCamera != null)
                    UpdateZoomPresets(_cameraCount);

                bool ok = await CameraHelper.StartPreviewAsync(_cameraView);
                IsPreviewRunning = ok;

                // Re-apply flash state after toggle — hardware resets to Off on each open.
                if (ok && _cameraView != null)
                    _cameraView.CameraFlashMode = _isFlashOn ? CameraFlashMode.On : CameraFlashMode.Off;
            }
            finally
            {
                ReleaseCameraLock();
            }
        }

        #endregion

        #region Zoom

        /// <summary>
        /// Rebuilds <see cref="ZoomPresets"/> with standard zoom stops based on camera count.
        /// CommunityToolkit.Maui ZoomFactor is an absolute multiplier (1.0 = 1× optical).
        /// </summary>
        private void UpdateZoomPresets(int cameraCount)
        {
            // Choose presets based on how many camera devices the hardware reports.
            // Single-camera devices have ShowZoomPresets=false so this list is never shown,
            // but we populate it anyway in case the count updates later.
            float[] presetZooms = cameraCount >= 3
                ? new[] { 0.6f, 1.0f, 2.0f, 5.0f }   // ultra-wide + main + tele
                : new[] { 1.0f, 2.0f, 5.0f };          // main + tele (2-camera or fallback)

            System.Diagnostics.Debug.WriteLine(
                $"[MainPageViewModel] Building zoom presets for {cameraCount} cameras");

            ZoomPresets.Clear();
            foreach (var zoom in presetZooms)
            {
                var label = FormatZoomLabel(zoom);
                var preset = new ZoomPreset(label, zoom, SelectZoomPreset);
                ZoomPresets.Add(preset);
            }

            // Select 1× by default
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
            // Allow video stop even if preview is not running (preview stops during recording)
            if (_isDestroyed || _cameraView == null) return;
            if (SelectedMode == CaptureMode.Photo && !IsPreviewRunning) return;

            if (SelectedMode == CaptureMode.Photo)
                await CapturePhotoAsync();
            else
                await ToggleRecordingAsync();
        }

        private async Task CapturePhotoAsync()
        {
            if (_isCapturing || _cameraView == null) return;
            _isCapturing = true;

            try
            {
                await CameraHelper.TriggerCaptureAsync(_cameraView);
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
                _ = galleryPath; // published to MediaStore; not stored in VM

                UpdateLastCapture(thumbPath);
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
            // Allow stopping even if preview is not running (preview stops during recording)
            if (_isDestroyed || _cameraView == null) return;
            if (!IsRecording && !IsPreviewRunning) return; // Only start requires preview running
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
            if (_cameraView == null)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] StartRecordingAsync: _cameraView is null!");
                return;
            }

            if (!IsPreviewRunning)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] StartRecordingAsync: Preview is NOT running! Cannot record.");
                return;
            }

            // Check microphone permission (required for video)
            var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (micStatus != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Microphone permission NOT granted — requesting...");
                await RequestCameraPermissionAsync(); // requests both camera and mic
                micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
                if (micStatus != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Microphone permission denied — cannot record video");
                    // Optionally show alert to user
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Starting video recording... Camera.SelectedCamera={_cameraView.SelectedCamera?.Name}, IsAvailable={_cameraView.IsAvailable}, IsBusy={_cameraView.IsBusy}");

            if (_cameraView.IsBusy)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Camera is busy — cannot start recording now");
                return;
            }

            // Some Android devices require stopping preview to free camera for MediaRecorder
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Stopping preview before recording...");
            CameraHelper.StopPreview(_cameraView);
            IsPreviewRunning = false;
            await Task.Delay(150);

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
                    "[MainPageViewModel] StartVideoRecording returned false — check camera state and permissions");
            }
        }

        /// <param name="restartPreview">
        /// Pass <c>false</c> when stopping because the app is going to background
        /// (OnWindowStopped). The camera will be stopped by the caller immediately
        /// after, so restarting preview only to stop it again is wasteful and can
        /// cause Camera2 instability on some devices.
        /// </param>
        private async Task StopRecordingAsync(bool restartPreview = true)
        {
            if (_cameraView == null)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] StopRecordingAsync: _cameraView is null!");
                return;
            }

            StopRecordingTimer();

            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] StopRecordingAsync: Starting stop operation...");
            Stream? videoStream = null;
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Calling CameraHelper.StopVideoRecordingAsync...");
                var stopTask = CameraHelper.StopVideoRecordingAsync(_cameraView);
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Waiting for stopTask to complete...");
                videoStream = await stopTask;
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] StopVideoRecordingAsync returned. Stream exists: {videoStream != null}, CanSeek: {videoStream?.CanSeek}, Length: {videoStream?.Length}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] StopVideoRecording exception: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                IsRecording = false;
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] IsRecording set to false");
            }

            // Force stop preview if camera is still busy (reset) - DO THIS BEFORE saving to avoid locks
            if (_cameraView != null && _cameraView.IsBusy)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Camera still busy — forcing StopPreview");
                CameraHelper.StopPreview(_cameraView);
                await Task.Delay(200);
            }

            // Save video if we got a non-empty stream
            if (videoStream != null && videoStream.Length > 0)
            {
                try
                {
                    using (videoStream) // Ensure the stream is disposed after saving to release file lock
                    {
                        var city = GetCityForFileName();
                        var (galleryPath, thumbPath) = await FileHelper.SaveVideoAsync(videoStream, city);
                        System.Diagnostics.Debug.WriteLine(
                            $"[MainPageViewModel] Video published: {galleryPath}");
                        UpdateLastCapture(thumbPath);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] SaveVideoAsync error: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Video stream is null or empty — not saving.");
                videoStream?.Dispose();
            }

            // Restart preview only when appropriate (not when going to background).
            if (restartPreview && !_isDestroyed && _cameraView != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Attempting to restart preview...");
                    IsPreviewRunning = await CameraHelper.StartPreviewAsync(_cameraView);
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] Preview restarted: {IsPreviewRunning}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] Restart preview error: {ex.Message}");
                    IsPreviewRunning = false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Skipping preview restart: restartPreview={restartPreview}, _isDestroyed={_isDestroyed}, _cameraView={_cameraView != null}");
            }
        }

        private void StartRecordingTimer()
        {
            StopRecordingTimer(); // Clean up any previous timer first
            _recordingTimer = new System.Timers.Timer(1000);
            _recordingTimer.Elapsed += OnRecordingTimerElapsed;
            _recordingTimer.AutoReset = true;
            _recordingTimer.Enabled = true;
        }

        private void StopRecordingTimer()
        {
            var timer = _recordingTimer;
            _recordingTimer = null;
            if (timer != null)
            {
                timer.Elapsed -= OnRecordingTimerElapsed;
                try { timer.Stop(); } catch { }
                try { timer.Dispose(); } catch { }
            }
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

        #region Image Viewer

        private void OpenImageViewer()
        {
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] OpenImageViewer called");

            var imagePaths = FileHelper.GetAllGalleryMediaPaths();
            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] GetAllGalleryMediaPaths returned {imagePaths.Count} paths");

            // Temp files are deleted after gallery copy — fall back to last thumbnail
            if (imagePaths.Count == 0 && !string.IsNullOrEmpty(_lastThumbPath) && File.Exists(_lastThumbPath))
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Using fallback thumb: {_lastThumbPath}");
                imagePaths = new List<string> { _lastThumbPath! };
            }

            if (imagePaths.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] No images found, gallery not opening");
                return;
            }

            // Just use file paths directly - MAUI Image control will load them
            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Setting gallery paths: {imagePaths.Count} images");

            GalleryImagePaths = new ObservableCollection<string>(imagePaths);
            CurrentImageIndex = 0; // newest first
            IsImageViewerVisible = true;

            System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Gallery opened with paths bound to Image.Source");
        }

        private async Task ShareCurrentImageAsync()
        {
            if (_galleryImagePaths.Count == 0 ||
                _currentImageIndex < 0 ||
                _currentImageIndex >= _galleryImagePaths.Count)
                return;

            var path = _galleryImagePaths[_currentImageIndex];
            if (!File.Exists(path)) return;

            // Determine media type from file extension
            var extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
            var isVideo = extension == ".mp4";
            var title = isVideo ? "Share Video" : "Share Photo";

            try
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = title,
                    File = new ShareFile(path)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Share error: {ex.Message}");
            }
        }

        private async Task DeleteCurrentImageAsync()
        {
            if (_galleryImagePaths.Count == 0 ||
                _currentImageIndex < 0 ||
                _currentImageIndex >= _galleryImagePaths.Count)
                return;

            var path = _galleryImagePaths[_currentImageIndex];

            // Determine media type from file extension
            var extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
            var isVideo = extension == ".mp4";
            var message = isVideo ? "Delete this video?" : "Delete this photo?";

            // Confirm deletion
            var page = Shell.Current ?? Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
            {
                bool confirm = await page.DisplayAlertAsync("Delete", message, "Delete", "Cancel");
                if (!confirm) return;
            }

            if (!FileHelper.DeleteMedia(path)) return;

            var updated = new List<string>(_galleryImagePaths);
            updated.RemoveAt(_currentImageIndex);

            if (updated.Count == 0)
            {
                IsImageViewerVisible = false;
                GalleryImagePaths = new ObservableCollection<string>();
                return;
            }

            // Adjust index if we deleted the last item
            var newIndex = _currentImageIndex >= updated.Count ? updated.Count - 1 : _currentImageIndex;
            GalleryImagePaths = new ObservableCollection<string>(updated);
            CurrentImageIndex = newIndex;
        }

        private async Task PlayVideoAsync(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] PlayVideoAsync: invalid path");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Playing video: {filePath}");

#if ANDROID
                // On Android 7+ (API 24+), Uri.FromFile() for app-private files throws
                // FileUriExposedException. Use FileProvider to get a shareable content:// URI.
                try
                {
                    var context = Android.App.Application.Context;
                    var javaFile = new Java.IO.File(filePath);
                    var authority = context.PackageName + ".fileprovider";
                    var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, authority, javaFile);
                    var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                    intent.SetDataAndType(uri, "video/mp4");
                    intent.AddFlags(Android.Content.ActivityFlags.NewTask | Android.Content.ActivityFlags.GrantReadUriPermission);
                    context.StartActivity(intent);
                    return;
                }
                catch (Exception andEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Android FileProvider intent failed: {andEx.Message}");
                }
#endif
                // Fallback: use MAUI Launcher (cross-platform)
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] PlayVideoAsync error: {ex.Message}");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Updates the thumbnail preview from a plain file path and persists it.
        /// Used after both photo capture and video recording.
        /// </summary>
        private void UpdateLastCapture(string thumbPath)
        {
            if (string.IsNullOrEmpty(thumbPath) || !File.Exists(thumbPath)) return;

            _lastThumbPath = thumbPath;
            try { Preferences.Set(PrefLastThumbPath, thumbPath); } catch { /* best effort */ }

            // Load into memory and display on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(thumbPath);
                    LastCaptureImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Error loading thumbnail: {ex.Message}");
                }
            });
        }

        private string GetCityForFileName()
        {
            var cityItem = OverlayItems.FirstOrDefault(s => s.Name == "City");
            var city = cityItem?.Value;
            return string.IsNullOrWhiteSpace(city) || city == "Unknown" ? "CekliCam" : city;
        }

        /// <summary>
        /// Wraps an async action in try-catch so that exceptions from Command lambdas
        /// don't become unobserved task exceptions that crash the app.
        /// </summary>
        private static async Task SafeExecuteAsync(Func<Task> action, [CallerMemberName] string? caller = null)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Command error in {caller}: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely acquires the camera lock with timeout. Returns false if the lock
        /// could not be acquired or was disposed (during app shutdown).
        /// </summary>
        private async Task<bool> TryAcquireCameraLockAsync(int timeoutMs = 3000)
        {
            try
            {
                if (_isDestroyed) return false;
                return await _cameraLock.WaitAsync(timeoutMs);
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        /// <summary>
        /// Safely releases the camera lock. No-op if already disposed.
        /// </summary>
        private void ReleaseCameraLock()
        {
            try { _cameraLock.Release(); }
            catch (ObjectDisposedException) { }
        }

        /// <summary>
        /// Logs detailed state information for crash diagnostics.
        /// </summary>
        private void LogState(string operation, string message)
        {
            try
            {
                // Use app-private cache directory (same as FileHelper.GetAppCacheDir)
                var logPath = Path.Combine(FileSystem.CacheDirectory, CrashLogPath);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] {operation}: {message}\n";

                // Append to existing log file
                if (File.Exists(logPath))
                {
                    File.AppendAllText(logPath, logEntry);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                    File.WriteAllText(logPath, logEntry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Failed to write crash log: {ex.Message}");
            }
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        #endregion
    }
}