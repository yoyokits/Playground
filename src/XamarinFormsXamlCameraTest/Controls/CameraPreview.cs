namespace XamarinFormsXamlCameraTest.Controls
{
    using Xamarin.Forms;

    /// <summary>
    /// Defines the <see cref="CameraPreview" />.
    /// </summary>
    public class CameraPreview : View
    {
        #region Fields

        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
            propertyName: nameof(Camera),
            returnType: typeof(CameraOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: CameraOptions.Rear);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the Camera.
        /// </summary>
        public CameraOptions Camera { get => (CameraOptions)GetValue(CameraProperty); set => SetValue(CameraProperty, value); }

        #endregion Properties
    }
}