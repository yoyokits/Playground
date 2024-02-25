// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.ViewModels
{
    using System.ComponentModel;

    public class DashboardViewModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public DataBaseViewModel DataBase { get; } = new();

        public ProcessViewModel Process { get; } = new();

        public SettingsViewModel Settings { get; } = new();

        #endregion Properties
    }
}