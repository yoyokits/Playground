﻿namespace DeepFakeStudio.Helpers
{
    using Ookii.Dialogs.Wpf;

    /// <summary>
    /// Defines the <see cref="FileHelper" />.
    /// </summary>
    public static class FileHelper
    {
        #region Methods

        /// <summary>
        /// The GetFile.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetFile()
        {
            var dialog = new VistaOpenFileDialog();
            var result = dialog.ShowDialog();
            var fileName = string.Empty;
            if (result.HasValue && result.Value)
            {
                fileName = dialog.FileName;
            }

            return fileName;
        }

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

        #endregion Methods
    }
}