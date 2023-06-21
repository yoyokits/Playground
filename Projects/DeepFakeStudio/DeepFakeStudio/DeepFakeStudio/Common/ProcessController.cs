namespace DeepFakeStudio.Common
{
    using System;
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
        /// Gets the ProcessCommand.
        /// </summary>
        public string ProcessCommand { get; }

        /// <summary>
        /// Gets or sets the SendErrorAction.
        /// </summary>
        public Action<string> SendErrorAction { get; set; }

        /// <summary>
        /// Gets or sets the SendMessageAction.
        /// </summary>
        public Action<string> SendMessageAction { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The Execute.
        /// </summary>
        public void Execute()
        {
            //* Create your Process
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/C {this.ProcessCommand}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            //* Set your output and error (asynchronous) handlers
            process.OutputDataReceived += OnProcess_OutputDataReceived;
            process.ErrorDataReceived += OnProcess_ErrorDataReceived;

            //* Start process and handlers
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        /// <summary>
        /// The OnProcess_ErrorDataReceived.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="DataReceivedEventArgs"/>.</param>
        private void OnProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.SendErrorAction?.Invoke(e.Data);
        }

        /// <summary>
        /// The OnProcess_OutputDataReceived.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="DataReceivedEventArgs"/>.</param>
        private void OnProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.SendMessageAction?.Invoke(e.Data);
        }

        #endregion Methods
    }
}