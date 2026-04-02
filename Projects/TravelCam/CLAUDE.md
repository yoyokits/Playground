# CLAUDE.md — TravelCam MAUI Camera App

> This file configures Claude Code when working on the TravelCam project.
> Read it on every task before making changes.

---

## PROJECT OVERVIEW

TravelCam is a .NET MAUI camera application for Android (API 29+) that captures photos and video with real-time sensor data overlays (location, temperature, compass). It follows MVVM architecture with dependency injection.

### Tech Stack

| Component | Version |
|---|---|
| .NET MAUI | 10.0.30 |
| Target Framework | `net10.0-android` (also `net10.0-windows10.0.19041.0`) |
| Android Min SDK | API 29 (Android 10) |
| Android Target SDK | API 35 (Android 15) |
| CommunityToolkit.Maui | 14.0.0 |
| CommunityToolkit.Maui.Camera | 6.0.0 |

### Source

- CommunityToolkit.Maui.Camera API: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/camera-view

---

## CRITICAL API REFERENCE — CommunityToolkit.Maui.Camera 6.0.0

**DO NOT** use hypothetical API names. Only use what is documented below or verified from the library source.

### Namespaces

```csharp
using CommunityToolkit.Maui.Views;   // CameraView
using CommunityToolkit.Maui.Core;    // CameraInfo, CameraPosition, CameraFlashMode
```

### XAML namespace

```xml
xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
<!-- CameraView element: <toolkit:CameraView ... /> -->
```

### MauiProgram setup

```csharp
builder.UseMauiCommunityToolkit()
       .UseMauiCommunityToolkitCamera()
```

### Key types

```csharp
// Camera device info
public class CameraInfo {
    string Name { get; }
    CameraPosition Position { get; }   // Front, Rear  (NOT Back)
    // ⚠️ Zoom bounds are NOT reliably available on CameraInfo in v6.0.0.
    // Do NOT attempt to read MinZoomFactor / MaxZoomFactor / MinimumZoomFactor / MaximumZoomFactor.
    // Manage zoom preset values in the ViewModel directly.
}

// Camera position enum
public enum CameraPosition { Front, Rear }

// Flash mode enum (NOT FlashMode)
public enum CameraFlashMode { Off, On }
```

### CameraView API

```csharp
// SELECTED CAMERA
CameraInfo? SelectedCamera { get; set; }   // set before StartCameraPreview

// PROPERTIES
float ZoomFactor { get; set; }
CameraFlashMode CameraFlashMode { get; set; }
bool IsAvailable { get; }
bool IsBusy { get; }

// METHODS — note: no "Async" suffix on these methods
ValueTask<IReadOnlyList<CameraInfo>> GetAvailableCameras(CancellationToken);
Task StartCameraPreview(CancellationToken);   // starts preview
void StopCameraPreview();                      // stops preview (sync)
Task CaptureImage(CancellationToken);          // triggers capture → MediaCaptured event
Task StartVideoRecording(CancellationToken);   // starts recording
Task<Stream> StopVideoRecording(CancellationToken);  // stops and returns video stream

// EVENTS
event EventHandler<MediaCapturedEventArgs> MediaCaptured;
event EventHandler<MediaCaptureFailedEventArgs> MediaCaptureFailed;

// MediaCapturedEventArgs
Stream Media { get; }   // captured image data

// MediaCaptureFailedEventArgs
string FailureReason { get; }
```

### CRITICAL BEHAVIORS

1. **No `CamerasLoaded` event**: Unlike Camera.MAUI, there is no camera-loaded event.
   Camera is initialized in `OnAppearing()` via `await ViewModel.OnViewReady(CameraView)`.

2. **Photo capture is event-based**: Call `CaptureImage(token)` to trigger, then
   handle the result in the `MediaCaptured` event handler. The event fires with a `Stream`.

3. **Video returns a Stream**: `StopVideoRecording(token)` returns `Task<Stream>`.
   Save the stream to a temp file, then publish to MediaStore.

4. **No `IsPreviewing` or `IsRecording` property**: Track state manually in the ViewModel.

5. **Toggle camera**: `StopCameraPreview()`, set `SelectedCamera`, then `StartCameraPreview()`.
   Unlike Camera.MAUI, there is no auto-restart on property change.

---

## ARCHITECTURE

### MVVM Pattern
- **Views** (XAML + code-behind): UI only, no business logic
- **ViewModels**: Business logic, state, commands. Implement `INotifyPropertyChanged`
- **Models**: Pure data classes
- **Helpers**: Static utility classes for platform operations
- **Services**: External communications (none yet, but reserved)

### Dependency Injection (MauiProgram.cs)
```csharp
// Singletons — shared across app
builder.Services.AddSingleton<SensorHelper>();

// Transient — new instance per injection
builder.Services.AddTransient<MainPageViewModel>();
builder.Services.AddTransient<SensorValueSettingsViewModel>();
builder.Services.AddTransient<MainPage>();
builder.Services.AddTransient<SensorValueSettingsView>();
```

### Camera Lifecycle
1. `CamerasLoaded` event fires → ViewModel receives CameraView reference
2. ViewModel calls `CameraHelper.SelectFirstAvailableCamera()` then `StartPreviewAsync()`
3. `Window.Stopped` → stop camera, stop sensors
4. `Window.Resumed` → restart sensors, restart camera preview

