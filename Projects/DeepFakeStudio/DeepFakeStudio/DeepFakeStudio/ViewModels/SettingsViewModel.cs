namespace DeepFakeStudio.ViewModels
{
    using System.IO;
    using System.Windows.Input;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Core;
    using DeepFakeStudio.Helpers;

    /// <summary>
    /// Defines the <see cref="SettingsViewModel" />.
    /// </summary>
    public class SettingsViewModel : NotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        internal SettingsViewModel()
        {
            SelectFolderCommand = new RelayCommand(OnSelectFolder, nameof(SelectFolderCommand));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the AppSettings.
        /// </summary>
        public AppSettings AppSettings { get; set; }

        /// <summary>
        /// Gets the SelectFolderCommand.
        /// </summary>
        public ICommand SelectFolderCommand { get; }

        /// <summary>
        /// Gets or sets the WorkspaceFolder.
        /// </summary>
        public string WorkspaceFolder
        {
            get { return AppSettings.WorkspaceFolder; }
            set
            {
                if (WorkspaceFolder != value)
                {
                    AppSettings.WorkspaceFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnSelectFolder.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnSelectFolder(object obj)
        {
            var folder = FileHelper.GetFolder(this.WorkspaceFolder);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            WorkspaceFolder = folder;
        }

        #endregion Methods
    }
}