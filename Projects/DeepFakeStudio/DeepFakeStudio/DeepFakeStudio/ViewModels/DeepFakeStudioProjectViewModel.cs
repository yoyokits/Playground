#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System.Collections.Generic;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Core;
    using DeepFakeStudio.Models;

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
    }
}