#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using DeepFakeStudio.Common;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioPreviewViewModel" />.
    /// </summary>
    public class DeepFakeStudioPreviewViewModel : NotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepFakeStudioPreviewViewModel"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/>.</param>
        public DeepFakeStudioPreviewViewModel(string path)
        {
            Path = path;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        public string Path { get; set; }

        #endregion Properties
    }
}