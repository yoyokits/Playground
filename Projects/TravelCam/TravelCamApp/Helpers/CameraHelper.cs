// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// CameraHelper provides static camera operations for Camera.MAUI.
// Each method handles its own permission/state checks and returns
// a clean result. The caller (MainPageViewModel) manages camera
// lifecycle.
//
// API reference: https://github.com/hjam40/Camera.MAUI (master branch)

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Camera.MAUI;

namespace TravelCamApp.Helpers
{
    public static class CameraHelper
    {
        private const int CameraStabilizationMs = 1500;

        #region Device Selection

        /// <summary>
        /// Selects the first available camera (prefers back camera).
        /// Returns the camera name, or null if none found.
        /// </summary>
        public static string? SelectFirstAvailableCamera(CameraView cameraView)
        {
            ObservableCollection<CameraInfo> cameras = cameraView.Cameras;
            if (cameras == null || cameras.Count == 0)
            {
                LogDebug("[CameraHelper] No camera devices available");
                return null;
            }

            // Prefer back camera by position
            CameraInfo? backCamera = cameras.FirstOrDefault(c => c.Position == CameraPosition.Back);

            CameraInfo selected = backCamera ?? cameras[0];
            cameraView.Camera = selected;
            LogDebug("[CameraHelper] Selected camera: {0}", selected.Name);
            return selected.Name;
        }

        /// <summary>
        /// Switches to the next available camera device (front/back toggle).
        /// </summary>
        public static string? ToggleCameraDevice(CameraView cameraView)
        {
            var cameras = cameraView.Cameras;
            if (cameras == null || cameras.Count <= 1)
                return cameraView.Camera?.Name;

            var current = cameraView.Camera;
            var currentIndex = cameras.IndexOf(current!);
            var nextIndex = (currentIndex + 1) % cameras.Count;

            cameraView.Camera = cameras[nextIndex];
            LogDebug("[CameraHelper] Toggled to camera: {0}", cameras[nextIndex].Name);
            return cameras[nextIndex].Name;
        }

        #endregion

        #region Preview

        /// <summary>
        /// Starts the camera preview. Idempotent -- safe to call multiple times.
        /// Returns true if preview started successfully.
        /// </summary>
        public static async Task<bool> StartPreviewAsync(CameraView cameraView)
        {
            try
            {
                if (cameraView.Camera == null)
                {
                    SelectFirstAvailableCamera(cameraView);
                }

                if (cameraView.Camera == null)
                {
                    LogDebug("[CameraHelper] No camera device to start preview");
                    return false;
                }

                CameraResult result = await cameraView.StartCameraAsync();
                LogDebug("[CameraHelper] StartCameraAsync result: {0}", result);
                return result == CameraResult.Success;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception starting preview: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops the camera preview gracefully.
        /// </summary>
        public static async Task<bool> StopPreviewAsync(CameraView cameraView)
        {
            try
            {
                CameraResult result = await cameraView.StopCameraAsync();
                return result == CameraResult.Success || result == CameraResult.AccessError;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception stopping preview: {0}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Photo Capture

        /// <summary>
        /// Takes a photo and returns the image stream.
        /// Returns null on failure.
        /// </summary>
        public static async Task<Stream?> TakePhotoAsync(CameraView cameraView)
        {
            try
            {
                if (cameraView.Camera == null)
                {
                    LogDebug("[CameraHelper] Cannot take photo -- no camera selected");
                    return null;
                }

                Stream stream = await cameraView.TakePhotoAsync(Camera.MAUI.ImageFormat.JPEG);
                if (stream == null || stream.Length == 0)
                {
                    LogDebug("[CameraHelper] Photo stream is null or empty");
                    return null;
                }

                LogDebug("[CameraHelper] Photo captured: {0} bytes", stream.Length);
                return stream;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception taking photo: {0}", ex.Message);
                return null;
            }
        }

        #endregion

        #region Video Recording

        /// <summary>
        /// Starts video recording to the specified file path.
        /// Returns true if recording started successfully.
        /// </summary>
        public static async Task<bool> StartVideoRecordingAsync(CameraView cameraView, string filePath)
        {
            try
            {
                if (cameraView.Camera == null)
                {
                    LogDebug("[CameraHelper] No camera selected for recording");
                    return false;
                }

                if (!cameraView.Cameras.Any())
                {
                    LogDebug("[CameraHelper] No cameras available for recording");
                    return false;
                }

                // Camera.MAUI needs a running preview + microphone set before recording
                if (cameraView.Microphone == null && cameraView.Microphones.Count > 0)
                {
                    cameraView.Microphone = cameraView.Microphones[0];
                }

                CameraResult result = await cameraView.StartRecordingAsync(filePath);
                LogDebug("[CameraHelper] StartRecordingAsync result: {0}", result);
                return result == CameraResult.Success;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception starting recording: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops video recording. Camera.MAUI handles preview restoration internally.
        /// Returns true if recording was stopped successfully.
        /// </summary>
        public static async Task<bool> StopVideoRecordingAsync(CameraView cameraView)
        {
            try
            {
                CameraResult result = await cameraView.StopRecordingAsync();
                // Give the library time to restore preview state internally
                if (result == CameraResult.Success)
                {
                    await Task.Delay(CameraStabilizationMs);
                }

                LogDebug("[CameraHelper] StopRecordingAsync result: {0}", result);
                return result == CameraResult.Success;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception stopping recording: {0}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Flash / Torch

        /// <summary>
        /// Toggles the camera flash mode: Disabled -> Enabled -> Auto -> Disabled.
        /// </summary>
        public static FlashMode CycleFlashMode(CameraView cameraView)
        {
            var next = cameraView.FlashMode switch
            {
                FlashMode.Disabled => FlashMode.Enabled,
                FlashMode.Enabled => FlashMode.Auto,
                FlashMode.Auto => FlashMode.Disabled,
                _ => FlashMode.Disabled,
            };

            cameraView.FlashMode = next;
            LogDebug("[CameraHelper] Flash mode set to: {0}", next);
            return next;
        }

        #endregion

        #region Zoom

        /// <summary>
        /// Applies zoom to the camera. zoomFactor is 0.0 - 1.0 (relative of max).
        /// </summary>
        public static void SetZoom(CameraView cameraView, double zoomFactor)
        {
            zoomFactor = Math.Clamp(zoomFactor, 0.0, 1.0);

            if (cameraView.Camera != null)
            {
                float minZoom = cameraView.Camera.MinZoomFactor;
                float maxZoom = cameraView.Camera.MaxZoomFactor;
                float actualZoom = (float)(minZoom + (maxZoom - minZoom) * zoomFactor);
                actualZoom = Math.Max(minZoom, Math.Min(maxZoom, actualZoom));
                cameraView.ZoomFactor = actualZoom;
            }
            else
            {
                cameraView.ZoomFactor = (float)zoomFactor;
            }
        }

        #endregion

        #region Logging

        [System.Diagnostics.Conditional("DEBUG")]
        private static void LogDebug(string format, params object?[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }

        #endregion Methods
    }
}
