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
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Cache dir: {baseDir}");

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
            System.Diagnostics.Debug.WriteLine($"[FileHelper] SavePhotoAsync - Temp file saved: {tempPath}, size: {fileInfo.Length} bytes");

            if (fileInfo.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[FileHelper] SavePhotoAsync - WARNING: File is empty!");
                return (tempPath, tempPath);
            }

            // Save a private thumbnail copy BEFORE MediaStore deletes the temp file.
            // Always use a plain file path (never content://) so it can be loaded via ImageSource.FromStream().
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
            System.Diagnostics.Debug.WriteLine($"[FileHelper] Calling CopyToGallery for {tempPath}");
            var galleryPath = CopyToGallery(tempPath, "image/jpeg");
            System.Diagnostics.Debug.WriteLine($"[FileHelper] CopyToGallery returned: {galleryPath}");

            return (galleryPath ?? tempPath, thumbPath);
        }

        /// <summary>
        /// Returns all captured image file paths from the captures cache directory,
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

            var files = Directory.GetFiles(dir, "*.jpg")
                .OrderByDescending(File.GetLastWriteTime)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[FileHelper] GetAllCapturedImagePaths - found {files.Count} files");
            foreach (var f in files)
            {
                System.Diagnostics.Debug.WriteLine($"  - {f} ({new FileInfo(f).Length} bytes)");
            }

            return files;
        }

        /// <summary>
        /// Queries MediaStore for all images and videos saved by TravelCam (in Pictures/CekliCam).
        /// Returns file paths sorted newest first. Falls back to cache dir + thumb on failure.
        /// </summary>
        public static List<string> GetAllGalleryMediaPaths()
        {
            System.Diagnostics.Debug.WriteLine("[FileHelper] GetAllGalleryMediaPaths called");

#if ANDROID
            // First, try to get paths from MediaStore
            try
            {
                var mediaStorePaths = GetMediaStoreImages();
                System.Diagnostics.Debug.WriteLine($"[FileHelper] MediaStore returned {mediaStorePaths.Count} paths");

                // Filter to only existing files - MediaStore entries may be stale or inaccessible
                var existingMediaPaths = mediaStorePaths
                    .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[FileHelper] {existingMediaPaths.Count} MediaStore paths exist on disk");

                // If we have valid MediaStore paths, return them
                if (existingMediaPaths.Count > 0)
                {
                    // Also include cache files to ensure we always have images available
                    var cachePaths = GetAllCapturedImagePaths();
                    var allPaths = existingMediaPaths.Concat(cachePaths).Distinct()
                        .OrderByDescending(p => File.GetLastWriteTime(p))
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"[FileHelper] Returning {allPaths.Count} combined paths");
                    return allPaths.Count > 0 ? allPaths : GetAllCapturedImagePaths();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FileHelper] GetAllGalleryMediaPaths MediaStore error: {ex.Message}");
            }

            // MediaStore query returned empty or failed - fall back to cache files
            // This handles: stale MediaStore entries, permission issues, app data cleared
            System.Diagnostics.Debug.WriteLine("[FileHelper] Falling back to cache files");
            return GetAllCapturedImagePaths();
#else
            System.Diagnostics.Debug.WriteLine("[FileHelper] Non-Android platform, returning cache files");
            return GetAllCapturedImagePaths();
#endif
        }

#if ANDROID
        // Directly return captured images from the app's cache directory.
        // This is simpler and more reliable than querying MediaStore.
        private static List<string> GetMediaStoreImages()
        {
            return GetAllCapturedImagePaths();
        }

        private static void QueryMediaStore(
            Android.Content.ContentResolver resolver,
            Android.Net.Uri collection,
            string relativePath,
            List<(string path, long dateAdded)> results)
        {
            string[] projection = {
                Android.Provider.MediaStore.IMediaColumns.Data!,
                Android.Provider.MediaStore.IMediaColumns.DateAdded!
            };

            string selection = $"{Android.Provider.MediaStore.IMediaColumns.RelativePath} = ?";
            string[] selectionArgs = { relativePath + "/" };

            // Declare the resolver variable - it shadows the parameter due to nested #if scope
            var contentResolver = resolver;

            using var cursor = contentResolver.Query(collection, projection, selection, selectionArgs,
                $"{Android.Provider.MediaStore.IMediaColumns.DateAdded} DESC");

            if (cursor == null) return;

            int dataIndex = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.Data!);
            int dateIndex = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.DateAdded!);

            while (cursor.MoveToNext())
            {
                var path = dataIndex >= 0 ? cursor.GetString(dataIndex) : null;
                var dateAdded = dateIndex >= 0 ? cursor.GetLong(dateIndex) : 0;

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    results.Add((path, dateAdded));
            }
        }
#endif

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

#if ANDROID

        // Static field to hold the ContentResolver reference
        private static Android.Content.ContentResolver? _contentResolver;

        /// <summary>
        /// Gets the ContentResolver reference for use in ConvertMediaStorePathToFilePath.
        /// </summary>
        private static Android.Content.ContentResolver? Resolver => _contentResolver;

        /// <summary>
        /// Converts a MediaStore URI to an actual file path, or returns the path as-is if it's already a file path.
        /// Returns empty string if conversion fails.
        /// </summary>
        // Simplified: just return path if it exists (caller validates)
        private static string ConvertMediaStorePathToFilePath(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                return path;
            return string.Empty;
        }

        /// <summary>
        /// Initializes the resolver reference for use in ConvertMediaStorePathToFilePath.
        /// This must be called before querying MediaStore images.
        /// </summary>
        public static void InitializeMediaStoreResolver(Android.Content.ContentResolver resolver)
        {
            _contentResolver = resolver;
        }

        private static void DeleteFromMediaStore(string filePath)
        {
            var context = Android.App.Application.Context;
            var contentResolver = context.ContentResolver;
            if (contentResolver == null) return;

            // Try images collection first, then videos
            var collections = new[] {
                Android.Provider.MediaStore.Images.Media.ExternalContentUri!,
                Android.Provider.MediaStore.Video.Media.ExternalContentUri!
            };

            foreach (var collection in collections)
            {
                string selection = $"{Android.Provider.MediaStore.IMediaColumns.Data} = ?";
                string[] selectionArgs = { filePath };
                int deleted = contentResolver.Delete(collection, selection, selectionArgs);
                if (deleted > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[FileHelper] Removed from MediaStore: {filePath}");
                    return;
                }
            }
        }
#endif

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

                // NOTE: We do NOT delete sourcePath here because we need to return it
                // for in-app viewing. The file remains in ExternalCacheDir and is
                // accessible until the app is uninstalled or the cache is cleared.
                // The MediaStore URI is NOT returned because Image control may not
                // handle it correctly in all scenarios.

                return sourcePath;
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