// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// Android-specific FileHelper implementations.
// MSBuild automatically includes this file only for net10.0-android builds.

using System;
using System.IO;
using System.Linq;
using TravelCamApp.Helpers;

namespace TravelCamApp.Helpers
{
    public static partial class FileHelper
    {
        private static Android.Content.ContentResolver? _contentResolver;

        /// <summary>
        /// Initializes the ContentResolver used for MediaStore operations.
        /// Call this once from MainActivity or after camera initialization.
        /// </summary>
        public static void InitializeMediaStoreResolver(Android.Content.ContentResolver resolver)
            => _contentResolver = resolver;

        private static string GetAppCacheDirAndroid()
        {
            var context = Android.App.Application.Context;
            var cacheDir = context.ExternalCacheDir?.AbsolutePath
                ?? context.CacheDir?.AbsolutePath
                ?? FileSystem.CacheDirectory;
            return Path.Combine(cacheDir, "captures");
        }

        private static string? CopyToGalleryAndroid(string sourcePath, string mimeType)
        {
            try
            {
                var context = Android.App.Application.Context;
                return CopyToMediaStore(context, sourcePath, Path.GetFileName(sourcePath), mimeType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] CopyToGallery failed: {ex.Message}");
                return null;
            }
        }

        private static List<string> GetAllGalleryMediaPathsAndroid()
        {
            System.Diagnostics.Debug.WriteLine("[FileHelper] GetAllGalleryMediaPaths called");
            try
            {
                // MediaStore returns DCIM/CekliCam paths; cache holds ExternalCacheDir copies.
                // They are separate files — do NOT combine to avoid showing duplicates.
                // Prefer MediaStore: it is the canonical gallery-visible copy.
                // Fall back to cache only when MediaStore returns nothing (first-run, reinstall).
                var mediaStorePaths = GetMediaStoreImages();
                System.Diagnostics.Debug.WriteLine(
                    $"[FileHelper] MediaStore returned {mediaStorePaths.Count} paths");

                if (mediaStorePaths.Count > 0)
                    return mediaStorePaths;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FileHelper] GetAllGalleryMediaPaths MediaStore error: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine("[FileHelper] Falling back to cache files");
            return GetAllCapturedImagePaths();
        }

        /// <summary>
        /// Queries MediaStore for all images and videos saved in the app's DCIM folder.
        /// Returns absolute file paths sorted newest-first.
        /// Returns empty list (not null) on any failure.
        /// </summary>
        private static List<string> GetMediaStoreImages()
        {
            var result = new List<string>();
            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;
            if (resolver == null) return result;

            string[] projection = { Android.Provider.MediaStore.IMediaColumns.Data };
            // RELATIVE_PATH ends with '/' on Android — match the DCIM subfolder name.
            string selection = $"{Android.Provider.MediaStore.IMediaColumns.RelativePath} LIKE ?";
            string[] selectionArgs = { $"%{Settings.DefaultCameraName}%" };
            string sortOrder = $"{Android.Provider.MediaStore.IMediaColumns.DateAdded} DESC";

            var collections = new[]
            {
                Android.Provider.MediaStore.Images.Media.ExternalContentUri!,
                Android.Provider.MediaStore.Video.Media.ExternalContentUri!
            };

            foreach (var uri in collections)
            {
                try
                {
                    using var cursor = resolver.Query(uri, projection, selection, selectionArgs, sortOrder);
                    if (cursor == null) continue;

                    var dataIdx = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.Data);
                    while (cursor.MoveToNext())
                    {
                        var path = cursor.GetString(dataIdx);
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                            result.Add(path);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[FileHelper] GetMediaStoreImages query error: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[FileHelper] MediaStore query returned {result.Count} paths");
            return result;
        }

        /// <summary>
        /// Extracts the first frame of a video file and saves it as a JPEG thumbnail.
        /// Returns the thumb file path, or empty string on failure.
        /// </summary>
        private static string ExtractVideoFirstFrame(string videoPath, string outputThumbPath)
        {
            Android.Graphics.Bitmap? bitmap = null;
            try
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] ExtractVideoFirstFrame START: video='{videoPath}', thumb='{outputThumbPath}'");

                if (!File.Exists(videoPath))
                {
                    System.Diagnostics.Debug.WriteLine("[FileHelper] ERROR: Video file does not exist!");
                    return string.Empty;
                }

                var videoSize = new FileInfo(videoPath).Length;
                System.Diagnostics.Debug.WriteLine($"[FileHelper] Video file size: {videoSize} bytes");
                if (videoSize == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[FileHelper] ERROR: Video file is empty!");
                    return string.Empty;
                }

                using var retriever = new Android.Media.MediaMetadataRetriever();
                retriever.SetDataSource(videoPath);

                bitmap = retriever.GetFrameAtTime(0, Android.Media.Option.ClosestSync);
                if (bitmap == null)
                {
                    System.Diagnostics.Debug.WriteLine("[FileHelper] ClosestSync returned null, trying Closest...");
                    bitmap = retriever.GetFrameAtTime(0, Android.Media.Option.Closest);
                    if (bitmap == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[FileHelper] Closest also returned null — no video track or unsupported codec.");
                        return string.Empty;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[FileHelper] Got bitmap: {bitmap.Width}x{bitmap.Height}");

                var thumbDir = Path.GetDirectoryName(outputThumbPath);
                if (!string.IsNullOrEmpty(thumbDir))
                    Directory.CreateDirectory(thumbDir);

                // Scale down to thumbnail size to reduce memory and file size
                const int maxDim = 120;
                Android.Graphics.Bitmap scaledBitmap;
                if (bitmap.Width > maxDim || bitmap.Height > maxDim)
                {
                    float scale = Math.Min((float)maxDim / bitmap.Width, (float)maxDim / bitmap.Height);
                    int newWidth = (int)(bitmap.Width * scale);
                    int newHeight = (int)(bitmap.Height * scale);
                    scaledBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, newWidth, newHeight, true);
                    if (scaledBitmap != bitmap)
                        bitmap.Recycle();
                }
                else
                {
                    scaledBitmap = bitmap;
                }

                using var outStream = new FileStream(outputThumbPath, FileMode.Create, FileAccess.Write, FileShare.None);
                bool compressed = scaledBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg!, 85, outStream);
                outStream.Flush();
                if (scaledBitmap != bitmap)
                    scaledBitmap.Recycle();

                System.Diagnostics.Debug.WriteLine($"[FileHelper] Video thumbnail saved: {outputThumbPath}, compressed={compressed}");

                if (File.Exists(outputThumbPath))
                {
                    var thumbSize = new FileInfo(outputThumbPath).Length;
                    if (thumbSize == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("[FileHelper] WARNING: Thumbnail file is empty!");
                        try { File.Delete(outputThumbPath); } catch { }
                        return string.Empty;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[FileHelper] ERROR: Thumbnail file was not created!");
                    return string.Empty;
                }

                return outputThumbPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] ExtractVideoFirstFrame EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FileHelper] StackTrace: {ex.StackTrace}");
                return string.Empty;
            }
            finally
            {
                try { bitmap?.Dispose(); } catch { }
            }
        }

