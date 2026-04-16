# Platforms/Android — Configuration Reference

> Android-specific settings and permissions for this project.

---

## Version Targeting

```
minSdkVersion  = 29  (Android 10)
targetSdkVersion = 35 (Android 15)
```

Set in `TravelCamApp.csproj`:
```xml
<SupportedOSPlatformVersion Condition="... == 'android'">29</SupportedOSPlatformVersion>
<TargetSdkVersion>35</TargetSdkVersion>
```

---

## Permissions (AndroidManifest.xml)

### Runtime Permissions (requested at runtime)
| Permission | Android Version | Purpose |
|---|---|---|
| `CAMERA` | All | Camera preview, photo, video |
| `RECORD_AUDIO` | All | Video with audio |
| `ACCESS_FINE_LOCATION` | All | GPS coordinates, reverse geocoding, temperature |
| `ACCESS_COARSE_LOCATION` | All | Fallback location |
| `READ_MEDIA_IMAGES` | API 33+ (Android 13) | View captured photos |
| `READ_MEDIA_VIDEO` | API 33+ (Android 13) | View captured videos |
| `Photos` (MAUI) | API 33+ | MAUI wrapper for READ_MEDIA_IMAGES |
| `StorageRead` (MAUI) | API 29-32 | Read photos on older Android |

### Manifest Permissions (declared, no runtime prompt)
| Permission | Purpose |
|---|---|
| `INTERNET` | Weather API, maps |
| `ACCESS_NETWORK_STATE` | Network availability |

### Features (required / optional)
| Feature | Required | Purpose |
|---|---|---|
| `android.hardware.camera` | true | Camera hardware required |

---

## Scoped Storage Rules (Android 10+)

- **Android 10-12 (API 29-32)**: Use `StorageRead` permission OR MediaStore API
- **Android 13+ (API 33+)**: Use `READ_MEDIA_IMAGES` / `READ_MEDIA_VIDEO` — `StorageRead` ignored
- `requestLegacyExternalStorage="true"` in manifest (Android 10 only, ignored on 11+)
- `WRITE_EXTERNAL_STORAGE` NOT needed — MediaStore `ContentResolver.OpenOutputStream` handles writes

### MediaStore Usage Pattern (FileHelper.cs)
1. Write capture to app-private `ExternalCacheDir` (no permissions needed)
2. Insert into MediaStore with `IsPending=1`, `RelativePath="Pictures/CekliCam"`
3. Write file content via `ContentResolver.OpenOutputStream(uri)`
4. Set `IsPending=0` to make gallery-visible
5. Delete temp file

---

## FileProvider

Configured in:
- `AndroidManifest.xml`: `<provider>` element
- `file_paths.xml`: `<external-path>`, `<cache-path>`, `<files-path>`, `<external-files-path>`
- Authority: `${applicationId}.fileprovider`

Used for: sharing captured photos with gallery apps via `FileProvider.GetUriForFile()`

---

## Build Configuration

`MainActivity.cs` — `LaunchMode = LaunchMode.SingleTask`, `ResizeableActivity = true` (required for MediaElement)
`MainApplication.cs` — Standard MAUI entry point, delegates to `MauiProgram.CreateMauiApp()`

---

## ExifHelper.cs — EXIF Read/Write + In-Memory Cache

Android-only static helper for JPEG EXIF metadata.

### Write
- `ApplyMetadata(Stream, PhotoCaptureMetadata)` → writes GPS, date, device, flash + JSON UserComment. Returns `MemoryStream`. **Must run on background thread.**

### Read (Cached)
- `GetOrReadMetadata(filePath)` → returns `MediaInfo` (cached after first read)
- `GetImageDimensions(filePath)` → returns `(int Width, int Height)` with EXIF orientation swap applied (values 5-8 = 90/270 rotation)
- `InvalidateCache(filePath)` → call before deleting a file
- `ReadMetadata(filePath)` → raw uncached read (internal, prefer `GetOrReadMetadata`)

### Cache Architecture
- `ConcurrentDictionary<string, CachedExifData>` — thread-safe, zero dependencies
- `CachedExifData` record stores: `MediaInfo`, `ImageWidth`, `ImageHeight`, `ExifOrientation`
- `PopulateCache` does: `ReadMetadata` + `BitmapFactory.DecodeFile(InJustDecodeBounds)` + `ExifInterface.TagOrientation`
- First read ~150ms, subsequent reads <5ms (dictionary lookup)
- Both `UpdateOverlayPositionAsync` and `LoadGalleryOverlayItemsAsync` use the same cache — at most one disk read per image
