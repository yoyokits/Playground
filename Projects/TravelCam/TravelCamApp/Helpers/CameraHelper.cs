// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// CameraHelper provides static camera operations for CommunityToolkit.Maui.Camera.
// Each method handles its own state checks and returns a clean result.
// The caller (MainPageViewModel) manages camera lifecycle.
//
// API reference: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/camera-view

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace TravelCamApp.Helpers
{
    public static class CameraHelper
    {
        #region Device Selection

        /// <summary>
        /// Selects the first available camera (prefers rear camera).
        /// Returns the selected CameraInfo, or null if none found.
        /// </summary>
        public static async Task<CameraInfo?> SelectFirstAvailableCameraAsync(CameraView cameraView)
        {
            try
            {
                var cameras = await cameraView.GetAvailableCameras(CancellationToken.None);
                if (cameras == null || cameras.Count == 0)
                {
                    LogDebug("[CameraHelper] No camera devices available");
                    return null;
                }

                // Prefer rear camera by position
                CameraInfo? rearCamera = cameras.FirstOrDefault(c => c.Position == CameraPosition.Rear);
                CameraInfo selected = rearCamera ?? cameras[0];

                cameraView.SelectedCamera = selected;
                LogDebug("[CameraHelper] Selected camera: {0}", selected.Name);
                return selected;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception selecting camera: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Switches to the next available camera device (front/rear toggle).
        /// </summary>
        public static async Task<CameraInfo?> ToggleCameraDeviceAsync(CameraView cameraView)
        {
            try
            {
                var cameras = await cameraView.GetAvailableCameras(CancellationToken.None);
                if (cameras == null || cameras.Count <= 1)
                    return cameraView.SelectedCamera;

                var cameraList = cameras.ToList();
                int currentIndex = -1;
                for (int i = 0; i < cameraList.Count; i++)
                {
                    if (cameraList[i].Position == (cameraView.SelectedCamera?.Position ?? CameraPosition.Rear))
                    {
                        currentIndex = i;
                        break;
                    }
                }

                int nextIndex = (currentIndex + 1) % cameraList.Count;
                cameraView.SelectedCamera = cameraList[nextIndex];
                LogDebug("[CameraHelper] Toggled to camera: {0}", cameraList[nextIndex].Name);
                return cameraList[nextIndex];
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception toggling camera: {0}", ex.Message);
                return cameraView.SelectedCamera;
            }
        }

        #endregion

        #region Preview

        /// <summary>
        /// Starts the camera preview. Selects a camera first if none is selected.
        /// Returns true if preview started successfully.
        /// </summary>
        public static async Task<bool> StartPreviewAsync(CameraView cameraView)
        {
            try
            {
                if (cameraView.SelectedCamera == null)
                    await SelectFirstAvailableCameraAsync(cameraView);

                if (cameraView.SelectedCamera == null)
                {
                    LogDebug("[CameraHelper] No camera device to start preview");
                    return false;
                }

                await cameraView.StartCameraPreview(CancellationToken.None);
                LogDebug("[CameraHelper] StartCameraPreview completed");
                return true;
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
        public static void StopPreview(CameraView cameraView)
        {
            try
            {
                cameraView.StopCameraPreview();
                LogDebug("[CameraHelper] Camera preview stopped");
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception stopping preview: {0}", ex.Message);
            }
        }

        #endregion

        #region Photo Capture

        /// <summary>
        /// Triggers a photo capture. The result is returned via the CameraView.MediaCaptured event.
        /// Subscribe to MediaCaptured on the CameraView before calling this.
        /// </summary>
        public static async Task TriggerCaptureAsync(CameraView cameraView)
        {
            try
            {
                if (cameraView.SelectedCamera == null)
                {
                    LogDebug("[CameraHelper] Cannot capture — no camera selected");
                    return;
                }

                await cameraView.CaptureImage(CancellationToken.None);
                LogDebug("[CameraHelper] CaptureImage triggered");
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception triggering capture: {0}", ex.Message);
            }
        }

        #endregion

        #region Video Recording

        /// <summary>
        /// Starts video recording.
        /// Returns true if recording started successfully.
        /// </summary>
        public static async Task<bool> StartVideoRecordingAsync(CameraView cameraView)
        {
            try
            {
                if (cameraView.SelectedCamera == null)
                {
                    LogDebug("[CameraHelper] No camera selected for recording");
                    return false;
                }

                await cameraView.StartVideoRecording(CancellationToken.None);
                LogDebug("[CameraHelper] Video recording started");
                return true;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception starting recording: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops video recording and returns the recorded video as a Stream.
        /// Returns null on failure.
        /// </summary>
        public static async Task<Stream?> StopVideoRecordingAsync(CameraView cameraView)
        {
            try
            {
                var videoStream = await cameraView.StopVideoRecording(CancellationToken.None);
                LogDebug("[CameraHelper] Video recording stopped, stream: {0} bytes",
                    videoStream?.Length.ToString() ?? "null");
                return videoStream;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception stopping recording: {0}", ex.Message);
                return null;
            }
        }

        #endregion

        #region Flash

        /// <summary>
        /// Toggles the camera flash mode: Off → On → Off.
        /// Returns the new flash mode.
        /// </summary>
        public static CameraFlashMode CycleFlashMode(CameraView cameraView)
        {
            var next = cameraView.CameraFlashMode == CameraFlashMode.Off
                ? CameraFlashMode.On
                : CameraFlashMode.Off;

            cameraView.CameraFlashMode = next;
            LogDebug("[CameraHelper] Flash mode set to: {0}", next);
            return next;
        }

        #endregion

        #region Zoom

        /// <summary>
        /// Applies zoom to the camera. zoomFactor is 0.0–1.0 (relative of max).
        /// </summary>
        public static void SetZoom(CameraView cameraView, double zoomFactor)
        {
            zoomFactor = Math.Clamp(zoomFactor, 0.0, 1.0);

            // Camera.MAUI's ZoomFactor is normalized 0.0–1.0 directly.
            // No need to convert using min/max bounds.
            cameraView.ZoomFactor = (float)zoomFactor;

            LogDebug("[CameraHelper] ZoomFactor set to: {0}", cameraView.ZoomFactor);
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
