﻿#nullable disable

namespace DeepFakeStudio.ViewModels
{
    using System.Collections.ObjectModel;
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
            this.NewProjectCommand = new RelayCommand(this.OnNewProject, nameof(this.NewProjectCommand));
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the DeepFakeStudioProjects.
        /// </summary>
        public ObservableCollection<DeepFakeStudioProjectViewModel> DeepFakeStudioProjects { get; } = new();

        /// <summary>
        /// Gets the NewProjectCommand.
        /// </summary>
        public ICommand NewProjectCommand { get; }

        #endregion Properties

        #region Methods

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