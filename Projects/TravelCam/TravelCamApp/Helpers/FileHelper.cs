// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TravelCamApp.Helpers
{
    public static class FileHelper
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
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            var fileInfo = new FileInfo(tempPath);
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SaveVideoAsync - Temp file size: {fileInfo.Length} bytes");

            if (fileInfo.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[FileHelper] SaveVideoAsync - WARNING: File is empty!");
                return (tempPath, string.Empty);
            }

            // Extract first frame as thumbnail before publishing to MediaStore
            var thumbPath = string.Empty;
#if ANDROID
            thumbPath = ExtractVideoFirstFrame(tempPath);
#endif

            var galleryPath = CopyToGallery(tempPath, "video/mp4");
            return (galleryPath ?? tempPath, thumbPath);
        }

#if ANDROID
        /// <summary>
        /// Extracts the first frame of a video file and saves it as a JPEG thumbnail.
        /// Returns the thumb file path, or empty string on failure.
        /// </summary>
        private static string ExtractVideoFirstFrame(string videoPath)
        {
            try
            {
                using var retriever = new Android.Media.MediaMetadataRetriever();
                retriever.SetDataSource(videoPath);
                using var bitmap = retriever.GetFrameAtTime(0,
                    Android.Media.Option.ClosestSync);
                if (bitmap == null)
                {
                    System.Diagnostics.Debug.WriteLine("[FileHelper] Video first frame is null");
                    return string.Empty;
                }

                var thumbPath = ThumbPath;
                using var outStream = new FileStream(thumbPath, FileMode.Create, FileAccess.Write, FileShare.None);
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg!, 90, outStream);
                outStream.Flush();
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Video thumbnail saved: {thumbPath}");
                return thumbPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] ExtractVideoFirstFrame error: {ex.Message}");
                return string.Empty;
            }
        }
#endif

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

            // Save to app-private cache first
            var baseDir = GetAppCacheDir();
            Directory.CreateDirectory(baseDir);

            var fileName = $"{datePart}_{timePart}_{safeCity}.jpg";
            var tempPath = Path.Combine(baseDir, fileName);
            tempPath = EnsureUniquePath(tempPath);

            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Saving to temp: {tempPath}");

            if (stream.CanSeek)
                stream.Position = 0;

            // Write to temp file and CLOSE it before CopyToGallery reads it.
            using (var fileStream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
            // fileStream is now closed - safe to read from CopyToGallery.

            var fileInfo = new FileInfo(tempPath);
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Temp file size: {fileInfo.Length} bytes");

            if (fileInfo.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[FileHelper] SavePhotoAsync - WARNING: File is empty!");
                return (tempPath, tempPath);
            }

            // Save a private thumbnail copy BEFORE MediaStore deletes the temp file.
            // Always use a plain file path (never content://) so ImageSource.FromFile works reliably.
            var thumbPath = ThumbPath;
            try
            {
                File.Copy(tempPath, thumbPath, overwrite: true);
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Thumbnail saved to: {thumbPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Thumbnail copy failed: {ex.Message}");
                thumbPath = tempPath; // fallback: keep using temp path
            }

            // Copy into the gallery via MediaStore (Android 10+) or direct path (other platforms)
            var galleryPath = CopyToGallery(tempPath, "image/jpeg");
            return (galleryPath ?? tempPath, thumbPath);
        }

        /// <summary>
        /// Returns all captured image file paths from the captures cache directory,
        /// sorted newest first.
        /// </summary>
        public static List<string> GetAllCapturedImagePaths()
        {
            var dir = GetAppCacheDir();
            if (!Directory.Exists(dir))
                return new List<string>();

            return Directory.GetFiles(dir, "*.jpg")
                .OrderByDescending(File.GetLastWriteTime)
                .ToList();
        }

        /// <summary>
        /// Gets a temporary app-private directory for staging captures.
        /// </summary>
        private static string GetAppCacheDir()
        {
#if ANDROID
            var context = Android.App.Application.Context;
            var cacheDir = context.ExternalCacheDir?.AbsolutePath
                ?? context.CacheDir?.AbsolutePath
                ?? FileSystem.CacheDirectory;
            return Path.Combine(cacheDir, "captures");
#else
            return Path.Combine(FileSystem.CacheDirectory, "captures");
#endif
        }

        /// <summary>
        /// Copies a file into shared gallery storage using MediaStore (Android 10+)
        /// or direct file copy + media scan (older Android / other platforms).
        /// Returns the gallery-visible path, or null on failure.
        /// </summary>
        private static string? CopyToGallery(string sourcePath, string mimeType)
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var fileName = Path.GetFileName(sourcePath);
                return CopyToMediaStore(context, sourcePath, fileName, mimeType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] CopyToGallery failed: {ex.Message}");
                return null;
            }
#else
            return sourcePath;
#endif
        }

#if ANDROID
        private static string? CopyToMediaStore(
            Android.Content.Context context, string sourcePath, string fileName, string mimeType)
        {
            var isVideo = mimeType.StartsWith("video", StringComparison.OrdinalIgnoreCase);
            var collection = isVideo
                ? Android.Provider.MediaStore.Video.Media.ExternalContentUri
                : Android.Provider.MediaStore.Images.Media.ExternalContentUri;

            var values = new Android.Content.ContentValues();
            values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, Path.GetFileNameWithoutExtension(fileName));
            values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, mimeType);
            values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,
                $"{Android.OS.Environment.DirectoryPictures}/{Settings.DefaultCameraName}");
            // Mark as pending so gallery apps don't show a blank entry during the write
            values.Put(Android.Provider.MediaStore.IMediaColumns.IsPending, 1);

            var resolver = context.ContentResolver;
            var uri = resolver?.Insert(collection!, values);
            if (uri == null || resolver == null)
            {
                System.Diagnostics.Debug.WriteLine("[FileHelper] MediaStore insert returned null URI");
                return null;
            }

            try
            {
                using (var outputStream = resolver.OpenOutputStream(uri))
                {
                    if (outputStream == null)
                    {
                        resolver.Delete(uri, null, null);
                        return null;
                    }

                    using var inputStream = File.OpenRead(sourcePath);
                    inputStream.CopyTo(outputStream);
                    outputStream.Flush();
                }

                // Clear pending flag - the file is now complete and gallery-visible
                var updateValues = new Android.Content.ContentValues();
                updateValues.Put(Android.Provider.MediaStore.IMediaColumns.IsPending, 0);
                resolver.Update(uri, updateValues, null, null);

                System.Diagnostics.Debug.WriteLine($"[FileHelper] MediaStore: published {fileName} to gallery");

                // Clean up the temp file (thumbnail was already saved before this call)
                try { File.Delete(sourcePath); } catch { /* best effort */ }

                return uri.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] MediaStore write failed: {ex.Message}");
                // Remove the broken MediaStore entry
                try { resolver.Delete(uri, null, null); } catch { }
                return null;
            }
        }

        #endif

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
            return filePath; // fallback (shouldn't reach here)
        }

        #endregion
    }
}