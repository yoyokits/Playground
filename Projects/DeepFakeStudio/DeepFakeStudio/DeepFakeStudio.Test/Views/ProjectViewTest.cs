namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="ProjectViewTest" />.
    /// </summary>
    [TestClass]
    public class ProjectViewTest
    {
        #region Methods

        /// <summary>
        /// Show ProjectView.
        /// </summary>
        [TestMethod]
        public void ShowDeepFakeStudioProjectView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new ProjectViewModel();
                var view = new ProjectView { DataContext = viewModel };
                view.ShowDialog(nameof(ProjectView));
            });
        }

        #endregion Methods
    }
}