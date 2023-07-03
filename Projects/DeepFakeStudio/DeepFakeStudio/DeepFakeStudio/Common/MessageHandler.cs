namespace DeepFakeStudio.Common
{
    using System;

    /// <summary>
    /// Defines the <see cref="MessageHandler" />.
    /// </summary>
    internal class MessageHandler : NotifyPropertyChanged
    {
        #region Fields

        private Action<string> _sendMessageAction;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the SendMessageAction.
        /// </summary>
        public Action<string> SendMessageAction
        {
            get { return _sendMessageAction; }
            set
            {
                if (_sendMessageAction != value)
                {
                    _sendMessageAction = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsLastErrorSent.
        /// </summary>
        private bool IsLastErrorSent { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The WriteLine.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        public void WriteLine(string message)
        {
            SendMessage($"{message}\r\n");
        }

        /// <summary>
        /// The ApplicationTitle.
        /// </summary>
        internal void ApplicationHeader()
        {
            Separator();
            WriteLine($"{AppEnvironment.LongName} - {AppEnvironment.Version}");
            WriteLine($"Author: {AppEnvironment.Author}");
            WriteLine($"Website: {AppEnvironment.HomeUrl}");
            WriteLine($"Workspace Folder: {AppEnvironment.WorkspaceFolder}");
            Date();
            Space();
            Separator();
            Space();
        }

        /// <summary>
        /// The Date.
        /// </summary>
        internal void Date()
        {
            SendMessage(DateTime.Now.ToString());
        }

        /// <summary>
        /// The SendErrorAction.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        internal void SendError(string message)
        {
            if (!IsLastErrorSent)
            {
                Space();
                this.SendMessageAction?.Invoke("Error:\r\n");
                IsLastErrorSent = true;
            }

            this.SendMessageAction?.Invoke($"- {message}\r\n");
        }

        /// <summary>
        /// The SendMessage.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        internal void SendMessage(string message)
        {
            if (IsLastErrorSent)
            {
                Space();
                IsLastErrorSent = false;
            }

            this.SendMessageAction?.Invoke($"● {message}");
        }

        /// <summary>
        /// The Separator.
        /// </summary>
        internal void Separator()
        {
            this.SendMessageAction?.Invoke("==========================================\r\n");
        }

        /// <summary>
        /// The Space.
        /// </summary>
        internal void Space()
        {
            this.SendMessageAction?.Invoke("\r\n");
        }

        #endregion Methods
    }
}