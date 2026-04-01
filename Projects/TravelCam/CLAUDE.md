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
| Camera.MAUI | 1.5.1 |
| CommunityToolkit.Maui | 14.0.0 |

### Source

- Camera.MAUI API: https://github.com/hjam40/Camera.MAUI (master branch)

---

## CRITICAL API REFERENCE — Camera.MAUI 1.5.1

**DO NOT** use hypothetical API names. Only use what is documented below or verified from the library source.

### Types in `Camera.MAUI` namespace

```csharp
// Camera info type (NOT CameraDevice, NOT CameraDeviceInfo)
public class CameraInfo {
    string Name { get; }
    string DeviceId { get; }
    CameraPosition Position { get; }  // Back, Front, Unknow
    bool HasFlashUnit { get; }
    float MinZoomFactor { get; }
    float MaxZoomFactor { get; }
    List<Size> AvailableResolutions { get; }
}

// Result enum (NOT bool)
public enum CameraResult {
    Success, AccessDenied, NoCameraSelected, AccessError,
    NoVideoFormatsAvailable, NotInitiated, NoMicrophoneSelected, ResolutionNotAvailable
}

// Flash modes (NOT Off/On)
public enum FlashMode { Auto, Enabled, Disabled }

// Image format
public enum ImageFormat { JPEG, PNG, ... }

// Microphone info
public class MicrophoneInfo {
    string Name { get; }
    string DeviceId { get; }
}
```

### CameraView API (the MAUI control)

```csharp
// COLLECTIONS — use "Cameras" NOT "Devices"
ObservableCollection<CameraInfo> Cameras { get; set; }
ObservableCollection<MicrophoneInfo> Microphones { get; set; }
int NumCamerasDetected { get; set; }
int NumMicrophonesDetected { get; set; }

// ACTIVE SELECTIONS — must be set before operations
CameraInfo Camera { get; set; }           // REQUIRED before StartCameraAsync
MicrophoneInfo Microphone { get; set; }   // REQUIRED before StartRecordingAsync

// METHODS — all return CameraResult, NOT bool
Task<CameraResult> StartCameraAsync(Size Resolution = default);
Task<CameraResult> StopCameraAsync();
Task<CameraResult> StartRecordingAsync(string file, Size Resolution = default);
Task<CameraResult> StopRecordingAsync();
Task<Stream>       TakePhotoAsync(ImageFormat imageFormat = ImageFormat.JPEG);
ImageSource        GetSnapShot(ImageFormat imageFormat = ImageFormat.PNG);
Task<bool>         SaveSnapShot(ImageFormat imageFormat, string SnapFilePath);

// BINDABLE PROPERTIES
CameraView.Self                (CameraView)   // OneWayToSource, MVVM binding
FlashMode FlashMode            (default: Disabled)
float ZoomFactor               (default: 1.0f)
bool TorchEnabled              (default: false)
bool MirroredImage             (default: false)
ImageSource SnapShot           (OneWayToSource)
Stream SnapShotStream          (OneWayToSource)
float AutoSnapShotSeconds      (0 = disabled)

// EVENTS
event EventHandler CamerasLoaded;       // fires when cameras are detected
event EventHandler MicrophonesLoaded;   // fires when microphones are detected
event BarcodeResultHandler BarcodeDetected;
```

### CRITICAL BEHAVIORS

1. **Camera auto-restart on property change**: Camera.MAUI has a `CameraChanged` bindable property callback that automatically restarts the camera when `CameraView.Camera` is changed. After toggling cameras, do NOT call `StartCameraAsync` again — the library handles it.

2. **Microphone required for recording**: `Microphone` property MUST be set before calling `StartRecordingAsync()`, otherwise it fails.

3. **No `IsPreviewing` or `IsRecording` property**: These do NOT exist on `CameraView`. Track state manually in the ViewModel if needed.

4. **`CamerasLoaded` fires once**: This is the reliable signal that the control is ready to use cameras. Listen to this event, not `OnAppearing`.

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

## FILES CHANGED IN REFACTOR

### Rewritten Files
| File | Description |
|---|---|
| `MauiProgram.cs` | DI registration (SensorHelper, ViewModels, Pages) |
| `MainPageViewModel.cs` | Complete rewrite — permission-first, event-driven sensors, lifecycle management |
| `MainPage.xaml` | Clean camera UI with permission overlay |
| `MainPage.xaml.cs` | Minimal — just CamerasLoaded handler and settings overlay toggle |
| `CameraHelper.cs` | Verified against Camera.MAUI 1.5.1 source |
| `SensorHelper.cs` | CancellationToken support, timer lifecycle |
| `AndroidManifest.xml` | Clean permissions |
| `TravelCamApp.csproj` | Android-first targets |
| `Views/SensorValueView.xaml` | Simplified overlay |
| `Views/SensorValueSettingsView.xaml` | Clean settings panel |

### Unchanged (Working Correctly)
| File | Description |
|---|---|
| `Helpers/Settings.cs` | Output path with fallback strategy |
| `Helpers/FileHelper.cs` | MediaStore-based gallery integration |
| `Helpers/SettingsHelper.cs` | JSON persistence for sensor settings |
| `Models/SensorData.cs` | Sensor data structure |
| `Models/SensorItem.cs` | Display item for sensor overlay |
| `Converters/CaptureModeConverters.cs` | Value converters for mode display |
| `App.xaml / App.xaml.cs` | App shell |
| `AppShell.xaml` | Single-page navigation |

---

## WHAT TO CHECK BEFORE BUILDING

When asked to build or fix, run:
```bash
cd C:\Git\Playground\Projects\TravelCam\TravelCamApp
dotnet build -f net10.0-android
```

Key compile checks:
1. All `Camera.MAUI` type names match library (`CameraInfo`, not `CameraDevice`)
2. `Cameras` property used (not `Devices`)
3. `CameraResult` enum used (not `bool`)
4. All namespaces use `using Camera.MAUI;`
5. Android-specific code wrapped in `#if ANDROID`

---

## TODO / INCOMPLETE FEATURES

- [ ] Weather API for temperature — Open-Meteo integrated but untested on device
- [ ] Flash control — method exists, no UI button yet
- [ ] Zoom control — method exists, no UI slider yet
- [ ] Map overlay — not implemented yet
- [ ] Video recording — code complete, untested on device
- [ ] iOS support — scaffold only, not targeted