        private static string? CopyToMediaStore(
            Android.Content.Context context, string sourcePath, string fileName, string mimeType)
        {
            var isVideo = mimeType.StartsWith("video", StringComparison.OrdinalIgnoreCase);

            // DCIM/CekliCam — standard camera location, visible in ALL gallery apps.
            // Using Pictures/ for videos causes MediaStore to silently reject inserts on
            // Android 11+ because Pictures is categorised as an image-only bucket.
            var relativePath = $"{Android.OS.Environment.DirectoryDcim}/{Settings.DefaultCameraName}";

            var collection = isVideo
                ? Android.Provider.MediaStore.Video.Media.ExternalContentUri!
                : Android.Provider.MediaStore.Images.Media.ExternalContentUri!;

            var nowSec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var values = new Android.Content.ContentValues();
            values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName,     Path.GetFileNameWithoutExtension(fileName));
            values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType,        mimeType);
            values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,    relativePath);
            values.Put(Android.Provider.MediaStore.IMediaColumns.DateAdded,       nowSec);
            values.Put(Android.Provider.MediaStore.IMediaColumns.DateModified,    nowSec);
            // DATE_TAKEN is milliseconds since epoch — used by gallery apps for sorting.
            values.Put("datetaken", nowSec * 1000L);
            values.Put(Android.Provider.MediaStore.IMediaColumns.IsPending, 1);

            var resolver = context.ContentResolver;
            var uri = resolver?.Insert(collection, values);
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
                        System.Diagnostics.Debug.WriteLine("[FileHelper] MediaStore OpenOutputStream returned null");
                        return null;
                    }

                    using var inputStream = File.OpenRead(sourcePath);
                    inputStream.CopyTo(outputStream);
                    outputStream.Flush();
                }

                // Clear IsPending — this makes the file visible to every gallery app.
                var updateValues = new Android.Content.ContentValues();
                updateValues.Put(Android.Provider.MediaStore.IMediaColumns.IsPending, 0);
                resolver.Update(uri, updateValues, null, null);

                System.Diagnostics.Debug.WriteLine(
                    $"[FileHelper] MediaStore: published '{fileName}' to {relativePath}");

                // Return the cache copy for in-app viewing (avoids a second round-trip read from DCIM).
                return sourcePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FileHelper] MediaStore write failed: {ex.Message}");
                try { resolver.Delete(uri, null, null); } catch { }
                return null;
            }
        }

        private static void DeleteFromMediaStore(string filePath)
        {
            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;
            if (resolver == null) return;

            // The MediaStore entry was created with DISPLAY_NAME = file-name-without-extension
            // and RELATIVE_PATH = DCIM/CekliCam — match both so we delete only our own entry.
            // We do NOT use _DATA because the MediaStore copy lives at a different path from
            // the cache copy that filePath refers to.
            var displayName = Path.GetFileNameWithoutExtension(filePath);
            var selection = $"{Android.Provider.MediaStore.IMediaColumns.DisplayName} = ?"
                + $" AND {Android.Provider.MediaStore.IMediaColumns.RelativePath} LIKE ?";
            var selectionArgs = new[]
            {
                displayName,
                $"%{Settings.DefaultCameraName}%"
            };

            var collections = new[]
            {
                Android.Provider.MediaStore.Images.Media.ExternalContentUri!,
                Android.Provider.MediaStore.Video.Media.ExternalContentUri!
            };

            foreach (var collection in collections)
            {
                try
                {
                    int deleted = resolver.Delete(collection, selection, selectionArgs);
                    if (deleted > 0)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[FileHelper] Removed from MediaStore: {displayName}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[FileHelper] DeleteFromMediaStore error: {ex.Message}");
                }
            }
        }
    }
}
