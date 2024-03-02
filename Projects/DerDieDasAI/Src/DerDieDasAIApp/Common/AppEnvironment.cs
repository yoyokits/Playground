// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.Common
{
    using DerDieDasAIApp.Properties;
    using System.ComponentModel;

    internal class AppEnvironment : INotifyPropertyChanged
    {
        #region Fields

        public static readonly AppEnvironment Instance = new();

        #endregion Fields

        #region Constructors

        private AppEnvironment()
        {
            LoadSettings();
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        internal void LoadSettings()
        {
        }

        internal void SaveSettings()
        {
            Settings.Default.Save();
        }

        #endregion Methods
    }
}