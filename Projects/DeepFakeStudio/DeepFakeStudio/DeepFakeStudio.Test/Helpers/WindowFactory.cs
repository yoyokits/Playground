namespace DeepFakeStudio.Test.Helpers
{
    using System.Windows;

    /// <summary>
    /// Defines the <see cref="WindowFactory" />.
    /// </summary>
    public static class WindowFactory
    {
        #region Methods

        /// <summary>
        /// The ShowDialog.
        /// </summary>
        /// <param name="view">The view<see cref="UIElement"/>.</param>
        /// <param name="title">The title<see cref="string"/>.</param>
        /// <param name="width">The width<see cref="double"/>.</param>
        /// <param name="height">The height<see cref="double"/>.</param>
        public static void ShowDialog(this UIElement view, string title, double width = 1200, double height = 900)
        {
            var window = new Window
            {
                Content = view,
                Width = width,
                Height = height,
                Title = title
            };

            window.ShowDialog();
        }

        #endregion Methods
    }
}