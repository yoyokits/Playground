namespace DeepFakeStudio.ViewModels
{
    using System.Collections.ObjectModel;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Defines the <see cref="MessageViewModel" />.
    /// </summary>
    public class MessageViewModel : NotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// Gets the Messages.
        /// </summary>
        public ObservableCollection<string> Messages { get; } = new();

        #endregion Properties

        #region Methods

        /// <summary>
        /// The SendError.
        /// </summary>
        /// <param name="error">The error<see cref="string"/>.</param>
        internal void SendError(string error)
        {
            this.Messages.Add($"Error:{error}");
        }

        /// <summary>
        /// The SendMessage.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        internal void SendMessage(string message)
        {
            this.Messages.Add(message);
        }

        #endregion Methods
    }
}