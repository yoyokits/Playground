namespace XamarinFormsXamlCameraTest.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// Defines the <see cref="CameraViewModel" />.
    /// </summary>
    public class CameraViewModel : BaseViewModel
    {
        #region Fields

        private ICommand _captureCommand;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the CaptureCommand.
        /// </summary>
        public ICommand CaptureCommand { get => _captureCommand; set => this.Set(ref _captureCommand, value); }

        #endregion Properties
    }
}