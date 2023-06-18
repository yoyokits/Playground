#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Models;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioPreviewViewModel" />.
    /// </summary>
    public class DeepFakeStudioPreviewViewModel : NotifyPropertyChanged
    {
        #region Fields

        private IList<BitmapFile> _bitmapFiles;

        private string _path;

        private BitmapFile _selectedBitmapFile;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepFakeStudioPreviewViewModel"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/>.</param>
        public DeepFakeStudioPreviewViewModel(string path)
        {
            Path = path;
            this.PropertyChanged += OnPropertyChanged;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the BitmapFiles.
        /// </summary>
        public IList<BitmapFile> BitmapFiles
        {
            get { return _bitmapFiles; }
            private set
            {
                if (_bitmapFiles != value)
                {
                    _bitmapFiles = value;
                    OnPropertyChanged(nameof(BitmapFiles));
                }
            }
        }

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        public string Path
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedBitmapFile.
        /// </summary>
        public BitmapFile SelectedBitmapFile
        {
            get => _selectedBitmapFile; set
            {
                if (_selectedBitmapFile == value)
                {
                    return;
                }

                _selectedBitmapFile = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ReloadBitmapCancellationTokenSource.
        /// </summary>
        private CancellationTokenSource ReloadBitmapCancellationTokenSource { get; set; }

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
                case nameof(this.Path):
                    this.ReloadImages();
                    break;
            }
        }

        /// <summary>
        /// The ReloadImages.
        /// </summary>
        private void ReloadImages()
        {
            if (string.IsNullOrEmpty(this.Path) || Directory.Exists(this.Path))
            {
                MessageBox.Show("Folder doesn't exist or invalid Folder", "Error Loading Images");
                return;
            }

            var files = Directory.GetFiles(this.Path, "*.jpg");
            if (files == null || !files.Any())
            {
                MessageBox.Show($"Images Not Found in the {this.Path}", "Images Not Found");
                return;
            }

            this.ReloadBitmapCancellationTokenSource?.Cancel();
            this.ReloadBitmapCancellationTokenSource = new CancellationTokenSource();
            var token = this.ReloadBitmapCancellationTokenSource.Token;

            Task.Run(() =>
            {
                var bitmapFiles = new List<BitmapFile>(files.Length);
                foreach (var file in files)
                {
                    var bitmapFile = new BitmapFile(file);
                    if (bitmapFile.IsValid)
                    {
                        bitmapFiles.Add(bitmapFile);
                    }
                }

                this.BitmapFiles = bitmapFiles;
                if (this.BitmapFiles.Any())
                {
                    this.SelectedBitmapFile = this.BitmapFiles[0];
                }
            }, token);
        }

        #endregion Methods
    }
}