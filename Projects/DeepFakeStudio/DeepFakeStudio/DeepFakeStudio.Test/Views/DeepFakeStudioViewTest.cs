namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Helpers;
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioViewTest" />.
    /// </summary>
    [TestClass]
    public class DeepFakeStudioViewTest
    {
        #region Methods

        /// <summary>
        /// Show DeepFakeStudioView.
        /// </summary>
        [TestMethod]
        public void ShowDeepFakeStudioView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new DeepFakeStudioViewModel();
                viewModel.DeepFakeStudioPreview.Path = FileHelper.GetFolder();
                var view = new DeepFakeStudioView { DataContext = viewModel };
                view.ShowDialog(nameof(DeepFakeStudioView));
            });
        }

        #endregion Methods
    }
}