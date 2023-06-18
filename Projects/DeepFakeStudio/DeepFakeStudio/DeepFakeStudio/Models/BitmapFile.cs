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

        private string _path;

        private BitmapImage _thumbnailBitmap;

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
        /// Gets the Bitmap
        /// Gets or sets the Bitmap.
        /// </summary>
        public BitmapImage Bitmap => BitmapHelper.CreateBitmapImage(this.Path);

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

        /// <summary>
        /// Gets or sets the ThumbnailBitmap.
        /// </summary>
        public BitmapImage ThumbnailBitmap
        {
            get { return _thumbnailBitmap; }
            set
            {
                if (_thumbnailBitmap != value)
                {
                    _thumbnailBitmap = value;
                    OnPropertyChanged(nameof(ThumbnailBitmap));
                }
            }
        }

        /// <summary>
        /// Gets or sets the ThumbnailHeight.
        /// </summary>
        public int ThumbnailHeight { get; set; } = 200;

        /// <summary>
        /// Gets or sets the ThumbnailWidth.
        /// </summary>
        public int ThumbnailWidth { get; set; } = 200;

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

            this.ThumbnailBitmap = BitmapHelper.CreateBitmapImage(this.Path, this.ThumbnailHeight);
        }

        #endregion Methods
    }
}