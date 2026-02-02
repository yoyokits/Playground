using System.Linq;
using Camera.MAUI;

namespace TravelCamApp.Helpers
{
    public static class CameraHelper
    {
        #region Methods

        public static void ConfigureDevices(CameraView cameraView)
        {
            if (cameraView.NumCamerasDetected <= 0)
            {
                return;
            }

            if (cameraView.NumMicrophonesDetected > 0)
            {
                cameraView.Microphone = cameraView.Microphones.FirstOrDefault();
            }

            cameraView.Camera = cameraView.Cameras.FirstOrDefault();
        }

        public static async Task<bool> RequestPermissionsAsync()
        {
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
            return cameraStatus == PermissionStatus.Granted && micStatus == PermissionStatus.Granted;
        }

        public static async Task StartPreviewAsync(CameraView cameraView)
        {
            await cameraView.StartCameraAsync();
        }

        public static Task StartRecordingAsync(CameraView cameraView, string filePath)
        {
            cameraView.AutoRecordingFile = filePath;
            cameraView.AutoStartRecording = true;
            return Task.CompletedTask;
        }

        public static async Task StopPreviewAsync(CameraView cameraView)
        {
            await cameraView.StopCameraAsync();
        }

        public static Task StopRecordingAsync(CameraView cameraView)
        {
            cameraView.AutoStartRecording = false;
            return Task.CompletedTask;
        }

        public static async Task<Stream?> TakePhotoAsync(CameraView cameraView)
        {
            return await cameraView.TakePhotoAsync();
        }

        #endregion
    }
}
