namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioProjectViewTest" />.
    /// </summary>
    [TestClass]
    public class DeepFakeStudioProjectViewTest
    {
        #region Methods

        /// <summary>
        /// Show DeepFakeStudioProjectView.
        /// </summary>
        [TestMethod]
        public void ShowDeepFakeStudioProjectView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new DeepFakeStudioProjectViewModel();
                var view = new DeepFakeStudioProjectView { DataContext = viewModel };
                view.ShowDialog(nameof(DeepFakeStudioProjectView));
            });
        }

        #endregion Methods
    }
}