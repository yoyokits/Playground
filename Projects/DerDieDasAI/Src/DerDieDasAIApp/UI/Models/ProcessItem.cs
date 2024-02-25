// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.Models
{
    using System.ComponentModel;

    public abstract class ProcessItem : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public string Name { get; set; }

        public PropertyChangedEventHandler PropertyChangedHandler => this.PropertyChanged;

        #endregion Properties

        #region Methods

        internal abstract void Execute();

        #endregion Methods
    }
}