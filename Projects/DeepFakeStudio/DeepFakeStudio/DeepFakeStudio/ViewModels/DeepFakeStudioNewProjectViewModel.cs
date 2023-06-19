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

        private Uri _destinationVideoUri;

        private string _sourceVideoPath;

        private Uri _sourceVideoUri;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepFakeStudioNewProjectViewModel"/> class.
        /// </summary>
        public DeepFakeStudioNewProjectViewModel()
        {
            this.SelectDestinationVideoCommand = new RelayCommand(this.OnSelectDestinationVideo, nameof(this.SelectDestinationVideoCommand));
            this.SelectSourceVideoCommand = new RelayCommand(this.OnSelectSourceVideo, nameof(this.SelectSourceVideoCommand));
            this.PropertyChanged += this.OnPropertyChanged;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the DestinationVideoPath.
        /// </summary>
        public string DestinationVideoPath { get => _destinationVideoPath; set => this.NotifyPropertyChanged(this.PropertyChangedHandler, ref _destinationVideoPath, value); }

        /// <summary>
        /// Gets or sets the DestinationVideoUri.
        /// </summary>
        public Uri DestinationVideoUri { get => _destinationVideoUri; set => this.NotifyPropertyChanged(this.PropertyChangedHandler, ref _destinationVideoUri, value); }

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

        /// <summary>
        /// Gets or sets the SourceVideoUri.
        /// </summary>
        public Uri SourceVideoUri { get => _sourceVideoUri; set => this.NotifyPropertyChanged(this.PropertyChangedHandler, ref _sourceVideoUri, value); }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnPropertyChanged.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.ComponentModel.PropertyChangedEventArgs"/>.</param>
        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.SourceVideoPath):
                    this.SourceVideoUri = new Uri(this.SourceVideoPath, UriKind.Absolute);
                    break;

                case nameof(this.DestinationVideoPath):
                    this.DestinationVideoUri = new Uri(this.DestinationVideoPath, UriKind.Absolute);
                    break;
            }
        }

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