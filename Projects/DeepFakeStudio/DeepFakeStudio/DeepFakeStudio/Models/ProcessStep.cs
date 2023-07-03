#nullable disable

namespace DeepFakeStudio.Models
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.ViewModels;

    /// <summary>
    /// Defines the <see cref="ProcessStep" />.
    /// </summary>
    public class ProcessStep : NotifyPropertyChanged
    {
        #region Fields

        private bool _isExecuted;

        private MessageHandler _messageHandler;

        private ProcessState _processState;

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
            this.ProcessController = new ProcessController(this.ProcessCommand);
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
        /// Gets the ProcessController.
        /// </summary>
        public ProcessController ProcessController { get; }

        /// <summary>
        /// Gets or sets the ProcessState.
        /// </summary>
        public ProcessState ProcessState
        {
            get { return _processState; }
            set
            {
                if (_processState != value)
                {
                    _processState = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ProcessStepId.
        /// </summary>
        public ProcessStepId ProcessStepId { get; internal set; }

        /// <summary>
        /// Gets the ProcessStepSettingItems.
        /// </summary>
        public IList<ProcessStepSettingItem> ProcessStepSettingItems { get; } = new List<ProcessStepSettingItem>();

        /// <summary>
        /// Gets or sets the MessageHandler.
        /// </summary>
        internal MessageHandler MessageHandler
        {
            get { return _messageHandler; }
            set
            {
                if (_messageHandler != value)
                {
                    _messageHandler = value;
                    ProcessController.MessageHandler = MessageHandler;
                    OnPropertyChanged(nameof(MessageHandler));
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            var message = $"{Name}:Command:{ProcessCommand}";
            return message;
        }

        /// <summary>
        /// The OnCommand.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnExecute(object obj)
        {
            this.ProcessController.Execute();
        }

        #endregion Methods
    }
}