namespace XamarinFormsXamlCameraTest.Droid
{
    using Android.Hardware.Camera2;
    using System;

    /// <summary>
    /// Defines the <see cref="CameraStateListener" />.
    /// </summary>
    public class CameraStateListener : CameraDevice.StateCallback
    {
        #region Fields

        public Action<CameraDevice> OnClosedAction;

        public Action<CameraDevice> OnDisconnectedAction;

        public Action<CameraDevice, CameraError> OnErrorAction;

        public Action<CameraDevice> OnOpenedAction;

        #endregion Fields

        #region Methods

        /// <summary>
        /// The OnClosed.
        /// </summary>
        /// <param name="camera">The camera<see cref="CameraDevice"/>.</param>
        public override void OnClosed(CameraDevice camera) => OnClosedAction(camera);

        /// <summary>
        /// The OnDisconnected.
        /// </summary>
        /// <param name="camera">The camera<see cref="CameraDevice"/>.</param>
        public override void OnDisconnected(CameraDevice camera) => OnDisconnectedAction?.Invoke(camera);

        /// <summary>
        /// The OnError.
        /// </summary>
        /// <param name="camera">The camera<see cref="CameraDevice"/>.</param>
        /// <param name="error">The error<see cref="CameraError"/>.</param>
        public override void OnError(CameraDevice camera, CameraError error) => OnErrorAction(camera, error);

        /// <summary>
        /// The OnOpened.
        /// </summary>
        /// <param name="camera">The camera<see cref="CameraDevice"/>.</param>
        public override void OnOpened(CameraDevice camera) => OnOpenedAction?.Invoke(camera);

        #endregion Methods
    }
}