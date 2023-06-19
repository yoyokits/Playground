namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioNewProjectViewTest" />.
    /// </summary>
    [TestClass]
    public class DeepFakeStudioNewProjectViewTest
    {
        #region Methods

        /// <summary>
        /// Show DeepFakeStudioNewProjectView.
        /// </summary>
        [TestMethod]
        public void DeepFakeStudioNewProjectView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new DeepFakeStudioNewProjectViewModel();
                var view = new DeepFakeStudioNewProjectView { DataContext = viewModel };
                view.ShowDialog(nameof(DeepFakeStudioNewProjectView));
            });
        }

        #endregion Methods
    }
}