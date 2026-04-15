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
        /// Runs on MainThread to prevent conflicts with Android lifecycle.
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

                LogDebug("[CameraHelper] Available cameras ({0}):", cameras.Count);
                foreach (var cam in cameras)
                {
                    LogDebug("  - {0} (Position={1})", cam.Name, cam.Position);
                }

                // Prefer rear camera by position
                CameraInfo? rearCamera = cameras.FirstOrDefault(c => c.Position == CameraPosition.Rear);
                CameraInfo selected = rearCamera ?? cameras[0];

                // ✅ Set camera on MainThread to prevent lifecycle conflicts
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    cameraView.SelectedCamera = selected;
                    LogDebug("[CameraHelper] Selected camera: {0}", selected.Name);
                });

                return selected;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception selecting camera: {0}", ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Switches to the next available camera device (front/rear toggle).
        /// Runs on MainThread to prevent lifecycle conflicts.
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
                var nextCamera = cameraList[nextIndex];

                // ✅ Set camera on MainThread to prevent lifecycle conflicts
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    cameraView.SelectedCamera = nextCamera;
                    LogDebug("[CameraHelper] Toggled to camera: {0}", nextCamera.Name);
                });

                return nextCamera;
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
        /// Runs on MainThread to prevent lifecycle conflicts.
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

                // ✅ Start preview on MainThread; use TCS so we actually await the result
                var tcs = new TaskCompletionSource<bool>();

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await cameraView.StartCameraPreview(CancellationToken.None);
                        LogDebug("[CameraHelper] StartCameraPreview completed");
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        LogDebug("[CameraHelper] MainThread preview start error: {0}", ex.Message);
                        tcs.TrySetResult(false);
                    }
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception starting preview: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops the camera preview gracefully.
        /// Runs on MainThread to prevent lifecycle conflicts.
        /// </summary>
        public static void StopPreview(CameraView cameraView)
        {
            try
            {
                // ✅ Stop preview on MainThread to prevent lifecycle conflicts
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        cameraView.StopCameraPreview();
                        LogDebug("[CameraHelper] Camera preview stopped");
                    }
                    catch (Exception ex)
                    {
                        LogDebug("[CameraHelper] MainThread preview stop error: {0}", ex.Message);
                    }
                });
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
        /// Runs on MainThread to prevent lifecycle conflicts.
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

                // ✅ Trigger capture on MainThread to prevent lifecycle conflicts
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await cameraView.CaptureImage(CancellationToken.None);
                        LogDebug("[CameraHelper] CaptureImage triggered");
                    }
                    catch (Exception ex)
                    {
                        LogDebug("[CameraHelper] MainThread capture error: {0}", ex.Message);
                    }
                });
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
        /// Runs on MainThread to prevent lifecycle conflicts.
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

                LogDebug("[CameraHelper] Starting video recording on camera: {0}", cameraView.SelectedCamera.Name);

                // ✅ Start recording on MainThread to prevent lifecycle conflicts
                bool success = false;
                var tcs = new TaskCompletionSource<bool>();

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Add timeout to prevent hang
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        try
                        {
                            await cameraView.StartVideoRecording(cts.Token).ConfigureAwait(false);
                            LogDebug("[CameraHelper] Video recording started successfully");
                            tcs.SetResult(true);
                        }
                        catch (OperationCanceledException)
                        {
                            LogDebug("[CameraHelper] StartVideoRecording TIMED OUT after 5 seconds");
                            tcs.SetResult(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogDebug("[CameraHelper] MainThread recording start error: {0}", ex.Message);
                        tcs.SetResult(false);
                    }
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] Exception starting recording: {0}", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Stops video recording and returns the recorded video as a Stream.
        /// Returns null on failure.
        /// Runs on MainThread to prevent lifecycle conflicts.
        /// </summary>
        public static async Task<Stream?> StopVideoRecordingAsync(CameraView cameraView)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                LogDebug("[CameraHelper] Calling StopVideoRecording on camera: {0}, IsBusy={1}", cameraView.SelectedCamera?.Name ?? "null", cameraView.IsBusy);

                // ✅ Stop recording on MainThread to prevent lifecycle conflicts
                var tcs = new TaskCompletionSource<Stream?>();

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Add timeout to prevent indefinite hang (some devices/codecs cause deadlock)
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        try
                        {
                            var videoStream = await cameraView.StopVideoRecording(cts.Token).ConfigureAwait(false);
                            stopwatch.Stop();
                            LogDebug("[CameraHelper] StopVideoRecording completed in {0} ms, stream: {1} bytes, stream type: {2}, CanSeek={3}",
                                stopwatch.ElapsedMilliseconds,
                                videoStream?.Length.ToString() ?? "null",
                                videoStream?.GetType().FullName ?? "null",
                                videoStream?.CanSeek.ToString() ?? "n/a");
                            tcs.SetResult(videoStream);
                        }
                        catch (OperationCanceledException)
                        {
                            stopwatch.Stop();
                            LogDebug("[CameraHelper] StopVideoRecording TIMED OUT after {0} ms — camera may be stuck", stopwatch.ElapsedMilliseconds);
                            tcs.SetResult(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        LogDebug("[CameraHelper] MainThread recording stop error after {0} ms: {1}", stopwatch.ElapsedMilliseconds, ex.Message);
                        tcs.SetResult(null);
                    }
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogDebug("[CameraHelper] Exception stopping recording after {0} ms: {1}", stopwatch.ElapsedMilliseconds, ex.ToString());
                return null;
            }
        }

        #endregion

        #region Flash

        /// <summary>
        /// Toggles the camera flash mode: Off → On → Off.
        /// Returns the new flash mode.
        /// Runs on MainThread to prevent lifecycle conflicts.
        /// </summary>
        public static CameraFlashMode CycleFlashMode(CameraView cameraView)
        {
            var next = cameraView.CameraFlashMode == CameraFlashMode.Off
                ? CameraFlashMode.On
                : CameraFlashMode.Off;

            // ✅ Update flash mode on MainThread to prevent lifecycle conflicts
            MainThread.BeginInvokeOnMainThread(() =>
            {
                cameraView.CameraFlashMode = next;
                LogDebug("[CameraHelper] Flash mode set to: {0}", next);
            });

            return next;
        }

        #endregion

        #region Zoom

        /// <summary>
        /// Returns the total number of camera devices available on the current hardware.
        /// Used by the ViewModel to decide whether to show the zoom preset strip.
        /// </summary>
        public static async Task<int> GetCameraCountAsync(CameraView cameraView)
        {
            try
            {
                var cameras = await cameraView.GetAvailableCameras(CancellationToken.None);
                return cameras?.Count ?? 0;
            }
            catch (Exception ex)
            {
                LogDebug("[CameraHelper] GetCameraCountAsync exception: {0}", ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Applies zoom to the camera.
        /// <paramref name="zoomFactor"/> is passed directly to <see cref="CameraView.ZoomFactor"/>.
        /// The ViewModel is responsible for ensuring the value is within the camera's supported range.
        /// Runs on MainThread to prevent lifecycle conflicts.
        /// </summary>
        public static void SetZoom(CameraView cameraView, float zoomFactor)
        {
            // ✅ Update zoom on MainThread to prevent lifecycle conflicts
            MainThread.BeginInvokeOnMainThread(() =>
            {
                cameraView.ZoomFactor = zoomFactor;
                LogDebug("[CameraHelper] ZoomFactor set to: {0}", zoomFactor);
            });
        }

        #endregion

        #region Logging

        [System.Diagnostics.Conditional("DEBUG")]
        private static void LogDebug(string format, params object?[] args)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
        }

        #endregion

    }
}
