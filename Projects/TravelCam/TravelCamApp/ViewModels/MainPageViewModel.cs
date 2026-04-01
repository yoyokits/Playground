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
// Sensor display is delegated to SensorValueViewModel, which
// subscribes to SensorHelper independently.  SensorItems is
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
using TravelCamApp.Helpers;
using TravelCamApp.Models;

namespace TravelCamApp.ViewModels
{
    public enum CaptureMode { Photo, Video }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly SensorHelper _sensorHelper;
        private readonly SensorValueViewModel _sensorValueViewModel;

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
        private bool _isTogglingRecording;
        private string _permissionStatus = "Initializing...";
        private bool _hasCameraPermission;

        // Overlay visibility
        private bool _isSensorOverlayVisible = true;
        private bool _isSettingsVisible;

        // Timer for recording display
        private System.Timers.Timer? _recordingTimer;
        private DateTime _recordingStart;

        // Flash
        private string _flashModeText = "Off";

        // Zoom
        private double _zoomFactor;

        // Lifecycle guards
        private bool _lifecycleSubscribed;
        private bool _isDestroyed;
        private bool _viewReadyCalled;

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
                    CameraHelper.SetZoom(_cameraView, value);
            }
        }

        /// <summary>
        /// Passthrough to SensorValueViewModel.SensorItems.
        /// Existing XAML bindings (SensorValueView, SensorValueSettingsView) use this.
        /// </summary>
        public ObservableCollection<SensorItem> SensorItems => _sensorValueViewModel.SensorItems;

        /// <summary>
        /// Exposes the dedicated sensor display sub-ViewModel for consumers that need it.
        /// </summary>
        public SensorValueViewModel SensorValueViewModel => _sensorValueViewModel;

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

        public MainPageViewModel(SensorHelper sensorHelper, SensorValueViewModel sensorValueViewModel)
        {
            _sensorHelper = sensorHelper;
            _sensorValueViewModel = sensorValueViewModel;

            // Commands
            ToggleCameraCommand = new Command(async () => await ToggleCameraAsync());
            CaptureCommand = new Command(async () => await CaptureAsync());
            SetPhotoModeCommand = new Command(() => SelectedMode = CaptureMode.Photo);
            SetVideoModeCommand = new Command(() => SelectedMode = CaptureMode.Video);
            ToggleFlashCommand = new Command(() => ToggleFlash());
            OpenSettingsCommand = new Command(() => IsSettingsVisible = true);
            CloseSettingsCommand = new Command(async () => await CloseSettingsAsync());
            OpenGalleryCommand = new Command(async () => await OpenGalleryAsync());

            // Start permissions + sensors. Wrapped in SafeFireAndForget so exceptions
            // during Activity reconstruction don't crash the process.
            _ = SafeInitializeAsync();

            // Subscribe to app lifecycle for camera stop/restart on background/foreground
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
            if (_isDestroyed) return;

            PermissionStatus = "Requesting permissions...";

            // 1. Camera + microphone permission
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

            // 2. Location permission
            try { await RequestLocationPermissionAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Location permission error: {ex.Message}");
            }

            if (_isDestroyed) return;

            // 3. Storage permission (Android ≤ 12)
#if ANDROID
            try { await RequestStoragePermissionsAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Storage permission error: {ex.Message}");
            }
#endif

            if (_isDestroyed) return;

            // 4. Apply persisted sensor settings via SensorValueViewModel
            await _sensorValueViewModel.ApplyPersistedSettingsAsync();

            if (_isDestroyed) return;

            // 5. Start sensor collection
            await _sensorHelper.StartAsync();
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
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Window Resumed");
                await _sensorHelper.StartAsync();
                if (_cameraView != null && HasCameraPermission && !_isDestroyed)
                {
                    IsPreviewRunning = false;
                    IsPreviewRunning = await CameraHelper.StartPreviewAsync(_cameraView);
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
                    $"[MainPageViewModel] Stop recording error: {ex.Message}");
            }

            _sensorHelper.Stop();

            if (_cameraView != null)
            {
                CameraHelper.StopPreview(_cameraView);
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

            bool ok = await CameraHelper.StartPreviewAsync(_cameraView);
            IsPreviewRunning = ok;
            PermissionStatus = ok ? "Camera ready" : "Failed to start camera";
        }

        private async Task ToggleCameraAsync()
        {
            if (_cameraView == null || !IsPreviewRunning) return;

            // Stop preview first, toggle camera, then restart
            CameraHelper.StopPreview(_cameraView);
            IsPreviewRunning = false;

            await CameraHelper.ToggleCameraDeviceAsync(_cameraView);

            bool ok = await CameraHelper.StartPreviewAsync(_cameraView);
            IsPreviewRunning = ok;
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
                IsShutterAnimated = true;
                await Task.Delay(200);
                IsShutterAnimated = false;

                // Trigger capture; result arrives via MediaCaptured event → OnMediaCaptured
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
        /// Saves the photo to the gallery and updates the thumbnail.
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
                var savedPath = await FileHelper.SavePhotoAsync(stream, city);

                if (!string.IsNullOrEmpty(savedPath))
                {
                    _lastCaptureImagePath = savedPath;
                    LastCaptureImage = savedPath.StartsWith("content://",
                        StringComparison.OrdinalIgnoreCase)
                        ? ImageSource.FromUri(new Uri(savedPath))
                        : ImageSource.FromFile(savedPath);
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainPageViewModel] Photo saved: {savedPath}");
                }
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

        /// <summary>
        /// Called by MainPage when the CameraView fires MediaCaptureFailed.
        /// </summary>
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
            bool ok = await CameraHelper.StartVideoRecordingAsync(_cameraView!);
            if (ok)
            {
                IsRecording = true;
                _recordingStart = DateTime.Now;
                StartRecordingTimer();
                System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Recording started");
            }
        }

        private async Task StopRecordingAsync()
        {
            StopRecordingTimer();

            var videoStream = await CameraHelper.StopVideoRecordingAsync(_cameraView!);
            IsRecording = false;

            if (videoStream != null)
            {
                var city = GetCityForFileName();
                var galleryPath = await FileHelper.SaveVideoAsync(videoStream, city);
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] Video published: {galleryPath}");
                // Camera preview automatically resumes after stop
                IsPreviewRunning = true;
            }
            else
            {
                // If no stream returned, try restarting preview manually
                if (_cameraView != null)
                    IsPreviewRunning = await CameraHelper.StartPreviewAsync(_cameraView);
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

        #region Flash & Zoom

        private void ToggleFlash()
        {
            if (_cameraView == null) return;
            var mode = CameraHelper.CycleFlashMode(_cameraView);
            FlashModeText = mode == CameraFlashMode.On ? "On" : "Off";
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
                await SettingsHelper.SaveSensorItemsConfigurationAsync(_sensorValueViewModel.SensorItems);
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
            if (string.IsNullOrEmpty(_lastCaptureImagePath)) return;

#if ANDROID
            try
            {
                var context = Android.App.Application.Context;

                if (_lastCaptureImagePath.StartsWith("content://", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = Android.Net.Uri.Parse(_lastCaptureImagePath);
                    var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                    intent.SetDataAndType(uri, "image/*");
                    intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
                    intent.AddFlags(Android.Content.ActivityFlags.NewTask);

                    try { context.StartActivity(intent); }
                    catch (Android.Content.ActivityNotFoundException)
                    {
                        var chooser = Android.Content.Intent.CreateChooser(intent, "Open with");
                        chooser!.AddFlags(Android.Content.ActivityFlags.NewTask);
                        context.StartActivity(chooser);
                    }
                    return;
                }

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
                System.Diagnostics.Debug.WriteLine(
                    $"[MainPageViewModel] OpenGallery error: {ex.Message}");
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
            var cityItem = SensorItems.FirstOrDefault(s => s.Name == "City");
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
