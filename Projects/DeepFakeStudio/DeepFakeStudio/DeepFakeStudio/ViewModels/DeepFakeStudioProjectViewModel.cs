#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Core;
    using DeepFakeStudio.Models;
    using DeepFakeStudio.Views;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioProjectViewModel" />.
    /// </summary>
    public class DeepFakeStudioProjectViewModel : NotifyPropertyChanged
    {
        #region Fields

        private string _name = "No name";

        private string _videoDestinationPath;

        private string _videoSourcePath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepFakeStudioProjectViewModel"/> class.
        /// </summary>
        public DeepFakeStudioProjectViewModel()
        {
            OpenVideoCommand = new RelayCommand(OnOpenVideo, nameof(OpenVideoCommand));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name
        {
            get => _name; set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the OpenVideoCommand.
        /// </summary>
        public ICommand OpenVideoCommand { get; }

        /// <summary>
        /// Gets the ProcessSteps.
        /// </summary>
        public IList<ProcessStep> ProcessSteps { get; } = ProcessStepFactory.CreateProcessSteps();

        /// <summary>
        /// Gets or sets the VideoDestinationPath.
        /// </summary>
        public string VideoDestinationPath
        {
            get => _videoDestinationPath;
            set
            {
                if (_videoDestinationPath == value)
                {
                    return;
                }

                _videoDestinationPath = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the VideoSourcePath.
        /// </summary>
        public string VideoSourcePath
        {
            get => _videoSourcePath; set
            {
                if (_videoSourcePath == value)
                {
                    return;
                }

                _videoSourcePath = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnOpenVideo.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnOpenVideo(object obj)
        {
            var viewModel = new OpenVideosViewModel();
            var view = new OpenVideosView { DataContext = viewModel };
            WindowFactory.ShowDialog(view, "Open Video to Edit", 1200, 600);
            if (!viewModel.IsValid)
            {
                return;
            }

            this.VideoSourcePath = viewModel.VideoSourcePath;
            this.VideoDestinationPath = viewModel.VideoDestinationPath;
        }

        #endregion Methods
    }
}