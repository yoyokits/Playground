namespace DeepFakeStudio.Models
{
    using System.IO;
    using System.Windows.Media.Imaging;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Helpers;

    /// <summary>
    /// Defines the <see cref="BitmapFile" />.
    /// </summary>
    public class BitmapFile : NotifyPropertyChanged
    {
        #region Fields

        private WriteableBitmap _bitmap;

        private string _path;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapFile"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/>.</param>
        public BitmapFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Logger.Warn($"Images File ({this.Path}) doesn't exist", "Images File Not Found");
                return;
            }

            this.IsValid = true;
            this.Path = path;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Bitmap.
        /// </summary>
        public WriteableBitmap Bitmap
        {
            get { return _bitmap; }
            internal set
            {
                if (_bitmap != value)
                {
                    _bitmap = value;
                    OnPropertyChanged(nameof(Bitmap));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsValid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        public string Path
        {
            get { return _path; }
            internal set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The Load.
        /// </summary>
        internal void Load()
        {
            if (!this.IsValid)
            {
                Logger.Warn($"Images File ({this.Path}) doesn't exist", "Images File Not Found");
                return;
            }

            this.Bitmap = BitmapHelper.LoadBitmap(this.Path);
        }

        #endregion Methods
    }
}