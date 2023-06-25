namespace DeepFakeStudio.Common
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Controls;

    /// <summary>
    /// Defines the <see cref="ConsoleWriter" />.
    /// </summary>
    public class ConsoleWriter : TextWriter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWriter"/> class.
        /// </summary>
        /// <param name="textBox">The output<see cref="TextBox"/>.</param>
        public ConsoleWriter(TextBox textBox)
        {
            TextBox = textBox;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the Encoding.
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Gets the TextBox.
        /// </summary>
        private TextBox TextBox { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The Clear.
        /// </summary>
        public void Clear()
        {
            TextBox.Dispatcher.BeginInvoke((Action)(() => TextBox.Text = string.Empty));
        }

        /// <summary>
        /// The Write.
        /// </summary>
        /// <param name="value">The value<see cref="string"/>.</param>
        public override void Write(string value)
        {
            base.Write(value);
            TextBox.Dispatcher.BeginInvoke((Action)(() => TextBox.AppendText(value)));
        }

        #endregion Methods
    }
}