// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.ViewModels
{
    using DerDieDasAIApp.UI.Models;
    using DerDieDasAICore.Extensions;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Data;

    public class ProcessViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ProcessItem _selectedProcess;

        #endregion Fields

        #region Constructors

        internal ProcessViewModel()
        {
            var items = new List<ProcessItem>();
            ProcessItems.Source = items;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public CollectionViewSource ProcessItems { get; } = new CollectionViewSource();

        public ProcessItem SelectedProcess
        {
            get => _selectedProcess;
            set => this.Set(PropertyChanged, ref _selectedProcess, value);
        }

        #endregion Properties
    }
}