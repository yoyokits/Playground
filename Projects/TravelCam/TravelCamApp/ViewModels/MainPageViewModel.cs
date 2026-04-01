// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// MainPageViewModel: Single coordinator for all camera, sensor,
// and UI state. SensorHelper is the single source of sensor truth.
// The view model subscribes to SensorDataUpdated and reflects
// changes into SensorItems for display.
//
// Permissions are requested early (constructor) so the camera
// can initialize as soon as the view binds.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Camera.MAUI;
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

        // Camera
        private CameraView? _cameraView;
        private bool _isPreviewRunning;
        private bool _isRecording;
        private CaptureMode _selectedMode = CaptureMode.Photo;
        private string _recordingTimeText = "00:00";
        private ImageSource? _lastCaptureImage;
        private string? _lastCaptureImagePath;
        private bool _isShutterAnimated;
        private bool _isCapturing;
        private string _permissionStatus = "Initializing...";
        private bool _hasCameraPermission;

        // Sensor data display
        private ObservableCollection<SensorItem> _sensorItems = new();

        // Overlay visibility
        private bool _isSensorOverlayVisible = true;
        private bool _isSettingsVisible;

        // Timer for recording display
        private System.Timers.Timer? _recordingTimer;
        private DateTime _recordingStart;
        private string? _currentVideoPath;

        // Flash
        private string _flashModeText = "Off";

        // Zoom
        private double _zoomFactor;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        public CameraView? CameraView
        {
            get => _cameraView;
            set
            {
                _cameraView = value;
                OnPropertyChanged();
            }
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

        public bool IsShutterAnimated
        {
            get => _isShutterAnimated;
            set { _isShutterAnimated = value; OnPropertyChanged(); }
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

        public string FlashModeText
        {
            get => _flashModeText;
            set { _flashModeText = value; OnPropertyChanged(); }
        }

        public double ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                _zoomFactor = value;
                OnPropertyChanged();
                if (_cameraView != null)
                {
                    CameraHelper.SetZoom(_cameraView, value);
                }
            }
        }

        public ObservableCollection<SensorItem> SensorItems
        {
            get => _sensorItems;
            set { _sensorItems = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand ToggleCameraCommand { get; }
        public ICommand CaptureCommand { get; }
        public ICommand SetPhotoModeCommand { get; }
        public ICommand SetVideoModeCommand { get; }
        public ICommand ToggleFlashCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand CloseSettingsCommand { get; }
        public ICommand OpenGalleryCommand { get; }

        #endregion

        #region Constructor

        public MainPageViewModel(SensorHelper sensorHelper)
        {
            _sensorHelper = sensorHelper;

            // Initialize sensor items with defaults
            InitializeSensorItems();

            // Subscribe to sensor updates
            _sensorHelper.SensorDataUpdatedCallback += OnSensorDataUpdated;

            // Commands
            ToggleCameraCommand = new Command(async () => this.ToggleCameraAsync());
            CaptureCommand = new Command(async () => await CaptureAsync());
            SetPhotoModeCommand = new Command(() => SelectedMode = CaptureMode.Photo);
            SetVideoModeCommand = new Command(() => SelectedMode = CaptureMode.Video);
            ToggleFlashCommand = new Command(async () => await ToggleFlashAsync());
            OpenSettingsCommand = new Command(() => IsSettingsVisible = true);
            CloseSettingsCommand = new Command(async () => await CloseSettingsAsync());
            OpenGalleryCommand = new Command(async () => await OpenGalleryAsync());

            // Initialize permissions and sensor collection on startup.
            // Wrapped in SafeFireAndForget so exceptions during Activity
            // reconstruction (e.g. Permissions.RequestAsync before Activity
            // is fully ready) don't crash the process.
            _ = SafeInitializeAsync();

            // Subscribe to app lifecycle so camera is stopped/restarted on background/foreground
            if (Application.Current is Application app)
            {
                app.PageAppearing += OnPageAppearing;
                app.PageDisappearing += OnPageDisappearing;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Safe wrapper that catches any exception thrown by InitializeAsync
        /// so a fire-and-forget call from the constructor never tears down the process.
        /// </summary>
        private async Task SafeInitializeAsync()
        {
            try
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] InitializeAsync failed: {ex.Message}");
                PermissionStatus = "Initialization error";
            }
        }

        /// <summary>
        /// Requests permissions, loads saved settings, and starts sensor collection.
        /// Called from constructor -- fire and forget, camera starts when ready.
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_isDestroyed) return;

            PermissionStatus = "Requesting permissions...";

            // 1. Request camera permission
            bool cameraOk = false;
            try
            {
                cameraOk = await RequestCameraPermissionAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Camera permission error: {ex.Message}");
            }

            if (_isDestroyed) return;

            if (!cameraOk)
            {
                PermissionStatus = "Camera permission denied";
                HasCameraPermission = false;
            }
            else
            {
                HasCameraPermission = true;
                PermissionStatus = "Camera ready";
            }

            // 2. Request location permission
            try
            {
                bool locationOk = await RequestLocationPermissionAsync();
                if (!locationOk)
                {
                    System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Location permission not granted");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Location permission error: {ex.Message}");
            }

            if (_isDestroyed) return;

            // 3. Request storage permission (Android 12 and below)
#if ANDROID
            try
            {
                await RequestStoragePermissionsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Storage permission error: {ex.Message}");
            }
#endif

            if (_isDestroyed) return;

            // 4. Load saved sensor settings
            await LoadSensorSettingsAsync();

            if (_isDestroyed) return;

            // 5. Start sensor collection
            await _sensorHelper.StartAsync();
        }

        private async Task<bool> RequestCameraPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }
            var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (micStatus != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
            }
            return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
        }

        private async Task<bool> RequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
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
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Storage permission error: {ex.Message}");
            }
        }
