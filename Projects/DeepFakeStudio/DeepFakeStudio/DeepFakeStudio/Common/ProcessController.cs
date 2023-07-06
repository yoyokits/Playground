namespace DeepFakeStudio.Common
{
    using System.Diagnostics;

    /// <summary>
    /// Defines the <see cref="ProcessController" />.
    /// </summary>
    public class ProcessController
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessController"/> class.
        /// </summary>
        /// <param name="command">The command<see cref="string"/>.</param>
        public ProcessController(string command)
        {
            this.ProcessCommand = command;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the LastError.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Gets the LastMessage.
        /// </summary>
        public string LastMessage { get; private set; }

        /// <summary>
        /// Gets the ProcessCommand.
        /// </summary>
        public string ProcessCommand { get; }

        /// <summary>
        /// Gets or sets the MessageHandler.
        /// </summary>
        internal MessageHandler MessageHandler { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The Execute.
        /// </summary>
        public async void ExecuteAsync()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\system32\cmd.exe",
                Arguments = $"/C {this.ProcessCommand}",

                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = false,
                UseShellExecute = true,
                RedirectStandardOutput = false
            };

            //////* Set your output and error (asynchronous) handlers
            ////pprocessStartInfo.OutputDataReceived += OnProcess_OutputDataReceived;
            ////pprocessStartInfo.ErrorDataReceived += OnProcess_ErrorDataReceived;

            //////* Start process and handlers
            ////process.Start();
            ////process.BeginOutputReadLine();
            ////process.BeginErrorReadLine();

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();
            await process.WaitForExitAsync();
            process.CloseMainWindow();
        }

        /// <summary>
        /// The OnProcess_ErrorDataReceived.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="DataReceivedEventArgs"/>.</param>
        private void OnProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            this.LastError = e.Data;
            this.MessageHandler?.SendError(this.LastError);
        }

        /// <summary>
        /// The OnProcess_OutputDataReceived.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="DataReceivedEventArgs"/>.</param>
        private void OnProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            this.LastMessage = e.Data;
            this.MessageHandler?.SendMessage(this.LastMessage);
        }

        #endregion Methods
    }
}