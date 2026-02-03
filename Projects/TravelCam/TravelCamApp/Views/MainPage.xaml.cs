// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using TravelCamApp.Helpers;
using TravelCamApp.Models;
using TravelCamApp.ViewModels;
using TravelCamApp.Views;
using SkiaSharp;
using Camera.MAUI;

namespace TravelCamApp.Views
{
    /// <summary>
    /// Main page of the TravelCam application that handles camera operations
    /// including photo capture, video recording, and camera lifecycle management.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        #region Fields

        private readonly IDispatcherTimer _recordingTimer;
        private DateTime _recordingStart;
        private readonly Border? _shutterFlash;
        private readonly SemaphoreSlim _cameraLifecycleLock = new(1, 1);
        private volatile bool _isPreviewRunning;
        private volatile bool _isCameraConfigured;
        private CancellationTokenSource? _previewCts;
        private volatile bool _isDisposing = false;
        private volatile int _lifecycleGeneration;
        private const int CAMERA_STABILIZATION_DELAY = 500; // Delay to allow camera callbacks to complete

        // Guards against starting camera work after page disappears (race with CamerasLoaded/async continuations).
        private volatile bool _isPageVisible;

        // Flag to prevent multiple simultaneous recording operations
        private volatile bool _isProcessingRecordingToggle = false;

        // Sensor helper for collecting and managing sensor data
        private SensorHelper? _sensorHelper;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            var viewModel = new MainPageViewModel();
            BindingContext = viewModel;

            _shutterFlash = FindByName("ShutterFlash") as Border;

            // Initialize sensor helper
            _sensorHelper = new SensorHelper();
            _sensorHelper.SensorDataUpdated += OnSensorDataUpdated;

            _recordingTimer = Dispatcher.CreateTimer();
            _recordingTimer.Interval = TimeSpan.FromSeconds(1);
            _recordingTimer.Tick += OnRecordingTimerTick;

#if DEBUG
            ExitButton.IsVisible = true;
            ExitButton.Clicked += OnExitButtonClicked;
#endif

            CameraView.CamerasLoaded += OnCameraViewCamerasLoaded;

            // Load the last taken image as the default preview
            LoadLastTakenImage(viewModel);

            // Request permissions for location and sensors
            Task.Run(async () => await viewModel.RequestPermissionsAsync());

            // Wire up the sensor value settings request event
            SensorValueOverlay.SensorValueSettingsRequested += OnSensorValueSettingsRequested;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Handles the event when cameras are loaded in the camera view.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnCameraViewCamerasLoaded(object? sender, EventArgs e)
        {
            if (_isDisposing) return;

            var gen = _lifecycleGeneration;
            var shouldStartPreview = false;

            await _cameraLifecycleLock.WaitAsync();
            try
            {
                if (!_isPageVisible || gen != _lifecycleGeneration || _isDisposing)
                {
                    return;
                }

                if (!_isCameraConfigured)
                {
                    CameraHelper.ConfigureDevices(CameraView);
                    _isCameraConfigured = true;
                }

                if (CameraView.Camera is null && CameraView.Cameras != null && CameraView.Cameras.Any())
                {
                    CameraView.Camera = CameraView.Cameras.FirstOrDefault();
                }

                if (CameraView.Camera is null || _isPreviewRunning)
                {
                    return;
                }
                shouldStartPreview = true;
            }
            finally
            {
                _cameraLifecycleLock.Release();
            }

            if (shouldStartPreview)
            {
                await StartPreviewSafeAsync(gen);
            }
        }

#if DEBUG

        /// <summary>
        /// Handles the exit button click event in debug mode.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnExitButtonClicked(object? sender, EventArgs e)
        {
            Application.Current?.Quit();
        }

#endif

