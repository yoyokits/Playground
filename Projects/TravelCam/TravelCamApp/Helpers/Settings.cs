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
        public static string OutputPath
        {
            get
            {
                var path = EnsureDirectory(PublicAppFolder);
                System.Diagnostics.Debug.WriteLine($"[Settings] OutputPath: {path}");
                return path;
            }
        }

        /// <summary>
        /// Gets the public app folder accessible by users and other apps.
        /// </summary>
        private static string PublicAppFolder
        {
            get
            {
#if ANDROID
                try
                {
                    // Try the public Pictures directory first
                    var picturesDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                    if (picturesDir != null)
                    {
                        var path = Path.Combine(picturesDir.AbsolutePath, DefaultCameraName);
                        System.Diagnostics.Debug.WriteLine($"[Settings] PublicAppFolder (External Pictures): {path}");
                        
                        // Test if we can write to this directory
                        try
                        {
                            Directory.CreateDirectory(path);
                            var testFile = Path.Combine(path, ".test_write");
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                            System.Diagnostics.Debug.WriteLine($"[Settings] External storage is writable");
                            return path;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Settings] Cannot write to external storage: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Settings] Error accessing external storage: {ex.Message}");
                }

                // Fallback to app-specific external storage (no permission needed on Android 10+)
                var context = Android.App.Application.Context;
                var externalFilesDir = context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);
                if (externalFilesDir != null)
                {
                    var fallbackPath = Path.Combine(externalFilesDir.AbsolutePath, DefaultCameraName);
                    System.Diagnostics.Debug.WriteLine($"[Settings] PublicAppFolder (App External Files): {fallbackPath}");
                    return fallbackPath;
                }

                // Last resort: app internal storage
                var internalPath = Path.Combine(FileSystem.AppDataDirectory, DefaultCameraName);
                System.Diagnostics.Debug.WriteLine($"[Settings] PublicAppFolder (Internal): {internalPath}");
                return internalPath;
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
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    System.Diagnostics.Debug.WriteLine($"[Settings] Created directory: {path}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] Failed to create directory {path}: {ex.Message}");
            }

            return path;
        }
    }
}