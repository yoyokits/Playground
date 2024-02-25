// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.ViewModels
{
    using CommunityToolkit.Mvvm.Input;
    using DerDieDasAIApp.Common;
    using DerDieDasAICore.Extensions;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.ComponentModel;
    using System.Windows.Input;

    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string _rootFolder = @"C:\Temp\";

        #endregion Fields

        #region Constructors

        internal SettingsViewModel()
        {
            SelectFolderCommand = new RelayCommand(OnSelectFolder);
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public string RootFolder
        {
            get => _rootFolder;
            set => this.Set(PropertyChanged, ref _rootFolder, value);
        }

        public ICommand SelectFolderCommand { get; }

        #endregion Properties

        #region Methods

        private void OnSelectFolder()
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "My Title",
                IsFolderPicker = true,
                InitialDirectory = AppEnvironment.Instance.RootDirectory,
                AddToMostRecentlyUsedList = true,
                AllowNonFileSystemItems = false,
                DefaultDirectory = AppEnvironment.Instance.RootDirectory,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RootFolder = dlg.FileName;
                AppEnvironment.Instance.RootDirectory = RootFolder;
            }
        }

        #endregion Methods
    }
}