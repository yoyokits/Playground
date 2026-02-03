using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using TravelCamApp.Helpers;

namespace TravelCamApp.Helpers
{
    public static class FileHelper
    {
        #region Methods

        public static string CreateVideoPath(string city)
        {
            var now = DateTime.Now;
            var year = now.Year.ToString(CultureInfo.InvariantCulture);
            var month = now.Month.ToString("00", CultureInfo.InvariantCulture);
            var day = now.Day.ToString("00", CultureInfo.InvariantCulture);

            var baseDir = Settings.OutputPath;
            Directory.CreateDirectory(baseDir);

            var prefix = $"{year}-{month}-{day}-{city}-";
            var index = GetNextIndex(baseDir, prefix, ".mp4");
            var fileName = $"{prefix}{index:000}.mp4";
            return Path.Combine(baseDir, fileName);
        }

        private static int GetNextIndex(string baseDir, string prefix, string extension)
        {
            var existing = Directory.GetFiles(baseDir, $"{prefix}*{extension}");
            var max = 0;

            foreach (var file in existing)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var tail = name.Replace(prefix, string.Empty, StringComparison.OrdinalIgnoreCase);
                if (int.TryParse(tail, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                {
                    max = Math.Max(max, value);
                }
            }

            return max + 1;
        }

        public static async Task<string> SavePhotoAsync(Stream stream, string city)
        {
            var now = DateTime.Now;
            var year = now.Year.ToString(CultureInfo.InvariantCulture);
            var month = now.Month.ToString("00", CultureInfo.InvariantCulture);
            var day = now.Day.ToString("00", CultureInfo.InvariantCulture);

            var baseDir = Settings.OutputPath;
            Directory.CreateDirectory(baseDir);

            var prefix = $"{year}-{month}-{day}-{city}-";
            var index = GetNextIndex(baseDir, prefix, ".jpg");
            var fileName = $"{prefix}{index:000}.jpg";
            var filePath = Path.Combine(baseDir, fileName);

            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Starting to save photo");
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Stream position: {stream.Position}, Length: {stream.Length}, CanSeek: {stream.CanSeek}");
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Target path: {filePath}");

            // Reset stream position to beginning if possible
            if (stream.CanSeek)
            {
                stream.Position = 0;
                System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Stream position reset to 0");
            }

            await using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            var fileInfo = new FileInfo(filePath);
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Photo saved successfully. File size: {fileInfo.Length} bytes");

            if (fileInfo.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - WARNING: File is empty!");
            }

            // Notify the media scanner on Android so the image appears in gallery
            NotifyMediaScanner(filePath);

            return filePath;
        }

        /// <summary>
        /// Notifies the media scanner to index the new file so it appears in the gallery.
        /// </summary>
        /// <param name="filePath">The file path to scan.</param>
        private static void NotifyMediaScanner(string filePath)
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                Android.Media.MediaScannerConnection.ScanFile(
                    context,
                    new[] { filePath },
                    new[] { "image/jpeg" },
                    null);
                System.Diagnostics.Debug.WriteLine($"[FileHelper] MediaScanner notified for: {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] MediaScanner notification failed: {ex.Message}");
            }
#endif
        }

        #endregion
    }
}
