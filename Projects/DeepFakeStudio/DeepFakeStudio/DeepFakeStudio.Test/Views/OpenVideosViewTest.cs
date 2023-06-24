namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="OpenVideosViewTest" />.
    /// </summary>
    [TestClass]
    public class OpenVideosViewTest
    {
        #region Methods

        /// <summary>
        /// Show OpenVideosView.
        /// </summary>
        [TestMethod]
        public void OpenVideosView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new OpenVideosViewModel();
                var view = new OpenVideosView { DataContext = viewModel };
                view.ShowDialog(nameof(OpenVideosView));
            });
        }

        #endregion Methods
    }
}