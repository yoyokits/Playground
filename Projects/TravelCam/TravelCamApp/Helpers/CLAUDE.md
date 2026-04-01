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
Camera.MAUI 1.5.1 wrapper. **API verified from library source.**
- Returns `bool` (internal), library returns `CameraResult` (we unwrap it)
- Null-check `cameraView.Camera` before operations
- Use `CameraInfo` type, `CameraPosition.Back`, `FlashMode.Disabled/Enabled/Auto`
- Video recording: set `cameraView.Microphone` before `StartRecordingAsync`
- Zoom: scale 0.0-1.0 range to `MinZoomFactor..MaxZoomFactor`

## FileHelper.cs
File capture to MediaStore gallery publishing.
- Flow: Write to `ExternalCacheDir/captures/` → Copy to MediaStore with `IsPending=1` → `IsPending=0` → delete temp → return `content://` URI
- **Critical**: Close stream handles before MediaStore reads temp file
- Sanitize filenames, handle same-second captures with `_2`, `_3` suffixes

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
