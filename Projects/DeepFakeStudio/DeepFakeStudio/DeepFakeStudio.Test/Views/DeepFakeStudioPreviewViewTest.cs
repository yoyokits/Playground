namespace DeepFakeStudio.Test.Views
{
    using DeepFakeStudio.Test.Helpers;
    using DeepFakeStudio.ViewModels;
    using DeepFakeStudio.Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ookii.Dialogs.Wpf;

    /// <summary>
    /// Defines the <see cref="DeepFakeStudioViewTest" />.
    /// </summary>
    [TestClass]
    public class DeepFakeStudioPreviewViewTest
    {
        #region Methods

        /// <summary>
        /// The GetFolder.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetFolder()
        {
            var dialog = new VistaFolderBrowserDialog();
            var result = dialog.ShowDialog();
            var path = string.Empty;
            if (result.HasValue && result.Value)
            {
                path = dialog.SelectedPath;
            }

            return path;
        }

        /// <summary>
        /// Show DeepFakeStudioView.
        /// </summary>
        [TestMethod]
        public void Show_DeepFakeStudioPreviewView()
        {
            WpfTestRunner.Run(_ =>
            {
                var viewModel = new DeepFakeStudioPreviewViewModel { Path = GetFolder() };
                var view = new DeepFakeStudioPreviewView { DataContext = viewModel };
                view.ShowDialog(nameof(DeepFakeStudioPreviewView));
            });
        }

        #endregion Methods
    }
}