// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Processes
{
    using System.ComponentModel;

    public abstract class Process : INotifyPropertyChanged
    {
        #region Properties

        public string Name { get; }

        #endregion Properties

        #region Methods

        public abstract void Execute();

        #endregion Methods

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events
    }
}