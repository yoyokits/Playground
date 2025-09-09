// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.ComponentModel;
using System.Windows;
using LLM.Settings;

namespace LLM.Settings
{
    public partial class SettingsDialog : Window, INotifyPropertyChanged
    {
        #region Fields

        private AppSettings _working;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Events

        #region Constructors

        public SettingsDialog()
        {
            InitializeComponent();
            _working = SettingsService.Clone();
            DataContext = this;
        }

        #endregion Constructors

        #region Properties

        public AppSettings Working
        {
            get => _working;
            private set
            {
                _working = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Working)));
            }
        }

        #endregion Properties

        #region Methods

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await SettingsService.UpdateAsync(Working);
            DialogResult = true;
            Close();
        }

        #endregion Methods
    }
}