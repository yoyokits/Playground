#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System;
    using System.Windows.Input;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Extensions;
    using DeepFakeStudio.Helpers;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioNewProjectViewModel" />.
    /// </summary>
    public class DeepFakeStudioNewProjectViewModel : NotifyPropertyChanged
    {
        #region Fields

        private string _destinationVideoPath;

        private string _sourceVideoPath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepFakeStudioNewProjectViewModel"/> class.
        /// </summary>
        public DeepFakeStudioNewProjectViewModel()
        {
            this.SelectDestinationVideoCommand = new RelayCommand(this.OnSelectDestinationVideo, nameof(this.SelectDestinationVideoCommand));
            this.SelectSourceVideoCommand = new RelayCommand(this.OnSelectSourceVideo, nameof(this.SelectSourceVideoCommand));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the DestinationVideoPath.
        /// </summary>
        public string DestinationVideoPath { get => _destinationVideoPath; set => this.NotifyPropertyChanged(this.PropertyChangedHandler, ref _destinationVideoPath, value); }

        /// <summary>
        /// Gets the SelectDestinationVideoCommand.
        /// </summary>
        public ICommand SelectDestinationVideoCommand { get; }

        /// <summary>
        /// Gets the SelectSourceVideoCommand.
        /// </summary>
        public ICommand SelectSourceVideoCommand { get; }

        /// <summary>
        /// Gets or sets the SourceVideoPath.
        /// </summary>
        public string SourceVideoPath { get => _sourceVideoPath; set => this.NotifyPropertyChanged(this.PropertyChangedHandler, ref this._sourceVideoPath, value); }

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

            this.DestinationVideoPath = path;
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

            this.SourceVideoPath = path;
        }

        #endregion Methods
    }
}