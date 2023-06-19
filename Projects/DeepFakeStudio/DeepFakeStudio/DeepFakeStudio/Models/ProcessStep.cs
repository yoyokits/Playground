#nullable disable

namespace DeepFakeStudio.Models
{
    using System.Diagnostics;
    using System.Windows.Input;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Defines the <see cref="ProcessStep" />.
    /// </summary>
    public class ProcessStep : NotifyPropertyChanged
    {
        #region Fields

        private bool _isExecuted;

        private ProcessState processState;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessStep"/> class.
        /// </summary>
        /// <param name="other">The other<see cref="ProcessStep"/>.</param>
        public ProcessStep(ProcessStep other) : this(other.Name, other.Description, other.ProcessCommand)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessStep"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/>.</param>
        /// <param name="description">The description<see cref="string"/>.</param>
        /// <param name="processCommand">The processCommand<see cref="string"/>.</param>
        public ProcessStep(string name, string description, string processCommand)
        {
            this.Name = name;
            this.Description = description;
            this.ProcessCommand = processCommand;
            this.ExecuteCommand = new RelayCommand(this.OnExecute, nameof(this.ExecuteCommand), _ => !this.IsExecuted);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the Description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the ExecuteCommand.
        /// </summary>
        public ICommand ExecuteCommand { get; }

        /// <summary>
        /// Gets or sets a value indicating whether IsExecuted.
        /// </summary>
        public bool IsExecuted
        {
            get => _isExecuted;
            set
            {
                if (_isExecuted != value)
                {
                    return;
                }

                _isExecuted = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the Name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the ProcessCommand.
        /// </summary>
        public string ProcessCommand { get; }

        /// <summary>
        /// Gets or sets the ProcessState.
        /// </summary>
        public ProcessState ProcessState
        {
            get { return processState; }
            set
            {
                if (processState != value)
                {
                    processState = value;
                    OnPropertyChanged(nameof(ProcessState));
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnCommand.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnExecute(object obj)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C {this.ProcessCommand}";
            process.StartInfo = startInfo;
            process.Start();
        }

        #endregion Methods
    }
}