### Sensor Architecture
- `SensorHelper` is the single source of truth for all sensor data
- Runs on a 10-second timer collecting GPS, compass, and weather (Open-Meteo API)
- Publishes updates via `SensorDataUpdatedCallback` event
- All ViewModels subscribe to this event — NO duplicate sensor polling

### Storage Architecture (Android 10+ / API 29+)
1. Photos/videos saved to app-private cache dir (`ExternalCacheDir/captures/`) first
2. `CopyToMediaStore` copies file to MediaStore with `IsPending=1` flag
3. After write completes, `IsPending=0` and temp file is deleted
4. Gallery returns `content://` URI for viewing

### Permissions Required
- Camera + Microphone (runtime)
- LocationWhenInUse (runtime)
- Photos (Android 13+ / API 33+) or StorageRead (Android 10-12)
- Manifest: CAMERA, RECORD_AUDIO, INTERNET, ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION, READ_EXTERNAL_STORAGE (<=32), READ_MEDIA_IMAGES, READ_MEDIA_VIDEO

---

## CODING STANDARDS

See `CodeStyle.txt` for full standards. Key rules:

### Naming
- Classes: PascalCase
- Methods: PascalCase, async suffix `Async`
- Private fields: `_camelCase`
- Constants: PascalCase
- Regions: Fields, Properties, Constructors, Methods, Commands

### File Size Limits
- Models: < 200 lines
- ViewModels: < 500 lines
- Helpers: < 400 lines
- XAML files: < 300 lines
- Code-behind: < 800 lines

### SOLID / DRY / KISS / YAGNI
- Single Responsibility Principle strictly enforced
- No over-engineering
- Only implement what Requirements.txt specifies

---

## GOOGLE PLAY COMPLIANCE

| Requirement | Status | Notes |
|---|---|---|
| Target SDK 35 (API 35) | OK | Current requirement until Aug 2026 |
| Target SDK 36 (API 36) | PLANNED | Required by Aug 31, 2026 for new uploads |
| Scoped Storage (API 29+) | OK | MediaStore with IsPending flag |
| Runtime Permissions | OK | Camera, Mic, Location, Storage handled |
| Camera.MAUI 1.5.1 | RISK | Last release; consider CommunityToolkit.Maui.Camera for long-term |

## KEY FILES

| File | Description |
|---|---|
| `MauiProgram.cs` | DI registration (SensorHelper singleton, ViewModels, Pages) |
| `MainPageViewModel.cs` | Main coordinator — permissions, camera, sensors, lifecycle |
| `MainPage.xaml` | Camera UI with permission overlay, sensor overlay, mode selector |
| `MainPage.xaml.cs` | OnAppearing camera init, MediaCaptured routing, settings overlay |
| `CameraHelper.cs` | Static CommunityToolkit.Maui.Camera 6.0.0 wrapper |
| `SensorHelper.cs` | GPS+Compass+Weather polling (10s), IDisposable |
| `FileHelper.cs` | MediaStore gallery integration |
| `SettingsHelper.cs` | JSON persistence for sensor settings |
| `SensorData.cs` | Sensor data model |
| `SensorItem.cs` | Observable display item (Name, Value, IsVisible) |
| `SensorValueSettingsViewModel.cs` | Manages visible/available sensor lists |
| `SensorValueViewModel.cs` | Owns SensorItems, subscribes to SensorHelper, wired into main flow |

---

## WHAT TO CHECK BEFORE BUILDING

When asked to build or fix, run:
```bash
cd C:\Git\Playground\Projects\TravelCam\TravelCamApp
dotnet build -f net10.0-android
```

Key compile checks:
1. `using CommunityToolkit.Maui.Views;` for `CameraView`
2. `using CommunityToolkit.Maui.Core;` for `CameraInfo`, `CameraPosition`, `CameraFlashMode`
3. `CameraPosition.Rear` (NOT `Back`) for rear camera selection
4. `CameraFlashMode` (NOT `FlashMode`) for flash control
5. `SelectedCamera` (NOT `Camera`) for setting active device
6. Android-specific code wrapped in `#if ANDROID`
7. `MediaCapturedEventArgs.Media` is the captured photo Stream
8. `StopVideoRecording(CancellationToken)` returns `Task<Stream>`

---

## TODO / INCOMPLETE FEATURES

- [ ] Test camera + video recording on physical Android device
- [x] Flash control — UI toggle in top toolbar, icon path, yellow when on / slash when off
- [x] Zoom control — 5 preset pills (.6×, 1×, 2, 3, 10) overlaying bottom of preview
- [ ] Map overlay — not implemented yet
- [ ] Weather API verification — Open-Meteo integrated, untested on device
- [ ] Upgrade Target SDK to API 36 before Aug 2026 Google Play deadline
- [x] Migrated from Camera.MAUI 1.5.1 to CommunityToolkit.Maui.Camera 6.0.0
- [x] SensorValueViewModel rewired — subscribes to SensorHelper, owns SensorItems
- [ ] Verify CommunityToolkit.Maui.Camera 6.0.0 net10.0 compatibility; upgrade if needed
- [ ] iOS support — scaffold only, not targeted
- [x] Premium Samsung-style camera UI — shutter ring+circle, flip path icon, grid lines, mode dots
- [x] Shutter button — vector white ring + white circle (ph