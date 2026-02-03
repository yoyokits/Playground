using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Camera.MAUI;

namespace TravelCamApp.Helpers
{
    /// <summary>
    /// Provides helper methods for camera operations including configuration, 
    /// preview control, recording, and photo capture.
    /// </summary>
    public static class CameraHelper
    {
        #region Fields

        private const int CAMERA_STABILIZATION_DELAY = 500; // Delay to allow camera callbacks to complete

        #endregion

        #region Methods

        /// <summary>
        /// Configures the available cameras and microphones for the camera view.
        /// </summary>
        /// <param name="cameraView">The camera view to configure.</param>
        public static void ConfigureDevices(CameraView cameraView)
        {
            try
            {
                LogDebug("[CameraHelper] Configuring devices - Cameras detected: {0}, Microphones detected: {1}",
                    cameraView.NumCamerasDetected, cameraView.NumMicrophonesDetected);

                if (cameraView.NumCamerasDetected <= 0)
                {
                    LogDebug("[CameraHelper] No cameras detected");
                    return;
                }

                if (cameraView.NumMicrophonesDetected > 0)
                {
                    cameraView.Microphone = cameraView.Microphones.FirstOrDefault();
                    LogDebug("[CameraHelper] Microphone configured: {0}",
                        cameraView.Microphone?.Name ?? "None");
                }

                var availableCameras = cameraView.Cameras?.ToList() ?? new List<CameraInfo>();
                LogDebug("[CameraHelper] Available cameras count: {0}", availableCameras.Count);

                foreach (var cam in availableCameras)
                {
                    LogDebug("[CameraHelper] Available camera: {0}, Position: {1}",
                        cam.Name, cam.Position);
                }

                var camera = availableCameras.FirstOrDefault();
                if (camera != null)
                {
                    cameraView.Camera = camera;
                    LogDebug("[CameraHelper] Camera configured: {0}, Position: {1}",
                        camera.Name, camera.Position);
                }
                else
                {
                    LogDebug("[CameraHelper] No camera available to configure");
                }

                LogDebug("[CameraHelper] Current camera after configuration: {0}",
                    cameraView.Camera?.Name ?? "None");
            }
            catch (Exception ex)
            {
                LogError("[CameraHelper] Error configuring devices: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Requests camera and microphone permissions from the user.
        /// </summary>
        /// <returns>True if both permissions are granted, false otherwise.</returns>
        public static async Task<bool> RequestPermissionsAsync()
        {
            try
            {
                var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

                bool permissionsGranted = cameraStatus == PermissionStatus.Granted && micStatus == PermissionStatus.Granted;

                if (!permissionsGranted)
                {
                    LogDebug("Permissions not granted. Camera: {0}, Microphone: {1}", cameraStatus, micStatus);
                }

                return permissionsGranted;
            }
            catch (Exception ex)
            {
                LogError("Error requesting permissions: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Starts the camera preview asynchronously.
        /// </summary>
        /// <param name="cameraView">The camera view to start preview for.</param>
        /// <returns>True if the preview started successfully, false otherwise.</returns>
        public static async Task<bool> StartPreviewAsync(CameraView cameraView)
        {
            try
            {
                LogDebug("[CameraHelper] Attempting to start camera preview. Current camera: {0}",
                    cameraView.Camera?.Name ?? "None");

                if (cameraView.Camera is null)
                {
                    LogDebug("[CameraHelper] Cannot start preview: no camera selected");
                    return false;
                }

                await cameraView.StartCameraAsync();
                LogDebug("[CameraHelper] Camera preview started successfully. Camera: {0}",
                    cameraView.Camera?.Name);
                return true;
            }
            catch (Exception ex)
            {
                LogError("[CameraHelper] Error starting camera preview: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Starts video recording to the specified file path.
        /// </summary>
        /// <param name="cameraView">The camera view to start recording with.</param>
        /// <param name="filePath">The file path to save the recording to.</param>
        /// <returns>True if recording started successfully, false otherwise.</returns>
        public static async Task<bool> StartRecordingAsync(CameraView cameraView, string filePath)
        {
            try
            {
                LogDebug("[CameraHelper] Attempting to start recording. Current camera: {0}, FilePath: {1}",
                    cameraView.Camera?.Name ?? "None", filePath);

                if (cameraView.Camera is null)
                {
                    LogDebug("[CameraHelper] Cannot start recording: no camera selected");
                    return false;
                }

                cameraView.AutoRecordingFile = filePath;
                cameraView.AutoStartRecording = true;

                LogDebug("[CameraHelper] Started recording to: {0}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                LogError("[CameraHelper] Error starting recording: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops the camera preview asynchronously.
        /// </summary>
        /// <param name="cameraView">The camera view to stop preview for.</param>
        /// <returns>True if the preview stopped successfully, false otherwise.</returns>
        public static async Task<bool> StopPreviewAsync(CameraView cameraView)
        {
            try
            {
                LogDebug("[CameraHelper] Attempting to stop camera preview. Current camera: {0}",
                    cameraView.Camera?.Name ?? "None");

                await cameraView.StopCameraAsync();
                LogDebug("[CameraHelper] Camera preview stopped successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError("[CameraHelper] Error stopping camera preview: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops video recording and releases camera resources.
        /// </summary>
        /// <param name="cameraView">The camera view to stop recording for.</param>
        /// <returns>True if recording stopped successfully, false otherwise.</returns>
        public static async Task<bool> StopRecordingAsync(CameraView cameraView)
        {
            try
            {
                LogDebug("[CameraHelper] Attempting to stop recording. Current camera: {0}, AutoStartRecording: {1}",
                    cameraView.Camera?.Name ?? "None", cameraView.AutoStartRecording);

                // First, disable auto recording to stop the recording process
                cameraView.AutoStartRecording = false;

                // Small delay to allow the recording to properly terminate
                await Task.Delay(100);

                // Stop camera to release recording use case.
                // This is necessary to release the recording resources
                await cameraView.StopCameraAsync();

                // Allow camera to fully release resources - increase the delay
                await Task.Delay(250);

                LogDebug("[CameraHelper] Recording stopped and camera released");
                return true;
            }
            catch (Exception ex)
            {
                LogError("[CameraHelper] Error stopping recording: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Restarts the camera preview by stopping and starting the camera.
        /// </summary>
        /// <param name="cameraView">The camera view to restart preview for.</param>
        /// <returns>True if the preview restarted successfully, false otherwise.</returns>
        public static async Task<bool> RestartPreviewAsync(CameraView cameraView)
        {
            try
            {
                // First stop the camera if it's running
                await cameraView.StopCameraAsync();

                // Small delay to ensure resources are released
                await Task.Delay(100);

                // Then start the camera again
                await cameraView.StartCameraAsync();

                LogDebug("Camera preview restarted successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError("Error restarting camera preview: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Takes a photo using the camera view.
        /// </summary>
        /// <param name="cameraView">The camera view to take photo with.</param>
        /// <returns>A stream containing the captured photo, or null if failed.</returns>
        public static async Task<Stream?> TakePhotoAsync(CameraView cameraView)
        {
            try
            {
                LogDebug("[CameraHelper] Attempting to take photo. Current camera: {0}",
                    cameraView.Camera?.Name ?? "None");

                if (cameraView.Camera is null)
                {
                    LogDebug("[CameraHelper] Cannot take photo: no camera selected");
                    return null;
                }

                var stream = await cameraView.TakePhotoAsync();
                LogDebug("[CameraHelper] Photo taken successfully");
                return stream;
            }
            catch (Exception ex)
            {
                LogError("[CameraHelper] Error taking photo: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Logs a debug message if debugging is enabled.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        private static void LogDebug(string format, params object?[] args)
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
        private static void LogError(string format, params object?[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }

        /// <summary>
        /// Starts video recording with proper lifecycle management.
        /// </summary>
        /// <param name="cameraView">The camera view to start recording with.</param>
        /// <param name="filePath">The file path to save the recording to.</param>
        /// <param name="isPreviewRunning">The current preview running state.</param>
        /// <returns>Tuple containing success status and updated preview running state.</returns>
        public static async Task<(bool Success, bool IsPreviewRunning)> StartVideoRecordingAsync(CameraView cameraView, string filePath, bool isPreviewRunning)
        {
            try
            {
                bool currentPreviewState = isPreviewRunning;

                // Ensure camera is properly configured before recording
                if (cameraView.Camera is null)
                {
                    ConfigureDevices(cameraView);
                }

                // If preview is not running, start it before recording
                if (!currentPreviewState)
                {
                    bool previewSuccess = await StartPreviewAsync(cameraView);
                    if (!previewSuccess)
                    {
                        LogError("Failed to start preview before recording");
                        return (false, currentPreviewState);
                    }

                    // Wait for camera to be ready
                    await Task.Delay(CAMERA_STABILIZATION_DELAY);
                    currentPreviewState = true;
                }

                bool success = await StartRecordingAsync(cameraView, filePath);
                if (!success)
                {
                    LogError("Failed to start recording");
                    return (false, currentPreviewState);
                }

                LogDebug("Recording started successfully");
                return (true, currentPreviewState);
            }
            catch (Exception ex)
            {
                LogError("Error starting video recording: {0}", ex.Message);
                return (false, isPreviewRunning);
            }
        }

        /// <summary>
        /// Stops video recording and manages camera state.
        /// </summary>
        /// <param name="cameraView">The camera view to stop recording with.</param>
        /// <param name="isPreviewRunning">The current preview running state.</param>
        /// <param name="updateThumbnailFunc">Function to update the thumbnail after stopping.</param>
        /// <returns>Tuple containing success status and updated preview running state.</returns>
        public static async Task<(bool Success, bool IsPreviewRunning)> StopVideoRecordingAsync(CameraView cameraView, bool isPreviewRunning, Func<Task> updateThumbnailFunc = null)
        {
            try
            {
                bool currentPreviewState = isPreviewRunning;

                bool success = await StopRecordingAsync(cameraView);
                if (!success)
                {
                    LogError("Failed to stop recording properly");
                    return (false, currentPreviewState);
                }

                LogDebug("Recording stopped successfully");

                // Wait for camera to stabilize after stopping recording
                LogDebug("Waiting for camera to stabilize after stopping recording...");
                await Task.Delay(CAMERA_STABILIZATION_DELAY * 2); // Double the delay to ensure camera is fully closed

                // Start the camera preview again to ensure it's ready for the next operation
                // Since StopRecording already stopped the camera, we just need to start it
                LogDebug("Attempting to restart camera preview...");

                // Ensure camera is properly configured before starting
                if (cameraView.Camera is null)
                {
                    ConfigureDevices(cameraView);
                }

                if (cameraView.Camera is not null)
                {
                    bool startSuccess = await StartPreviewAsync(cameraView);
                    if (startSuccess)
                    {
                        currentPreviewState = true;
                        LogDebug("Camera preview restarted successfully");
                    }
                    else
                    {
                        LogError("Failed to start camera preview after stopping recording");
                        currentPreviewState = false;
                    }
                }
                else
                {
                    LogError("No camera available to start preview after stopping recording");
                    currentPreviewState = false;
                }

                // Update the thumbnail after camera has restarted
                if (updateThumbnailFunc != null)
                {
                    LogDebug("Scheduling video preview update...");
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Much longer delay to ensure camera is fully stabilized after stopping recording
                            await Task.Delay(1000);
                            await updateThumbnailFunc();
                        }
                        catch (Exception ex)
                        {
                            LogError("Error updating video preview after recording: {0}", ex.Message);
                        }
                    });
                }

                LogDebug("Stop recording process completed");
                return (true, currentPreviewState);
            }
            catch (Exception ex)
            {
                LogError("Error stopping video recording: {0}", ex.Message);
                return (false, isPreviewRunning);
            }
        }

        /// <summary>
        /// Updates the video preview thumbnail after recording stops.
        /// </summary>
        /// <param name="cameraView">The camera view to update.</param>
        /// <param name="isPreviewRunning">Current preview running state.</param>
        /// <param name="isPageVisible">Current page visibility state.</param>
        /// <param name="isDisposing">Current disposing state.</param>
        /// <param name="updateImageAction">Action to update the image in the UI.</param>
        /// <returns>True if update was successful, false otherwise.</returns>
        public static async Task<bool> UpdateVideoPreviewAsync(CameraView cameraView, bool isPreviewRunning, bool isPageVisible, bool isDisposing, Action<ImageSource> updateImageAction)
        {
            try
            {
                LogDebug("UpdateVideoPreviewAsync called - IsPageVisible: {0}, IsDisposing: {1}, IsPreviewRunning: {2}",
                    isPageVisible, isDisposing, isPreviewRunning);

                if (isDisposing) return false;

                LogDebug("UpdateVideoPreviewAsync - Current camera: {0}",
                    cameraView.Camera?.Name ?? "None");

                // Check if camera is available before proceeding
                if (cameraView.Camera is null)
                {
                    LogDebug("No camera available in UpdateVideoPreviewAsync, configuring devices...");
                    ConfigureDevices(cameraView);
                }

                // Double-check camera availability after configuration
                if (cameraView.Camera is null)
                {
                    LogDebug("Cannot update video preview: no camera available after configuration");
                    return false;
                }

                // Ensure preview is running
                if (!isPreviewRunning)
                {
                    LogDebug("Preview not running, starting preview for thumbnail...");
                    bool success = await StartPreviewAsync(cameraView);
                    if (!success)
                    {
                        LogDebug("Failed to start preview for video thumbnail");
                        return false;
                    }

                    // Wait for camera session to be fully ready
                    await Task.Delay(CAMERA_STABILIZATION_DELAY);

                    LogDebug("Preview started for thumbnail update");
                }

                // Wait additional time for camera to be fully ready after state transition
                LogDebug("Waiting for camera to be ready before taking thumbnail...");
                await Task.Delay(200);

                LogDebug("Attempting to take thumbnail photo...");
                var stream = await TakePhotoAsync(cameraView);
                if (stream is null)
                {
                    LogDebug("Failed to get thumbnail for video preview - stream is null");
                    return false;
                }

                using var memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                var bytes = memory.ToArray();

                var imageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                updateImageAction(imageSource);

                LogDebug("Thumbnail updated successfully");
                return true;
            }
            catch (Exception ex) when (IsJavaIllegalStateException(ex))
            {
                // Handle the case where the camera was closed during the operation
                LogError("Camera already closed during preview update: {0}", ex?.Message ?? "Unknown error");
                return false;
            }
            catch (Exception ex)
            {
                LogError("Error updating video preview: {0}", ex?.Message ?? "Unknown error");
                return false;
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

        #endregion
    }
}