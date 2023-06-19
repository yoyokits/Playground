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
    public class DeepFakeStudioPreviewViewTest
    {
        #region Methods

        /// <summary>
        /// Show DeepFakeStudioView.
        /// </summary>
        [TestMethod]
        public void Show_DeepFakeStudioPreviewView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new DeepFakeStudioPreviewViewModel { Path = FileHelper.GetFolder() };
                var view = new DeepFakeStudioPreviewView { DataContext = viewModel };
                view.ShowDialog(nameof(DeepFakeStudioPreviewView));
            });
        }

        #endregion Methods
    }
}