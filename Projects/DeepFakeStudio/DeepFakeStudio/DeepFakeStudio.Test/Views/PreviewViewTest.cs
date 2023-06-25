namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Helpers;
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="PreviewViewTest" />.
    /// </summary>
    [TestClass]
    public class PreviewViewTest
    {
        #region Methods

        /// <summary>
        /// Show PreviewViewTest.
        /// </summary>
        [TestMethod]
        public void Show_DeepFakeStudioPreviewView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new PreviewViewModel { Path = FileHelper.GetFolder() };
                var view = new PreviewView { DataContext = viewModel };
                view.ShowDialog(nameof(PreviewView));
            });
        }

        #endregion Methods
    }
}