#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System.IO;
    using System.Windows.Input;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Extensions;
    using DeepFakeStudio.Helpers;

    /// <summary>
    /// Defines the <see cref="OpenVideosViewModel" />.
    /// </summary>
    public class OpenVideosViewModel : NotifyPropertyChanged
    {
        #region Fields

        private string _videoDestinationPath;

        private string _videoSourcePath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenVideosViewModel"/> class.
        /// </summary>
        public OpenVideosViewModel()
        {
            this.SelectDestinationVideoCommand = new RelayCommand(this.OnSelectDestinationVideo, nameof(this.SelectDestinationVideoCommand));
            this.SelectSourceVideoCommand = new RelayCommand(this.OnSelectSourceVideo, nameof(this.SelectSourceVideoCommand));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether IsValid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                var isSrcValid = !string.IsNullOrEmpty(VideoSourcePath) && File.Exists(VideoSourcePath);
                var isDstValid = !string.IsNullOrEmpty(VideoDestinationPath) && File.Exists(VideoDestinationPath);
                return isSrcValid && isDstValid;
            }
        }

        /// <summary>
        /// Gets the SelectDestinationVideoCommand.
        /// </summary>
        public ICommand SelectDestinationVideoCommand { get; }

        /// <summary>
        /// Gets the SelectSourceVideoCommand.
        /// </summary>
        public ICommand SelectSourceVideoCommand { get; }

        /// <summary>
        /// Gets or sets the VideoDestinationPath.
        /// </summary>
        public string VideoDestinationPath { get => _videoDestinationPath; set => this.NotifyPropertyChanged(this.PropertyChangedHandler, ref _videoDestinationPath, value); }

        /// <summary>
        /// Gets or sets the VideoSourcePath.
        /// </summary>
        public string VideoSourcePath { get => _videoSourcePath; set => this.NotifyPropertyChanged(this.PropertyChangedHandler, ref this._videoSourcePath, value); }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnSelectDestinationVideo.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnSelectDestinationVideo(object obj)
        {
            var path = FileHelper.GetFile();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            this.VideoDestinationPath = path;
        }

        /// <summary>
        /// The OnSelectSourceVideo.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnSelectSourceVideo(object obj)
        {
            var path = FileHelper.GetFile();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            this.VideoSourcePath = path;
        }

        #endregion Methods
    }
}