        /// <inheritdoc/>
        protected override async void OnAppearing()
        {
            System.Diagnostics.Debug.WriteLine("[MainPage] OnAppearing called");
            
            if (_isDisposing) return;

            base.OnAppearing();

            _isPageVisible = true;
            _isDisposing = false;
            System.Threading.Interlocked.Increment(ref _lifecycleGeneration);
            var gen = _lifecycleGeneration;

            if (!await CameraHelper.RequestPermissionsAsync())
            {
                LogDebug("Camera permissions not granted");
                return;
            }

            // Always reconfigure camera when appearing (e.g., after returning from gallery)
            if (CameraView.NumCamerasDetected > 0)
            {
                CameraHelper.ConfigureDevices(CameraView);
                _isCameraConfigured = true;
            }

            // Start the sensor helper to collect data
            if (_sensorHelper != null)
            {
                await _sensorHelper.StartAsync();

                // Update the UI with initial sensor data
                UpdateOverlayLabels();
            }

            // Reload sensor settings to ensure UI is up-to-date
            await ReloadSensorSettingsAsync();

            // Always restart preview when appearing
            _isPreviewRunning = false;
            await StartPreviewSafeAsync(gen);
            
            System.Diagnostics.Debug.WriteLine("[MainPage] OnAppearing completed");
        }

