namespace DeepFakeStudio.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Interaction logic for ConsoleOutput.xaml.
    /// </summary>
    public partial class ConsoleOutput : UserControl
    {
        #region Fields

        // Using a DependencyProperty as the backing store for SendMessageAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SendMessageActionProperty =
            DependencyProperty.Register(nameof(SendMessageAction), typeof(Action<string>), typeof(ConsoleOutput), new PropertyMetadata(null));

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleOutput"/> class.
        /// </summary>
        public ConsoleOutput()
        {
            InitializeComponent();
            ConsoleWriter = new ConsoleWriter(TextBox);
            Loaded += OnConsoleOutput_Loaded;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the SendMessageAction.
        /// </summary>
        public Action<string> SendMessageAction
        {
            get { return (Action<string>)GetValue(SendMessageActionProperty); }
            set { SetValue(SendMessageActionProperty, value); }
        }

        /// <summary>
        /// Gets the ConsoleWriter.
        /// </summary>
        private ConsoleWriter ConsoleWriter { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnClearButton_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnClearButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleWriter.Clear();
        }

        /// <summary>
        /// The OnConsoleOutput_Loaded.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void OnConsoleOutput_Loaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction = OnSendMessageAction;
        }

        /// <summary>
        /// The OnSendMessageAction.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        private void OnSendMessageAction(string message)
        {
            ConsoleWriter.Write(message);
        }

        #endregion Methods
    }
}