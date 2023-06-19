namespace DeepFakeStudio.Extensions
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines the <see cref="NotifyPropertyChangedExtension" />.
    /// </summary>
    public static class NotifyPropertyChangedExtension
    {
        #region Methods

        /// <summary>
        /// The NotifyPropertyChanged.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="sender">The sender<see cref="INotifyPropertyChanged"/>.</param>
        /// <param name="handler">The handler<see cref="PropertyChangedEventHandler"/>.</param>
        /// <param name="oldValue">The oldValue<see cref="T"/>.</param>
        /// <param name="newValue">The newValue<see cref="T"/>.</param>
        /// <param name="propertyName">The propertyName<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool NotifyPropertyChanged<T>(this INotifyPropertyChanged sender, PropertyChangedEventHandler handler, ref T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            var equals = object.Equals(oldValue, newValue);
            if (equals)
            {
                return false;
            }

            oldValue = newValue;
            handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        #endregion Methods
    }
}