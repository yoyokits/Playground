using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelCamApp.Helpers;
using TravelCamApp.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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
        private const int CAMERA_STABILIZATION_DELAY = 500; // Delay to allow camera callbacks to complete

        // Guards against starting camera work after page disappears (race with CamerasLoaded/async continuations).
        private volatile bool _isPageVisible;
        private volatile int _lifecycleGeneration;
        private volatile bool _isDisposing = false;

        // Flag to prevent multiple simultaneous recording operations
        private volatile bool _isProcessingRecordingToggle = false;

        #endregion

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
        }

        #endregion

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

                if (CameraView.Camera is null || _isPreviewRunning)
                {
                    return;
                }

                _previewCts?.Cancel();
                _previewCts = new CancellationTokenSource();
            }
            finally
            {
                _cameraLifecycleLock.Release();
            }

            await StartPreviewSafeAsync(gen);
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
            if (_isDisposing) return;

            base.OnAppearing();

            _isPageVisible = true;
            _isDisposing = false;
            Interlocked.Increment(ref _lifecycleGeneration);
            var gen = _lifecycleGeneration;

            if (!await CameraHelper.RequestPermissionsAsync())
            {
                LogDebug("Camera permissions not granted");
                return;
            }

            // Only start preview if camera has been configured
            if (_isCameraConfigured)
            {
                await StartPreviewSafeAsync(gen);
            }
        }

        /// <inheritdoc/>
        protected override async void OnDisappearing()
        {
            _isPageVisible = false;
            _isDisposing = true;
            Interlocked.Increment(ref _lifecycleGeneration);

            // Cancel any pending preview operations first
            _previewCts?.Cancel();

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
                    var stream = await CameraHelper.TakePhotoAsync(CameraView);
                    if (stream is null)
                    {
                        LogDebug("Failed to take photo - stream is null");
                        return;
                    }

                    var filePath = await FileHelper.SavePhotoAsync(stream, "CekliCam");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        viewModel.LastCaptureImage = ImageSource.FromFile(filePath);
                        LogDebug("[MainPage] Photo saved to: {0}", filePath);
                    }
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
                // Look for the most recently created image file in the TravelCam directory
                var travelCamPath = Path.Combine(FileSystem.AppDataDirectory, "TravelCam");
                if (!Directory.Exists(travelCamPath))
                {
                    LogDebug("[MainPage] TravelCam directory does not exist: {0}", travelCamPath);
                    return;
                }

                // Get all years directories
                var yearDirs = Directory.GetDirectories(travelCamPath).OrderByDescending(d => d).ToArray();

                string? lastImagePath = null;

                foreach (var yearDir in yearDirs)
                {
                    var monthDirs = Directory.GetDirectories(yearDir).OrderByDescending(d => d).ToArray();

                    foreach (var monthDir in monthDirs)
                    {
                        // Get all jpg files in the month directory
                        var imageFiles = Directory.GetFiles(monthDir, "*.jpg")
                                                 .Concat(Directory.GetFiles(monthDir, "*.jpeg"))
                                                 .Concat(Directory.GetFiles(monthDir, "*.png"))
                                                 .OrderByDescending(f => new FileInfo(f).CreationTime)
                                                 .ToArray();

                        if (imageFiles.Length > 0)
                        {
                            lastImagePath = imageFiles[0];
                            break;
                        }
                    }

                    if (lastImagePath != null)
                        break;
                }

                if (!string.IsNullOrEmpty(lastImagePath) && File.Exists(lastImagePath))
                {
                    viewModel.LastCaptureImage = ImageSource.FromFile(lastImagePath);
                    LogDebug("[MainPage] Loaded last taken image: {0}", lastImagePath);
                }
                else
                {
                    LogDebug("[MainPage] No previous images found to load as default preview");
                    // The view model will keep its default state (no image)
                }
            }
            catch (Exception ex)
            {
                LogError("[MainPage] Error loading last taken image: {0}", ex.Message);
            }
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

        #endregion
    }
}