        /// <summary>
        /// Reloads sensor settings from storage and updates the SensorValueViewModel.
        /// </summary>
        private async Task ReloadSensorSettingsAsync()
        {
            try
            {
                if (BindingContext is MainPageViewModel viewModel)
                {
                    var config = await Helpers.SettingsHelper.LoadSensorItemsConfigurationAsync();
                    var additionalSettings = await Helpers.SettingsHelper.LoadAdditionalSettingsAsync();

                    if (config != null)
                    {
                        viewModel.SensorValueViewModel.ApplyConfiguration(config);
                    }

                    if (additionalSettings != null)
                    {
                        viewModel.SensorValueViewModel.FontSize = additionalSettings.FontSize;
                        viewModel.SensorValueViewModel.IsMapOverlayVisible = additionalSettings.IsMapOverlayVisible;
                    }
                    
                    System.Diagnostics.Debug.WriteLine("[MainPage] Sensor settings reloaded");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPage] Error reloading sensor settings: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        protected override async void OnDisappearing()
        {
            _isPageVisible = false;
            _isDisposing = true;
            System.Threading.Interlocked.Increment(ref _lifecycleGeneration);

            // Cancel any pending preview operations first
            _previewCts?.Cancel();

            // Update sensor data one final time before disappearing
            if (_sensorHelper != null)
            {
                _sensorHelper.SensorDataUpdated -= OnSensorDataUpdated;
                await _sensorHelper.UpdateSensorDataAsync();
                _sensorHelper.Stop();
            }

            await StopPreviewSafeAsync();
            base.OnDisappearing();
        }

        /// <summary>
        /// Safely starts the camera preview with proper lifecycle management.
        /// </summary>
        /// <param name="expectedGeneration">The expected lifecycle generation.</param>
        private async Task StartPreviewSafeAsync(int expectedGeneration)
        {
            if (_isDisposing) return;

            await _cameraLifecycleLock.WaitAsync();
            try
            {
                if (!_isPageVisible || expectedGeneration != _lifecycleGeneration || _isDisposing)
                {
                    return;
                }

                if (_isPreviewRunning || CameraView.Camera is null)
                {
                    return;
                }

                _previewCts?.Cancel();
                _previewCts = new CancellationTokenSource();

                bool success = await CameraHelper.StartPreviewAsync(CameraView);
                if (!success)
                {
                    LogDebug("Failed to start camera preview");
                    return;
                }

                // Wait for camera session to be fully established before marking as running
                await Task.Delay(CAMERA_STABILIZATION_DELAY, _previewCts.Token);

                if (!_isPageVisible || expectedGeneration != _lifecycleGeneration || _isDisposing)
                {
                    // If we became invisible while starting, stop immediately to avoid dangling sessions.
                    await CameraHelper.StopPreviewAsync(CameraView);
                    _isPreviewRunning = false;
                    return;
                }

                _isPreviewRunning = true;
                LogDebug("Camera preview started successfully");
            }
            catch (OperationCanceledException)
            {
                // Preview was cancelled during initialization, which is expected during rapid lifecycle changes
                LogDebug("Camera preview start was cancelled");
            }
            catch (Exception ex)
            {
                LogError("Error starting camera preview: {0}", ex.Message);
            }
            finally
            {
                _cameraLifecycleLock.Release();
            }
        }

        /// <summary>
        /// Safely stops the camera preview with proper lifecycle management.
        /// </summary>
        private async Task StopPreviewSafeAsync()
        {
            await _cameraLifecycleLock.WaitAsync();
            try
            {
                if (!_isPreviewRunning)
                {
                    return;
                }

                // Cancel any pending operations
                _previewCts?.Cancel();
                _previewCts = null;

                bool success = await CameraHelper.StopPreviewAsync(CameraView);
                if (success)
                {
                    _isPreviewRunning = false;
                    LogDebug("Camera preview stopped successfully");
                }
                else
                {
                    LogDebug("Failed to stop camera preview");
                    _isPreviewRunning = false; // Reset state anyway to prevent stuck state
                }
            }
            catch (Exception ex)
            {
                LogError("Error stopping camera preview: {0}", ex.Message);
                _isPreviewRunning = false;
            }
            finally
            {
                _cameraLifecycleLock.Release();
            }
        }

        /// <summary>
        /// Handles the recording timer tick event to update the recording time display.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnRecordingTimerTick(object? sender, EventArgs e)
        {
            if (BindingContext is not MainPageViewModel viewModel || !viewModel.IsRecording)
            {
                _recordingTimer.Stop();
                return;
            }

            var elapsed = DateTime.UtcNow - _recordingStart;
            viewModel.RecordingTimeText = elapsed.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Handles the shutter tap event for both photo and video capture.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnShutterTapped(object? sender, EventArgs e)
        {
            if (_isDisposing) return;

            if (BindingContext is not MainPageViewModel viewModel)
            {
                return;
            }

            LogDebug("[MainPage] Shutter tapped - Mode: {0}, IsRecording: {1}, IsPreviewRunning: {2}, IsPageVisible: {3}, Disposing: {4}",
                viewModel.SelectedMode, viewModel.IsRecording, _isPreviewRunning, _isPageVisible, _isDisposing);

            if (viewModel.SelectedMode == CaptureMode.Video)
            {
                // Prevent multiple simultaneous recording operations
                if (_isProcessingRecordingToggle)
                {
                    LogDebug("[MainPage] Recording toggle already in progress, ignoring additional tap");
                    return;
                }

                LogDebug("[MainPage] Shutter tapped - Video mode selected, IsRecording: {0}", viewModel.IsRecording);
                await ToggleVideoRecordingAsync(viewModel);
                return;
            }

            LogDebug("[MainPage] Shutter tapped - Photo mode selected");
            await PlayShutterAnimationAsync();

            try
            {
                await _cameraLifecycleLock.WaitAsync();
                try
                {
                    LogDebug("[MainPage] Taking photo...");
                    // Don't edit the original image - pass null for sensorData to capture without overlay
                    var stream = await CameraHelper.TakePhotoAsync(CameraView, null, false);
                    if (stream is null)
                    {
                        LogDebug("[MainPage] Failed to take photo - stream is null");
                        return;
                    }

                    LogDebug("[MainPage] Photo stream received. Length: {0}, Position: {1}", 
                        stream.CanSeek ? stream.Length.ToString() : "unknown",
                        stream.CanSeek ? stream.Position.ToString() : "unknown");

                    var filePath = await FileHelper.SavePhotoAsync(stream, "CekliCam");
                    LogDebug("[MainPage] SavePhotoAsync returned. FilePath: {0}", filePath ?? "NULL");
                    
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        // Verify the file exists and has content
                        if (File.Exists(filePath))
                        {
                            var fileInfo = new FileInfo(filePath);
                            LogDebug("[MainPage] Photo file verified. Size: {0} bytes", fileInfo.Length);
                            
                            if (fileInfo.Length > 0)
                            {
                                // Load as stream to ensure the image is properly displayed
                                var imageBytes = await File.ReadAllBytesAsync(filePath);
                                viewModel.LastCaptureImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                                LogDebug("[MainPage] Photo saved and preview updated: {0}", filePath);
                            }
                            else
                            {
                                LogError("[MainPage] Photo file is empty!");
                            }
                        }
                        else
                        {
                            LogError("[MainPage] Photo file does not exist: {0}", filePath);
                        }
                    }
                    else
                    {
                        LogError("[MainPage] SavePhotoAsync returned null or empty path");
                    }
                    
                    // Dispose the stream after saving
                    await stream.DisposeAsync();
                }
                catch (Exception ex) when (IsJavaIllegalStateException(ex))
                {
                    // Camera was closed concurrently – ignore or log and let user retry
                    LogError("[MainPage] Camera race condition while taking photo: {0}", ex.Message);
                }
                finally
                {
                    _cameraLifecycleLock.Release();
                }
            }
            catch (Exception ex)
            {
                LogError("[MainPage] Error taking photo: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Plays the shutter animation when taking a photo.
        /// </summary>
        private async Task PlayShutterAnimationAsync()
        {
            if (_shutterFlash is null || _isDisposing)
            {
                return;
            }

            try
            {
                _shutterFlash.BackgroundColor = Colors.White;
                _shutterFlash.Opacity = 0.8;

                // Use the dispatcher instead of Device.BeginInvokeOnMainThread
                if (Dispatcher.IsDispatchRequired)
                {
                    await Dispatcher.DispatchAsync(async () =>
                    {
                        await _shutterFlash.FadeToAsync(0, 200u, Easing.CubicOut);
                    });
                }
                else
                {
                    await _shutterFlash.FadeToAsync(0, 200u, Easing.CubicOut);
                }
            }
            catch (Exception ex)
            {
                LogError("Error in shutter animation: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Toggles video recording on/off based on the current state.
        /// </summary>
        /// <param name="viewModel">The main page view model.</param>
        private async Task ToggleVideoRecordingAsync(MainPageViewModel viewModel)
        {
            LogDebug("[MainPage] ToggleVideoRecordingAsync called - IsRecording: {0}, IsPreviewRunning: {1}, IsPageVisible: {2}, Disposing: {3}",
                viewModel.IsRecording, _isPreviewRunning, _isPageVisible, _isDisposing);

            if (_isDisposing) return;

            // Set the flag to prevent multiple simultaneous operations
            if (_isProcessingRecordingToggle)
            {
                LogDebug("[MainPage] Recording toggle already in progress, ignoring additional request");
                return;
            }

            _isProcessingRecordingToggle = true;

            try
            {
                // Capture the current state
                bool isCurrentlyRecording = viewModel.IsRecording;

                await _cameraLifecycleLock.WaitAsync();
                try
                {
                    if (isCurrentlyRecording) // User wants to stop (since viewModel.IsRecording is true when recording is active)
                    {
                        LogDebug("[MainPage] Stopping video recording...");

                        // Use the new centralized method from CameraHelper
                        var (success, newPreviewState) = await CameraHelper.StopVideoRecordingAsync(CameraView, _isPreviewRunning, async () =>
                        {
                            await UpdateVideoPreviewAsync(viewModel);
                        });

                        if (success)
                        {
                            _isPreviewRunning = newPreviewState;
                            _recordingTimer.Stop();
                            viewModel.IsRecording = false;
                            viewModel.RecordingTimeText = "00:00";
                            LogDebug("[MainPage] Recording stopped successfully");
                        }
                        else
                        {
                            LogError("[MainPage] Failed to stop video recording properly");
                        }
                    }
                    else // User wants to start recording (since viewModel.IsRecording is false when not recording)
                    {
                        LogDebug("[MainPage] Starting video recording...");

                        var filePath = FileHelper.CreateVideoPath("CekliCam");
                        LogDebug("[MainPage] Creating video file path: {0}", filePath);

                        // Use the new centralized method from CameraHelper
                        var (success, newPreviewState) = await CameraHelper.StartVideoRecordingAsync(CameraView, filePath, _isPreviewRunning);

                        if (success)
                        {
                            _isPreviewRunning = newPreviewState;
                            _recordingStart = DateTime.UtcNow;
                            viewModel.IsRecording = true;
                            _recordingTimer.Start();
                            LogDebug("[MainPage] Recording started, timer activated");
                        }
                        else
                        {
                            LogError("[MainPage] Failed to start video recording");
                            return;
                        }
                    }
                }
                finally
                {
                    _cameraLifecycleLock.Release();
                }
            }
            finally
            {
                // Reset the flag after the operation completes
                _isProcessingRecordingToggle = false;
            }
        }

        /// <summary>
        /// Updates the video preview thumbnail after recording stops.
        /// </summary>
        /// <param name="viewModel">The main page view model.</param>
        private async Task UpdateVideoPreviewAsync(MainPageViewModel viewModel)
        {
            LogDebug("[MainPage] UpdateVideoPreviewAsync called - IsPageVisible: {0}, IsDisposing: {1}, IsPreviewRunning: {2}",
                _isPageVisible, _isDisposing, _isPreviewRunning);

            if (_isDisposing) return;

            await _cameraLifecycleLock.WaitAsync();
            try
            {
                if (!_isPageVisible || _isDisposing)
                {
                    LogDebug("[MainPage] UpdateVideoPreviewAsync cancelled - page not visible or disposing");
                    return;
                }

                // Use the centralized method from CameraHelper
                bool success = await CameraHelper.UpdateVideoPreviewAsync(
                    CameraView,
                    _isPreviewRunning,
                    _isPageVisible,
                    _isDisposing,
                    (imageSource) =>
                    {
                        // Use the dispatcher instead of Device.BeginInvokeOnMainThread
                        if (Dispatcher.IsDispatchRequired)
                        {
                            Dispatcher.Dispatch(() =>
                            {
                                viewModel.LastCaptureImage = imageSource;
                                LogDebug("[MainPage] Thumbnail updated successfully");
                            });
                        }
                        else
                        {
                            viewModel.LastCaptureImage = imageSource;
                            LogDebug("[MainPage] Thumbnail updated successfully");
                        }
                    });

                if (!success)
                {
                    LogDebug("[MainPage] Failed to update video preview");
                }
            }
            catch (Exception ex) when (IsJavaIllegalStateException(ex))
            {
                // Handle the case where the camera was closed during the operation
                LogError("[MainPage] Camera already closed during preview update: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                LogError("[MainPage] Error updating video preview: {0}", ex.Message);
            }
            finally
            {
                _cameraLifecycleLock.Release();
                LogDebug("[MainPage] UpdateVideoPreviewAsync completed and lock released");
            }
        }

        /// <summary>
        /// Loads the last taken image as the default preview.
        /// </summary>
        /// <param name="viewModel">The main page view model.</param>
        private async void LoadLastTakenImage(MainPageViewModel viewModel)
        {
            try
            {
                // Look for the most recently created image file in the output directory
                var outputPath = Settings.OutputPath;
                System.Diagnostics.Debug.WriteLine($"[MainPage] LoadLastTakenImage - OutputPath: {outputPath}");
                
                if (!Directory.Exists(outputPath))
                {
                    LogDebug("[MainPage] Output directory does not exist: {0}", outputPath);
                    viewModel.LastCaptureImage = CreateWhitePlaceholderImage();
                    return;
                }

                // Get all image files directly from the output directory
                var imageFiles = Directory.GetFiles(outputPath, "*.jpg")
                                         .Concat(Directory.GetFiles(outputPath, "*.jpeg"))
                                         .Concat(Directory.GetFiles(outputPath, "*.png"))
                                         .ToArray();

                System.Diagnostics.Debug.WriteLine($"[MainPage] LoadLastTakenImage - Found {imageFiles.Length} image files");

                if (imageFiles.Length == 0)
                {
                    LogDebug("[MainPage] No image files found in output directory");
                    viewModel.LastCaptureImage = CreateWhitePlaceholderImage();
                    return;
                }

                // Get the most recent image by creation time
                var lastImagePath = imageFiles
                    .OrderByDescending(f => new FileInfo(f).CreationTime)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(lastImagePath) && File.Exists(lastImagePath))
                {
                    var fileInfo = new FileInfo(lastImagePath);
                    System.Diagnostics.Debug.WriteLine($"[MainPage] LoadLastTakenImage - Loading: {lastImagePath}, Size: {fileInfo.Length} bytes");
                    
                    if (fileInfo.Length > 0)
                    {
                        // Load image as stream to ensure it's properly loaded
                        var imageBytes = await File.ReadAllBytesAsync(lastImagePath);
                        viewModel.LastCaptureImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        LogDebug("[MainPage] Loaded last taken image: {0}", lastImagePath);
                    }
                    else
                    {
                        LogDebug("[MainPage] Last image file is empty");
                        viewModel.LastCaptureImage = CreateWhitePlaceholderImage();
                    }
                }
                else
                {
                    LogDebug("[MainPage] No previous images found to load as default preview");
                    viewModel.LastCaptureImage = CreateWhitePlaceholderImage();
                }
            }
            catch (Exception ex)
            {
                LogError("[MainPage] Error loading last taken image: {0}", ex.Message);
                viewModel.LastCaptureImage = CreateWhitePlaceholderImage();
            }
        }

        /// <summary>
        /// Creates a white placeholder image dynamically using SkiaSharp.
        /// </summary>
        /// <returns>An ImageSource representing a white placeholder image.</returns>
        private ImageSource CreateWhitePlaceholderImage()
        {
            return ImageSource.FromStream(() =>
            {
                var width = 100;
                var height = 100;

                using var surface = SKSurface.Create(new SKImageInfo(width, height));
                var canvas = surface.Canvas;

                // Fill the canvas with white color
                canvas.Clear(SKColors.White);

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                var stream = new MemoryStream();
                data.SaveTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            });
        }

        /// <summary>
        /// Checks if the exception is a Java IllegalStateException at runtime.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns>True if the exception is a Java IllegalStateException, false otherwise.</returns>
        private static bool IsJavaIllegalStateException(Exception? ex)
        {
            return ex is not null && ex.GetType().FullName == "Java.Lang.IllegalStateException";
        }

        /// <summary>
        /// Logs a debug message if debugging is enabled.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        private static void LogDebug(string format, params object[] args)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(format, args);
#endif
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        private static void LogError(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }

        /// <summary>
        /// Handles the sensor data updated event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="data">The updated sensor data.</param>
        private void OnSensorDataUpdated(object? sender, SensorData data)
        {
            // Update UI on the main thread
            if (Dispatcher.IsDispatchRequired)
            {
                Dispatcher.Dispatch(() =>
                {
                    UpdateOverlayLabels();
                    UpdateSensorItems(data);
                });
            }
            else
            {
                UpdateOverlayLabels();
                UpdateSensorItems(data);
            }
        }

        /// <summary>
        /// Updates the overlay labels with the current sensor data.
        /// </summary>
        private void UpdateOverlayLabels()
        {
            if (_sensorHelper?.CurrentData == null || BindingContext is not MainPageViewModel viewModel) return;

            var data = _sensorHelper.CurrentData;

            // Update ViewModel properties which will update the UI through bindings
            viewModel.Temperature = data.Temperature.HasValue ? $"{data.Temperature:F1}°C" : "N/A";
            viewModel.City = data.City ?? "Unknown";
            viewModel.Altitude = data.Altitude.HasValue ? $"{data.Altitude:F0}m" : "N/A";
            viewModel.Country = data.Country ?? "Unknown";
        }

        /// <summary>
        /// Updates the sensor items in the ViewModel with the current sensor data.
        /// </summary>
        /// <param name="data">The sensor data to update from</param>
        private void UpdateSensorItems(SensorData data)
        {
            if (BindingContext is MainPageViewModel viewModel)
            {
                viewModel.UpdateSensorItemsFromData(data);
            }
        }

        /// <summary>
        /// Handles the sensor value settings request event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSensorValueSettingsRequested(object? sender, SensorValueSettingsRequestedEventArgs e)
        {
            ShowSensorValueSettings();
        }

        /// <summary>
        /// Shows the sensor value settings as an overlay.
        /// </summary>
        private async void ShowSensorValueSettings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainPage] ShowSensorValueSettings called");
                
                // Create the settings view
                var settingsView = new SensorValueSettingsView();

                // Load existing settings
                var settingsViewModel = settingsView.BindingContext as SensorValueSettingsViewModel;
                if (settingsViewModel != null)
                {
                    await settingsViewModel.LoadSettingsAsync();
                    System.Diagnostics.Debug.WriteLine("[MainPage] Settings loaded. Available: {0}, Visible: {1}", 
                        settingsViewModel.AvailableSensorItems.Count,
                        settingsViewModel.VisibleSensorItems.Count);
                }

                // Create an overlay layout
                var overlayLayout = new Grid
                {
                    BackgroundColor = Colors.Black.MultiplyAlpha(0.7f), // Semi-transparent background
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                    InputTransparent = false
                };

                // Add the settings view to the overlay with a container for proper sizing
                var settingsContainer = new Grid
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 300,
                    Margin = new Thickness(20),
                    InputTransparent = false
                };
                settingsContainer.Children.Add(settingsView);
                settingsView.HorizontalOptions = LayoutOptions.Fill;
                settingsView.VerticalOptions = LayoutOptions.Fill;
                settingsView.InputTransparent = false;

                overlayLayout.Children.Add(settingsContainer);

                // Helper function to close overlay and reload settings
                async void CloseOverlayAndReloadSettings()
                {
                    if (Content is Grid grid)
                    {
                        grid.Children.Remove(overlayLayout);
                    }
                    // Reload settings to update the sensor value display
                    await ReloadSensorSettingsAsync();
                }

                // Add the overlay to the main page - use Grid instead of Layout
                if (Content is Grid mainGrid)
                {
                    System.Diagnostics.Debug.WriteLine("[MainPage] Adding overlay to main Grid");
                    
                    // Add overlay to span all rows
                    Grid.SetRowSpan(overlayLayout, 2);
                    mainGrid.Children.Add(overlayLayout);

                    // Add a close gesture to the overlay background (but not the settings container)
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += async (s, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine("[MainPage] Overlay background tapped - closing");
                        CloseOverlayAndReloadSettings();
                    };
                    overlayLayout.GestureRecognizers.Add(tapGesture);

                    // Prevent taps on the settings container from closing the overlay
                    var containerTapGesture = new TapGestureRecognizer();
                    containerTapGesture.Tapped += (s, e) =>
                    {
                        // Do nothing - just consume the tap
                        System.Diagnostics.Debug.WriteLine("[MainPage] Settings container tapped - consuming");
                    };
                    settingsContainer.GestureRecognizers.Add(containerTapGesture);

                    // Add a close button to the settings container
                    var closeButton = new Button
                    {
                        Text = "✕",
                        BackgroundColor = Colors.Red,
                        TextColor = Colors.White,
                        WidthRequest = 32,
                        HeightRequest = 32,
                        CornerRadius = 16,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Start,
                        Margin = new Thickness(0, -16, -16, 0),
                        ZIndex = 100
                    };

                    closeButton.Clicked += async (s, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine("[MainPage] Close button clicked - closing overlay");
                        CloseOverlayAndReloadSettings();
                    };

                    // Add the close button to the settings container
                    settingsContainer.Children.Add(closeButton);
                    
                    System.Diagnostics.Debug.WriteLine("[MainPage] Overlay added successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MainPage] Content is not a Grid, type: {0}", Content?.GetType().Name ?? "null");
                }
            }
            catch (Exception ex)
            {
                LogError("Error showing sensor value settings: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Handles the preview image tap event to open the default operating system image gallery.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnPreviewImageTapped(object? sender, EventArgs e)
        {
            try
            {
                string picturesDirectory = Settings.OutputPath;

                if (!Directory.Exists(picturesDirectory))
                {
                    LogDebug("[OnPreviewImageTapped] Directory does not exist: {0}", picturesDirectory);
                    await Shell.Current.DisplayAlertAsync("No Images", "No images found in the CekliCam folder.", "OK");
                    return;
                }

                // Find the most recent image file to open
                var imageFiles = GetAllImageFiles(picturesDirectory);
                if (imageFiles.Length == 0)
                {
                    LogDebug("[OnPreviewImageTapped] No image files found in directory: {0}", picturesDirectory);
                    await Shell.Current.DisplayAlertAsync("No Images", "No images found in the CekliCam folder.", "OK");
                    return;
                }

                // Get the most recent image
                var mostRecentImage = imageFiles.OrderByDescending(f => new FileInfo(f).CreationTime).FirstOrDefault();
                LogDebug("[OnPreviewImageTapped] Opening most recent image: {0}", mostRecentImage);

#if ANDROID
                // For Android, open the specific image file which will allow viewing the folder
                try
                {
                    if (!string.IsNullOrEmpty(mostRecentImage) && File.Exists(mostRecentImage))
                    {
                        // Get the URI for the file using FileProvider for Android 7.0+
                        var context = Android.App.Application.Context;
                        var file = new Java.IO.File(mostRecentImage);
                        
                        Android.Net.Uri? uri;
                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
                        {
                            uri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                                context,
                                $"{context.PackageName}.fileprovider",
                                file);
                        }
                        else
                        {
                            uri = Android.Net.Uri.FromFile(file);
                        }

                        var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                        intent.SetDataAndType(uri, "image/*");
                        intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
                        intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                        
                        context.StartActivity(intent);
                        LogDebug("[OnPreviewImageTapped] Opened image successfully");
                    }
                    else
                    {
                        // Fallback: Open the gallery app
                        var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                        intent.SetData(Android.Provider.MediaStore.Images.Media.ExternalContentUri);
                        intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                        Android.App.Application.Context.StartActivity(intent);
                    }
                }
                catch (Exception ex)
                {
                    LogError("[OnPreviewImageTapped] Error opening image: {0}", ex.Message);
                    // Fallback: Try to open gallery
                    try
                    {
                        var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                        intent.SetData(Android.Provider.MediaStore.Images.Media.ExternalContentUri);
                        intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                        Android.App.Application.Context.StartActivity(intent);
                    }
                    catch
                    {
                        await Shell.Current.DisplayAlertAsync("Open Gallery", "Please use your device's gallery app to view your photos in the CekliCam folder.", "OK");
                    }
                }
#else
                // Use the Launcher to open the most recent image file
                if (!string.IsNullOrEmpty(mostRecentImage) && File.Exists(mostRecentImage))
                {
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFileResult(mostRecentImage)
                    });
                }
#endif
            }
            catch (Exception ex)
            {
                LogError("[OnPreviewImageTapped] Error opening image gallery: {0}", ex.Message);
                await Shell.Current.DisplayAlertAsync("Error", "Could not open image gallery.", "OK");
            }
        }

        /// <summary>
        /// Gets all image files from the specified directory and subdirectories.
        /// </summary>
        /// <param name="folderPath">The folder path to search in.</param>
        /// <returns>Array of image file paths.</returns>
        private string[] GetAllImageFiles(string folderPath)
        {
            try
            {
                var imageExtensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp", "*.webp" };
                var imageFiles = new List<string>();

                foreach (var extension in imageExtensions)
                {
                    imageFiles.AddRange(Directory.GetFiles(folderPath, extension, SearchOption.AllDirectories));
                }

                return imageFiles.OrderBy(f => new FileInfo(f).CreationTimeUtc).ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting image files: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Shows a dialog with image selection from the specified folder.
        /// </summary>
        /// <param name="folderPath">The folder path to get images from.</param>
        private async Task ShowImageSelectionDialog(string folderPath)
        {
            var imageFiles = GetAllImageFiles(folderPath);

            if (imageFiles.Length == 0)
            {
                await Shell.Current.DisplayAlertAsync("No Images", "No images found in the selected folder.", "OK");
                return;
            }

            // Create a list of image filenames for the user to select
            var imageNames = imageFiles.Select(f => Path.GetFileName(f)).ToArray();

            var selectedImage = await Shell.Current.DisplayActionSheetAsync("Select an image", "Cancel", null, imageNames);

            if (!string.IsNullOrEmpty(selectedImage) && selectedImage != "Cancel")
            {
                var selectedFilePath = imageFiles.First(f => Path.GetFileName(f) == selectedImage);

                // Open the selected image using the system's default image viewer
                var fileResult = new ReadOnlyFileResult(selectedFilePath);
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = fileResult
                });
            }
        }

        /// <summary>
        /// Handles the Flip Camera button click event to toggle between front and back cameras.
        /// </summary>
        private async void OnFlipCameraButtonClicked(object sender, EventArgs e)
        {
            try
            {
                bool success = await CameraHelper.FlipCameraAsync(CameraView);
                if (success)
                {
                    LogDebug("[MainPage] Camera flip completed successfully.");
                }
                else
                {
                    LogDebug("[MainPage] Camera flip failed or no alternate camera available.");
                }
            }
            catch (Exception ex)
            {
                LogError("[MainPage] Error flipping camera: {0}", ex.Message);
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// A simple implementation of ReadOnlyFile for opening files.
    /// </summary>
    internal class ReadOnlyFileResult : Microsoft.Maui.Storage.ReadOnlyFile
    {
        public ReadOnlyFileResult(string fullPath) : base(fullPath)
        {
        }
    }
}