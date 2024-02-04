// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Common
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines the <see cref="NotifyPropertyChanged" />
    /// Standard INotifyPropertyChanged implementation.
    /// </summary>
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Defines the PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// The property changed handler
        /// </summary>
        protected PropertyChangedEventHandler PropertyChangedHandler => this.PropertyChanged;

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnPropertyChanged
        /// </summary>
        /// <param name="propertyName">The <see cref="string"/></param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}