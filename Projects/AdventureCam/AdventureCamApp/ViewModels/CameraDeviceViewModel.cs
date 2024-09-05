// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace AdventureCamApp.ViewModels
{
    using Camera.MAUI;
    using ImageFormat = Camera.MAUI.ImageFormat;

    public class CameraDeviceViewModel
    {
        #region Properties

        /// Full path to file where record video will be recorded.
        public string? AutoRecordingFile { get; set; }

        /// If true SnapShot property is refreshed according to the frequency set in the AutoSnapShotSeconds property
        public bool AutoSnapShotAsImageSource { get; set; }

        /// Sets the snaphost image format
        public ImageFormat AutoSnapShotFormat { get; set; }

        /// Sets how often the SnapShot property is updated in seconds.
        /// Default 0: no snapshots are taken
        /// WARNING! A low frequency directly impacts over control performance and memory usage (with AutoSnapShotAsImageSource = true)
        public float AutoSnapShotSeconds { get; set; }

        /// Starts/Stops the Preview if camera property has been set
        public bool AutoStartPreview { get; set; }

        /// Starts/Stops record video to AutoRecordingFile if camera and microphone properties have been set
        public bool AutoStartRecording { get; set; }

        public CameraView? CameraView { get; set; }

        /// Refreshes according to the frequency set in the AutoSnapShotSeconds property (if AutoSnapShotAsImageSource is set to true) or when GetSnapShot is called or TakeAutoSnapShot is set to true
        public ImageSource? SnapShot { get; set; }

        /// Refreshes according to the frequency set in the AutoSnapShotSeconds property or when GetSnapShot is called.
        /// WARNING. Each time a snapshot is made, the previous stream is disposed.
        public Stream? SnapShotStream { get; set; }

        /// Change from false to true refresh SnapShot property
        public bool TakeAutoSnapShot { get; set; }

        #endregion Properties
    }
}