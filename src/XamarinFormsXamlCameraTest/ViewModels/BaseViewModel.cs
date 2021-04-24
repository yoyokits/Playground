namespace XamarinFormsXamlCameraTest.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines the <see cref="BaseViewModel" />.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Fields

        private bool _isBusy = false;

        private string _title = string.Empty;

        #endregion Fields

        #region Events

        /// <summary>
        /// Defines the PropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether IsBusy.
        /// </summary>
        public bool IsBusy { get => _isBusy; set => Set(ref _isBusy, value); }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnPropertyChanged.
        /// </summary>
        /// <param name="propertyName">The propertyName<see cref="string"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The SetProperty.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="backingStore">The backingStore<see cref="T"/>.</param>
        /// <param name="value">The value<see cref="T"/>.</param>
        /// <param name="propertyName">The propertyName<see cref="string"/>.</param>
        /// <param name="onChanged">The onChanged<see cref="Action"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected bool Set<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion Methods
    }
}