using System;
using System.IO;

namespace TravelCamApp.Helpers
{
    /// <summary>
    /// Provides common settings for the TravelCam application.
    /// </summary>
    internal static class Settings
    {
        /// <summary>
        /// Gets the default camera name.
        /// </summary>
        public static string DefaultCameraName => "CekliCam";

        /// <summary>
        /// Gets the application version.
        /// </summary>
        public static string AppVersion => AppInfo.VersionString;

        /// <summary>
        /// Gets the output path for images and videos.
        /// </summary>
        public static string OutputPath => EnsureDirectory(PublicAppFolder);

        /// <summary>
        /// Gets the public app folder accessible by users and other apps.
        /// </summary>
        private static string PublicAppFolder
        {
            get
            {
#if ANDROID
                return Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, DefaultCameraName);
#elif IOS
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), DefaultCameraName);
#else
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), DefaultCameraName);
#endif
            }
        }

        /// <summary>
        /// Ensures the specified directory exists.
        /// </summary>
        /// <param name="path">The directory path.</param>
        /// <returns>The directory path.</returns>
        private static string EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}