#endif

        /// <summary>
        /// Loads saved sensor item visibility settings and applies them.
        /// </summary>
        private async Task LoadSensorSettingsAsync()
        {
            try
            {
                var config = await SettingsHelper.LoadSensorItemsConfigurationAsync();
                if (config != null)
                {
                    SettingsHelper.ApplyConfigurationToSensorItems(new System.Collections.Generic.List<SensorItem>(_sensorItems), config);
                }
                else
                {
                    // Set defaults: City, Country, Temperature visible
                    UpdateSensorItem("City", isVisible: true);
                    UpdateSensorItem("Country", isVisible: true);
                    UpdateSensorItem("Temperature", isVisible: true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Load settings error: {ex.Message}");
            }
        }

        #endregion

        #region Lifecycle

        private bool _lifecycleSubscribed;
        private bool _isDestroyed;

        /// <summary>
        /// Subscribe to Window Resumed/Stopped events once the CameraView is available.
        /// These fire reliably when the app enters foreground/background on Android.
        /// </summary>
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

            try
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Resumed");
                // Restart sensor collection
                await _sensorHelper.StartAsync();
                // Restart camera preview
                if (_cameraView != null && HasCameraPermission && !_isDestroyed)
                {
                    IsPreviewRunning = false; // reset so StartCameraPreviewAsync will run
                    await StartCameraPreviewAsync();
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
                // Stop recording if active
                if (_isRecording && _cameraView != null)
                {
                    await StopRecordingAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Stop recording error: {ex.Message}");
            }

            // Stop sensor updates (synchronous, safe)
            _sensorHelper.Stop();

            // Stop camera preview
            try
            {
                if (_cameraView != null)
                {
                    await CameraHelper.StopPreviewAsync(_cameraView);
                    IsPreviewRunning = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Stop preview error: {ex.Message}");
                IsPreviewRunning = false;
            }
        }

        private void OnWindowDestroying(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Destroying");
            _isDestroyed = true;

            // Unsubscribe window events
            if (sender is Window window)
            {
                window.Resumed -= OnWindowResumed;
                window.Stopped -= OnWindowStopped;
                window.Destroying -= OnWindowDestroying;
            }

            // Unsubscribe app-level events
            if (Application.Current is Application app)
            {
                app.PageAppearing -= OnPageAppearing;
                app.PageDisappearing -= OnPageDisappearing;
            }

            // Unsubscribe sensor callback
            _sensorHelper.SensorDataUpdatedCallback -= OnSensorDataUpdated;

            // Ensure everything is stopped
            _sensorHelper.Stop();
            _recordingTimer?.Dispose();
            _recordingTimer = null;
            _cameraView = null;
        }

        private void OnPageAppearing(object? sender, Page page)
        {
            // Hook window lifecycle the first time our page appears
            EnsureWindowLifecycle();
        }

        private void OnPageDisappearing(object? sender, Page page)
        {
            // No-op: Window.Stopped handles background transitions
        }

        #endregion

        #region Camera Operations

        /// <summary>
        /// Called by the view once it has bound the CameraView.
        /// This is where actual camera initialization happens.
        /// </summary>
        public async Task OnViewReady(CameraView cameraView)
        {
            if (_isDestroyed) return;

            _cameraView = cameraView;

            if (!HasCameraPermission)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] No camera permission, skipping preview");
                return;
            }

            await StartCameraPreviewAsync();
        }

        private async Task StartCameraPreviewAsync()
        {
            if (_isDestroyed || _cameraView == null)
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] CameraView not set or destroyed");
                return;
            }

            if (IsPreviewRunning)
            {
                return;
            }

            // Select camera device and start preview
            var selectedName = CameraHelper.SelectFirstAvailableCamera(_cameraView);

            if (selectedName == null || _cameraView.Camera == null)
            {
                PermissionStatus = "No camera device found";
                IsPreviewRunning = false;
                return;
            }

            bool previewOk = await CameraHelper.StartPreviewAsync(_cameraView);
            IsPreviewRunning = previewOk;
            PermissionStatus = previewOk ? "Camera ready" : "Failed to start camera";
        }

        private void ToggleCameraAsync()
        {
            // Camera.MAUI auto-restarts preview internally when the Camera
            // property is changed. No need to call StartPreviewAsync again.
            CameraHelper.ToggleCameraDevice(_cameraView!);
        }

        #endregion

        #region Capture

        private async Task CaptureAsync()
        {
            if (_cameraView == null || !IsPreviewRunning) return;

            if (SelectedMode == CaptureMode.Photo)
            {
                await CapturePhotoAsync();
            }
            else
            {
                await ToggleRecordingAsync();
            }
        }

        private async Task CapturePhotoAsync()
        {
            if (_isCapturing) return;
            _isCapturing = true;

            try
            {
                // Trigger shutter animation
                IsShutterAnimated = true;
                await Task.Delay(300);
                IsShutterAnimated = false;

                var stream = await CameraHelper.TakePhotoAsync(_cameraView!);
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine("[MainPageViewModel] CapturePhotoAsync returned null stream");
                    return;
                }

                var city = GetCityForFileName();
                var savedPath = await FileHelper.SavePhotoAsync(stream, city);

                if (!string.IsNullOrEmpty(savedPath))
                {
                    _lastCaptureImagePath = savedPath;
                    LastCaptureImage = savedPath.StartsWith("content://", StringComparison.OrdinalIgnoreCase)
                        ? ImageSource.FromUri(new Uri(savedPath))
                        : ImageSource.FromFile(savedPath);
                    System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Photo saved: {savedPath}");
                }
            }
            finally
            {
                _isCapturing = false;
            }
        }

        private async Task ToggleRecordingAsync()
        {
            if (_cameraView == null || !IsPreviewRunning) return;

            if (IsRecording)
            {
                await StopRecordingAsync();
            }
            else
            {
                await StartRecordingAsync();
            }
        }

        private async Task StartRecordingAsync()
        {
            _currentVideoPath = FileHelper.CreateVideoPath(GetCityForFileName());

            bool ok = await CameraHelper.StartVideoRecordingAsync(_cameraView!, _currentVideoPath);
            if (ok)
            {
                IsRecording = true;
                _recordingStart = DateTime.Now;
                StartRecordingTimer();
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Recording started: {_currentVideoPath}");
            }
        }

        private async Task StopRecordingAsync()
        {
            StopRecordingTimer();
            bool ok = await CameraHelper.StopVideoRecordingAsync(_cameraView!);
            IsRecording = false;

            // Publish the recorded video to the gallery
            if (ok && !string.IsNullOrEmpty(_currentVideoPath))
            {
                var galleryPath = FileHelper.PublishVideoToGallery(_currentVideoPath);
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Video published to gallery: {galleryPath}");
                _currentVideoPath = null;

                IsPreviewRunning = true;
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Recording stopped, preview should be running");
            }
            else
            {
                _currentVideoPath = null;
                if (_cameraView != null)
                {
                    IsPreviewRunning = await CameraHelper.StartPreviewAsync(_cameraView);
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
            RecordingTimeText = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        }

        #endregion

        #region Flash & Zoom

        private async Task ToggleFlashAsync()
        {
            if (_cameraView == null) return;
            var mode = CameraHelper.CycleFlashMode(_cameraView);
            FlashModeText = mode switch
            {
                FlashMode.Enabled => "On",
                FlashMode.Auto => "Auto",
                _ => "Off",
            };
        }

        #endregion

        #region Sensor Data Updates

        /// <summary>
        /// Called when SensorHelper publishes new data.
        /// Updates all sensor item values from the SensorData.
        /// </summary>
        private void OnSensorDataUpdated(Models.SensorData data)
        {
            if (_isDestroyed) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_isDestroyed) return;

                UpdateSensorItem("Temperature",
                    data.Temperature.HasValue ? $"{data.Temperature.Value:F1}\u00b0C" : "N/A");
                UpdateSensorItem("City", data.City ?? "Unknown");
                UpdateSensorItem("Country", data.Country ?? "Unknown");
                UpdateSensorItem("Altitude",
                    data.Altitude.HasValue ? $"{data.Altitude.Value:F0}m" : "N/A");
                UpdateSensorItem("Latitude", data.Latitude.ToString(CultureInfo.InvariantCulture));
                UpdateSensorItem("Longitude", data.Longitude.ToString(CultureInfo.InvariantCulture));
                UpdateSensorItem("Date", data.Timestamp.ToString("MM/dd/yyyy"));
                UpdateSensorItem("Time", data.Timestamp.ToString("HH:mm:ss"));
                UpdateSensorItem("Heading",
                    data.Heading.HasValue ? $"{data.Heading.Value:F0}\u00b0" : "N/A");
                UpdateSensorItem("Speed",
                    data.Speed.HasValue ? $"{data.Speed.Value:F1} km/h" : "N/A");
            });
        }

        private void UpdateSensorItem(string name, string? value = null, bool? isVisible = null)
        {
            var item = System.Linq.Enumerable.FirstOrDefault(_sensorItems, si => si.Name == name);
            if (item != null)
            {
                if (value != null) item.Value = value;
                if (isVisible.HasValue) item.IsVisible = isVisible.Value;
            }
        }

        private void InitializeSensorItems()
        {
            _sensorItems = new ObservableCollection<SensorItem>
            {
                new SensorItem("City", "", isVisible: true),
                new SensorItem("Country", "", isVisible: true),
                new SensorItem("Temperature", "", isVisible: true),
                new SensorItem("Altitude", "", isVisible: false),
                new SensorItem("Latitude", "", isVisible: false),
                new SensorItem("Longitude", "", isVisible: false),
                new SensorItem("Date", DateTime.Now.ToString("MM/dd/yyyy"), isVisible: false),
                new SensorItem("Time", DateTime.Now.ToString("HH:mm:ss"), isVisible: false),
                new SensorItem("Heading", "", isVisible: false),
                new SensorItem("Speed", "", isVisible: false),
            };
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
                await SettingsHelper.SaveSensorItemsConfigurationAsync(_sensorItems);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Save settings error: {ex.Message}");
            }
        }

        #endregion

        #region Gallery

        private async Task OpenGalleryAsync()
        {
            if (string.IsNullOrEmpty(_lastCaptureImagePath)) return;

#if ANDROID
            try
            {
                var context = Android.App.Application.Context;

                // If the path is a content:// URI from MediaStore, open the gallery
                // at the CekliCam album so the user can swipe between all photos.
                if (_lastCaptureImagePath.StartsWith("content://", StringComparison.OrdinalIgnoreCase))
                {
                    // Try to open the default gallery app to the Pictures/CekliCam album
                    // by querying for the bucket, allowing swiping between photos.
                    var uri = Android.Net.Uri.Parse(_lastCaptureImagePath);
                    var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                    intent.SetDataAndType(uri, "image/*");
                    intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
                    intent.AddFlags(Android.Content.ActivityFlags.NewTask);

                    // Try to resolve to the default gallery (not a chooser) so swiping works
                    try
                    {
                        context.StartActivity(intent);
                    }
                    catch (Android.Content.ActivityNotFoundException)
                    {
                        // Fallback: open any app that can view images
                        var chooser = Android.Content.Intent.CreateChooser(intent, "Open with");
                        chooser!.AddFlags(Android.Content.ActivityFlags.NewTask);
                        context.StartActivity(chooser);
                    }

                    return;
                }

                // File path: use FileProvider
                var file = new Java.IO.File(_lastCaptureImagePath);
                if (file.Exists())
                {
                    var providerUri = Microsoft.Maui.Storage.FileProvider.GetUriForFile(
                        Microsoft.Maui.ApplicationModel.Platform.AppContext,
                        context.PackageName + ".fileprovider",
                        file);

                    var intent2 = new Android.Content.Intent(Android.Content.Intent.ActionView);
                    intent2.SetDataAndType(providerUri, "image/*");
                    intent2.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
                    intent2.AddFlags(Android.Content.ActivityFlags.NewTask);
                    context.StartActivity(intent2);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] OpenGallery error: {ex.Message}");
            }
#else
            await Launcher.Default.OpenAsync(new OpenFileRequest("Open Image",
                new ReadOnlyFile(_lastCaptureImagePath)));
#endif
        }

        #endregion

        #region Helpers

        private string GetCityForFileName()
        {
            var cityItem = System.Linq.Enumerable.FirstOrDefault(_sensorItems, s => s.Name == "City");
            var city = cityItem?.Value;
            return string.IsNullOrWhiteSpace(city) || city == "Unknown" ? "CekliCam" : city;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
