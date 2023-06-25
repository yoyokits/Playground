#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System;
    using System.Windows.Input;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioViewModel" />.
    /// </summary>
    public class DeepFakeStudioViewModel : NotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepFakeStudioViewModel"/> class.
        /// </summary>
        public DeepFakeStudioViewModel()
        {
            this.DeepFakeStudioProject = new() { MessageHandler = MessageHandler };
            this.LoadedCommand = new RelayCommand(this.OnLoaded, nameof(this.LoadedCommand));
            this.NewProjectCommand = new RelayCommand(this.OnNewProject, nameof(this.NewProjectCommand));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the DeepFakeStudioPreview.
        /// </summary>
        public PreviewViewModel DeepFakeStudioPreview { get; } = new();

        /// <summary>
        /// Gets the DeepFakeStudioProject.
        /// </summary>
        public ProjectViewModel DeepFakeStudioProject { get; }

        /// <summary>
        /// Gets the LoadedCommand.
        /// </summary>
        public ICommand LoadedCommand { get; }

        /// <summary>
        /// Gets the NewProjectCommand.
        /// </summary>
        public ICommand NewProjectCommand { get; }

        /// <summary>
        /// Gets or sets the SendMessageAction.
        /// </summary>
        public Action<string> SendMessageAction
        {
            get { return MessageHandler.SendMessageAction; }
            set
            {
                if (SendMessageAction != value)
                {
                    MessageHandler.SendMessageAction = value;
                    if (SendMessageAction != null)
                    {
                        MessageHandler.ApplicationHeader();
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the MessageHandler.
        /// </summary>
        internal MessageHandler MessageHandler { get; } = new();

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnLoaded.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnLoaded(object obj)
        {
        }

        /// <summary>
        /// The OnNewProject.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnNewProject(object obj)
        {
        }

        #endregion Methods
    }
}