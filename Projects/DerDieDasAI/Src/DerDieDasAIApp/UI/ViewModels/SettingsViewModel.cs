// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.ViewModels
{
    using CommunityToolkit.Mvvm.Input;
    using DerDieDasAICore.Extensions;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;

    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string _chatGPTKey;

        private string _rootDirectory = @"C:\";

        #endregion Fields

        #region Constructors

        internal SettingsViewModel()
        {
            LoadedCommand = new RelayCommand<object>(o => OnLoaded(o));
            OpenRootDirectoryCommand = new RelayCommand(OnOpenRootDirectory);
            SelectFolderCommand = new RelayCommand(OnSelectFolder);
            ChatGPTKey = Settings.ChatGPTKey;
            RootDirectory = Settings.RootDirectory;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public string ChatGPTKey { get => _chatGPTKey; set => this.Set(PropertyChanged, ref _chatGPTKey, value); }

        public ICommand LoadedCommand { get; }

        public ICommand OpenRootDirectoryCommand { get; }

        public string RootDirectory
        {
            get => _rootDirectory;
            set => this.Set(PropertyChanged, ref _rootDirectory, value);
        }

        public ICommand SelectFolderCommand { get; }

        public DerDieDasAICore.Properties.Settings Settings => DerDieDasAICore.Properties.Settings.Default;

        private Window Window { get; set; }

        #endregion Properties

        #region Methods

        internal void Save()
        {
            Settings.ChatGPTKey = ChatGPTKey;
            Settings.RootDirectory = RootDirectory;
            Settings.Save();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            Window.Closing -= OnClosing;
            this.Save();
        }

        private void OnLoaded(object obj)
        {
            if (this.Window != null)
            {
                return;
            }

            this.Window = Window.GetWindow(obj as UIElement);
            Window.Closing += OnClosing;
        }

        private void OnOpenRootDirectory()
        {
            var psi = new ProcessStartInfo()
            {
                FileName = this.RootDirectory,
                UseShellExecute = true,
                Verb = "Select Root Directory",
            };
            Process.Start(psi);
        }

        private void OnSelectFolder()
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select Root Directory",
                IsFolderPicker = true,
                InitialDirectory = Settings.RootDirectory,
                AddToMostRecentlyUsedList = true,
                AllowNonFileSystemItems = false,
                DefaultDirectory = Settings.RootDirectory,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RootDirectory = dlg.FileName;
                Settings.RootDirectory = RootDirectory;
            }
        }

        #endregion Methods
    }
}