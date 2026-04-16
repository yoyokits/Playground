# Helpers — Coding Conventions

> Read before modifying any file in this directory.

---

## Design Principles

1. **Static classes for utilities** — All helpers here are `static` classes with static methods.
2. **No ViewModel references** — Helpers accept plain data types and return plain results.
3. **Platform-specific code** — Wrap Android-specific APIs in `#if ANDROID` / `#endif`.
4. **Error handling** — Catch exceptions, log via `Debug.WriteLine`, return safe defaults.

---

## CameraHelper.cs
CommunityToolkit.Maui.Camera 6.0.0 wrapper. **API verified from library docs.**
- `using CommunityToolkit.Maui.Views;` for `CameraView`
- `using CommunityToolkit.Maui.Core;` for `CameraInfo`, `CameraPosition`, `CameraFlashMode`
- `CameraPosition.Rear` (NOT `Back`) for rear camera
- `CameraFlashMode` (NOT `FlashMode`)
- `SelectedCamera` (NOT `Camera`) to set the active device
- `StartCameraPreview(CancellationToken)` / `StopCameraPreview()` for preview
- `CaptureImage(CancellationToken)` triggers capture → `MediaCaptured` event fires
- `StartVideoRecording(CancellationToken)` / `StopVideoRecording(CancellationToken) → Task<Stream>`
- Zoom: scale 0.0-1.0 range to `MinZoomFactor..MaxZoomFactor` via `SelectedCamera`

## FileHelper.cs
File capture to MediaStore gallery publishing.
- Flow: Write to `ExternalCacheDir/captures/` → Copy to MediaStore with `IsPending=1` → `IsPending=0` → **retain temp file** for in-app viewing
- Returns `(GalleryPath, ThumbPath)` — both are plain file paths, not `content://` URIs
- **Critical**: Close stream handles before MediaStore reads temp file
- `SavePhotoAsync`: Saves photo, extracts thumbnail copy, returns both paths
- `SaveVideoAsync`: Saves video, extracts first frame as thumbnail (Android only)
- Sanitize filenames, handle same-second captures with `_2`, `_3` suffixes
- `GetAllGalleryMediaPaths()`: Combines MediaStore query with cache files as fallback
- `DeleteMedia()` invalidates ExifHelper cache before deleting file

## SensorHelper.cs
Location + compass + weather polling (10s timer).
- CancellationToken required on all async operations
- `_isUpdating` flag prevents concurrent updates
- `_cts` recreated on each `StartAsync()`, cancelled+disposed on `Stop()`
- Compass: `TaskCompletionSource` with 2s timeout
- Weather: Open-Meteo (free, no key): `api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current=temperature_2m`

## Settings.cs
Output directory with Android fallback:
1. `Pictures/CekliCam` (public, needs permission)
2. `ExternalFilesDir/CekliCam` (app-specific, no permission Android 10+)
3. `FileSystem.AppDataDirectory/CekliCam` (last resort)

## SettingsHelper.cs
JSON persistence at `AppDataDirectory/sensor_settings.json`. Stores `[{ Name, IsVisible }]`.
