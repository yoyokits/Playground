#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using DeepFakeStudio.Common;
    using DeepFakeStudio.Core;

    /// <summary>
    /// Defines the <see cref="MainViewModel" />.
    /// </summary>
    public class MainViewModel : NotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            this.DeepFakeStudioProject = new() { MessageHandler = MessageHandler };
            this.LoadedCommand = new RelayCommand(this.OnLoaded, nameof(this.LoadedCommand));
            this.NewProjectCommand = new RelayCommand(this.OnNewProject, nameof(this.NewProjectCommand));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the AppSettingsController.
        /// </summary>
        public AppSettingsController AppSettingsController { get; } = new();

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
        /// Gets the PreviewViewModel.
        /// </summary>
        public PreviewViewModel PreviewViewModel { get; } = new();

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

        /// <summary>
        /// Gets or sets the Window.
        /// </summary>
        private Window Window { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The OnLoaded.
        /// </summary>
        /// <param name="view">The obj<see cref="object"/>.</param>
        private void OnLoaded(object view)
        {
            var tuple = ((object, object, object))view;
            var mainView = tuple.Item1 as FrameworkElement;
            this.Window = Window.GetWindow(mainView);
            this.Window.Closing += OnWindow_Closing;

            AppSettingsController.Load();
            var settings = AppSettingsController.AppSettings;
            this.Window.Width = settings.WindowWidth;
            this.Window.Height = settings.WindowHeight;
            this.Window.WindowState = settings.WindowState;
        }

        /// <summary>
        /// The OnNewProject.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        private void OnNewProject(object obj)
        {
        }

        /// <summary>
        /// The OnWindow_Closing.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.ComponentModel.CancelEventArgs"/>.</param>
        private void OnWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Window.Closing -= OnWindow_Closing;
            var settings = AppSettingsController.AppSettings;
            settings.WindowWidth = this.Window.ActualWidth;
            settings.WindowHeight = this.Window.ActualHeight;
            settings.WindowState = this.Window.WindowState;
            AppSettingsController.Save();
        }

        #endregion Methods
    }
}