namespace DeepFakeStudio.Helpers
{
    using System;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Defines the <see cref="BitmapHelper" />.
    /// </summary>
    public static class BitmapHelper
    {
        #region Methods

        /// <summary>
        /// The LoadBitmap.
        /// </summary>
        /// <param name="path">The path<see cref="string"/>.</param>
        /// <returns>The <see cref="WriteableBitmap"/>.</returns>
        public static WriteableBitmap LoadBitmap(string path)
        {
            var bitmap = new BitmapImage(new Uri(path, UriKind.Absolute));
            return new WriteableBitmap(bitmap);
        }

        #endregion Methods
    }
}