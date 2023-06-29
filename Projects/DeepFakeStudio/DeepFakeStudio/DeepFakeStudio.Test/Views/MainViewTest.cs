namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Helpers;
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="MainViewTest" />.
    /// </summary>
    [TestClass]
    public class MainViewTest
    {
        #region Methods

        /// <summary>
        /// Show MainView.
        /// </summary>
        [TestMethod]
        public void ShowMainView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new MainViewModel();
                viewModel.PreviewViewModel.Path = FileHelper.GetFolder();
                var view = new MainView { DataContext = viewModel };
                view.ShowDialog(nameof(MainView));
            });
        }

        #endregion Methods
    }
}