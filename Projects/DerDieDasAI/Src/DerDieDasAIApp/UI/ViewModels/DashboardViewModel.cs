// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.ViewModels
{
    using CommunityToolkit.Mvvm.Input;
    using DerDieDasAIApp.UI.Models;
    using DerDieDasAICore.Extensions;
    using System.ComponentModel;
    using System.Windows.Data;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.Windows.Input;
    using DerDieDasAIApp.Common;

    public class DashboardViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string myRootFolder = @"C:\Temp\";

        private ProcessItem mySelectedProcess;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Constructors

        public DashboardViewModel()
        {
            this.SelectFolderCommand = new RelayCommand(this.OnSelectFolder);
            Initialize();
        }

        #endregion Constructors

        #region Properties

        public CollectionViewSource ProcessItems { get; } = new CollectionViewSource();

        public string RootFolder
        {
            get => myRootFolder;
            set => this.Set(PropertyChanged, ref myRootFolder, value);
        }

        public ProcessItem SelectedProcess
        {
            get => mySelectedProcess;
            set => this.Set(PropertyChanged, ref mySelectedProcess, value);
        }

        public ICommand SelectFolderCommand { get; }

        #endregion Properties

        #region Methods

        private void Initialize()
        {
            var items = new List<ProcessItem>();
            ProcessItems.Source = items;
        }

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
                this.RootFolder = dlg.FileName;
                AppEnvironment.Instance.RootDirectory = this.RootFolder;
            }
        }

        #endregion Methods
    }
}