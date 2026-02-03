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

            await using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);

            return filePath;
        }

        #endregion
    }
}
