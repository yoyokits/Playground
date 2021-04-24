namespace XamarinFormsXamlCameraTest.Droid
{
    using Android.Hardware.Camera2;
    using System;

    /// <summary>
    /// Defines the <see cref="CameraCaptureStateListener" />.
    /// </summary>
    public class CameraCaptureStateListener : CameraCaptureSession.StateCallback
    {
        #region Fields

        public Action<CameraCaptureSession> OnConfiguredAction;

        public Action<CameraCaptureSession> OnConfigureFailedAction;

        #endregion Fields

        #region Methods

        /// <summary>
        /// The OnConfigured.
        /// </summary>
        /// <param name="session">The session<see cref="CameraCaptureSession"/>.</param>
        public override void OnConfigured(CameraCaptureSession session) => OnConfiguredAction?.Invoke(session);

        /// <summary>
        /// The OnConfigureFailed.
        /// </summary>
        /// <param name="session">The session<see cref="CameraCaptureSession"/>.</param>
        public override void OnConfigureFailed(CameraCaptureSession session) => OnConfigureFailedAction?.Invoke(session);

        #endregion Methods
    }
}