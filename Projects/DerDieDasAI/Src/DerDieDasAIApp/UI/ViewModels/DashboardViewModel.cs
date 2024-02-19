// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.ViewModels
{
    using DerDieDasAIApp.UI.Models;
    using System.ComponentModel;
    using System.Windows.Data;

    public class DashboardViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ProcessItem selectedItem;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Constructors

        public DashboardViewModel()
        {
            Initialize();
        }

        #endregion Constructors

        #region Properties

        public CollectionViewSource ProcessItems { get; } = new CollectionViewSource();

        public ProcessItem SelectedItem
        {
            get => selectedItem;
            set => this.selectedItem = value;
        }

        #endregion Properties

        #region Methods

        private void Initialize()
        {
            var items = new List<ProcessItem>();
            ProcessItems.Source = items;
        }

        #endregion Methods
    }
}