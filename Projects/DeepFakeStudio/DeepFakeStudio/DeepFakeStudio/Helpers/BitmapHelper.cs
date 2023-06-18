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
        /// The CreateBitmapImage.
        /// </summary>
        /// <param name="path">The path<see cref="string"/>.</param>
        /// <param name="height">The height<see cref="int"/>.</param>
        /// <returns>The <see cref="BitmapImage"/>.</returns>
        public static BitmapImage CreateBitmapImage(string path, int height = 0)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnDemand;
            bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
            bitmap.UriSource = new Uri(path);
            bitmap.DecodePixelHeight = height;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

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