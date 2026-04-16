// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TravelCamApp.Helpers
{
    public static partial class FileHelper
    {
        #region Fields / Constants

        /// <summary>File name for the private thumbnail stored in AppDataDirectory.</summary>
        public const string ThumbFileName = "last_thumb.jpg";

        /// <summary>Full path to the private thumbnail file (always a plain file path, never content://).</summary>
        public static string ThumbPath => Path.Combine(FileSystem.AppDataDirectory, ThumbFileName);

        #endregion

        #region Methods

        /// <summary>
        /// Saves a video stream (returned by StopVideoRecording) to the gallery and
        /// extracts the first frame as a thumbnail for the in-app preview.
        /// Returns (GalleryPath, ThumbPath). ThumbPath is a plain file path to a JPEG;
        /// empty string if frame extraction failed.
        /// </summary>
        public static async Task<(string GalleryPath, string ThumbPath)> SaveVideoAsync(Stream stream, string city)
        {
            var now = DateTime.Now;
            var datePart = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var timePart = now.ToString("HHmmss", CultureInfo.InvariantCulture);
            var safeCity = SanitizeFileName(city);

            System.Diagnostics.Debug.WriteLine($"[FileHelper] SaveVideoAsync called: city={city}, stream.Length={stream.Length}, CanSeek={stream.CanSeek}");

            if (stream.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[FileHelper] SaveVideoAsync: Stream is empty — aborting.");
                return (null, null);
            }

            var baseDir = GetAppCacheDir();
            Directory.CreateDirectory(baseDir);

            var fileName = $"{datePart}_{timePart}_{safeCity}.mp4";
            var tempPath = Path.Combine(baseDir, fileName);
            tempPath = EnsureUniquePath(tempPath);

            System.Diagnostics.Debug.WriteLine($"[FileHelper] SaveVideoAsync - Saving to temp: {tempPath}");

            if (stream.CanSeek)
                stream.Position = 0;

            using (var fileStream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                await fileStream.FlushAsync().ConfigureAwait(false);
            }

            var fileInfo = new FileInfo(tempPath);
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SaveVideoAsync - Temp file size: {fileInfo.Length} bytes");

            if (fileInfo.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[FileHelper] SaveVideoAsync - WARNING: File is empty! Deleting and returning nulls.");
                try { File.Delete(tempPath); } catch { /* ignore */ }
                return (null, null);
            }

            // Extract first frame as thumbnail before publishing to MediaStore (Android only)
            var thumbPath = string.Empty;
#if ANDROID
            var thumbFileName = Path.GetFileNameWithoutExtension(tempPath) + "_thumb.jpg";
            var thumbPathFull = Path.Combine(baseDir, thumbFileName);
            System.Diagnostics.Debug.WriteLine($"[FileHelper] Thumbnail will be saved to: {thumbPathFull}");
            thumbPath = await Task.Run(() => ExtractVideoFirstFrame(tempPath, thumbPathFull)).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"[FileHelper] Thumbnail extraction returned: '{thumbPath}'");
#endif

            var galleryPath = CopyToGallery(tempPath, "video/mp4");
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SaveVideoAsync returning: galleryPath='{galleryPath}', thumbPath='{thumbPath}'");
            return (galleryPath ?? tempPath, thumbPath);
        }

        /// <summary>
        /// Saves a captured photo stream to the gallery and also keeps a private
        /// thumbnail copy in AppDataDirectory for the in-app preview.
        /// Returns (GalleryPath, ThumbPath). GalleryPath is the content:// URI on
        /// Android; ThumbPath is always a plain file path suitable for ImageSource.FromFile.
        /// </summary>
        public static async Task<(string GalleryPath, string ThumbPath)> SavePhotoAsync(Stream stream, string city)
        {
            var now = DateTime.Now;
            var datePart = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var timePart = now.ToString("HHmmss", CultureInfo.InvariantCulture);
            var safeCity = SanitizeFileName(city);

            var baseDir = GetAppCacheDir();
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Cache dir: {baseDir}");

            Directory.CreateDirectory(baseDir);

            var fileName = $"{datePart}_{timePart}_{safeCity}.jpg";
            var tempPath = Path.Combine(baseDir, fileName);
            tempPath = EnsureUniquePath(tempPath);

            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Saving to temp: {tempPath}");

            if (stream.CanSeek)
                stream.Position = 0;

            using (var fileStream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            var fileInfo = new FileInfo(tempPath);
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Temp file saved: {tempPath}, size: {fileInfo.Length} bytes");

            if (fileInfo.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[FileHelper] SavePhotoAsync - WARNING: File is empty!");
                return (tempPath, tempPath);
            }

            // Save a private thumbnail copy BEFORE MediaStore deletes the temp file.
            var thumbPath = ThumbPath;
            try
            {
                File.Copy(tempPath, thumbPath, overwrite: true);
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Thumbnail saved to: {thumbPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Thumbnail copy failed: {ex.Message}");
                thumbPath = tempPath;
            }

            System.Diagnostics.Debug.WriteLine($"[FileHelper] Calling CopyToGallery for {tempPath}");
            var galleryPath = CopyToGallery(tempPath, "image/jpeg");
            System.Diagnostics.Debug.WriteLine($"[FileHelper] CopyToGallery returned: {galleryPath}");

            return (galleryPath ?? tempPath, thumbPath);
        }

        /// <summary>
        /// Returns all captured media (images and videos) from the captures cache directory,
        /// sorted newest first.
        /// </summary>
        public static List<string> GetAllCapturedImagePaths()
        {
            var dir = GetAppCacheDir();
            System.Diagnostics.Debug.WriteLine($"[FileHelper] GetAllCapturedImagePaths - checking dir: {dir}");

            if (!Directory.Exists(dir))
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] GetAllCapturedImagePaths - directory does not exist");
                return new List<string>();
            }

            var jpgFiles = Directory.GetFiles(dir, "*.jpg")
                .Where(f => !f.EndsWith("_thumb.jpg", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var mp4Files = Directory.GetFiles(dir, "*.mp4").ToList();
            var allFiles = jpgFiles.Concat(mp4Files).ToList();

            var files = allFiles
                .OrderByDescending(File.GetLastWriteTime)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[FileHelper] GetAllCapturedImagePaths - found {files.Count} files ({jpgFiles.Count} jpg, {mp4Files.Count} mp4)");
            foreach (var f in files)
                System.Diagnostics.Debug.WriteLine($"  - {f} ({new FileInfo(f).Length} bytes)");

            return files;
        }

        /// <summary>
        /// Queries MediaStore for all images and videos saved by TravelCam.
        /// Returns file paths sorted newest first. Falls back to cache dir on failure.
        /// </summary>
        public static List<string> GetAllGalleryMediaPaths()
        {
#if ANDROID
            return GetAllGalleryMediaPathsAndroid();
#else
            System.Diagnostics.Debug.WriteLine("[FileHelper] Non-Android platform, returning cache files");
            return GetAllCapturedImagePaths();
#endif
        }

        /// <summary>
        /// Deletes a media file from both the filesystem and MediaStore.
        /// Returns true if deletion succeeded.
        /// </summary>
        public static bool DeleteMedia(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            try
            {
#if ANDROID
                DeleteFromMediaStore(filePath);
                ExifHelper.InvalidateCache(filePath);
#endif
                File.Delete(filePath);
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Deleted: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Delete error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a temporary app-private directory for staging captures.
        /// </summary>
        private static string GetAppCacheDir()
        {
#if ANDROID
            return GetAppCacheDirAndroid();
#else
            return Path.Combine(FileSystem.CacheDirectory, "captures");
#endif
        }

        /// <summary>
        /// Copies a file into shared gallery storage using MediaStore (Android 10+)
        /// or direct file copy + media scan (other platforms).
        /// Returns the gallery-visible path, or null on failure.
        /// </summary>
        private static string? CopyToGallery(string sourcePath, string mimeType)
        {
#if ANDROID
            return CopyToGalleryAndroid(sourcePath, mimeType);
#else
            return sourcePath;
#endif
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Concat(name.Select(c => Array.IndexOf(invalid, c) >= 0 || c == ' ' ? '_' : c));
            return string.IsNullOrWhiteSpace(sanitized) ? "photo" : sanitized;
        }

        /// <summary>
        /// If the file already exists (same-second capture), appends _2, _3, etc. to avoid overwriting.
        /// </summary>
        private static string EnsureUniquePath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            var dir = Path.GetDirectoryName(filePath)!;
            var name = Path.GetFileNameWithoutExtension(filePath);
            var ext = Path.GetExtension(filePath);

            for (int i = 2; i < 1000; i++)
            {
                var candidate = Path.Combine(dir, $"{name}_{i}{ext}");
                if (!File.Exists(candidate))
                    return candidate;
            }
            return filePath;
        }

        #endregion
    }
}
