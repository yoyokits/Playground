# MEMORY.md — Project State & Version Lock

> Claude Code: Read this file at start of every session to understand the current project state, version constraints, and API compatibility requirements.

---

## VERSION LOCK

| Component | Version | Constraint |
|---|---|---|
| .NET MAUI | 10.0.30 | Pinned in .csproj (`<MauiVersion>`) |
| Target Framework | `net10.0-android` | Primary; Windows secondary |
| Android Min SDK | API 29 | Android 10 — MediaStore with `RelativePath` available |
| Android Target SDK | API 35 | Android 15 |
| Camera.MAUI | 1.5.1 | Pinned in .csproj |
| CommunityToolkit.Maui | 14.0.0 | Pinned in .csproj |
| System.Text.Json | 10.0.2 | Via .NET 10 |
| HttpClient | Built-in | .NET 8+ default handler |

**IMPORTANT**: All code must be compatible with these exact versions. Do NOT generate code for different versions unless explicitly asked.

---

## Camera.MAUI 1.5.1 — Authoritative API

Source verified from https://github.com/hjam40/Camera.MAUI (master branch).

### Types (namespace: `Camera.MAUI`)

```
CameraInfo
  Properties: Name, DeviceId, Position, HasFlashUnit,
              MinZoomFactor, MaxZoomFactor,
              HorizontalViewAngle, VerticalViewAngle, AvailableResolutions
  ToString() -> Name

CameraPosition: Back | Front | Unknow  (note: "Unknow" not "Unknown" — library typo)

CameraResult: Success | AccessDenied | NoCameraSelected | AccessError
              | NoVideoFormatsAvailable | NotInitiated
              | NoMicrophoneSelected | ResolutionNotAvailable

FlashMode: Auto | Enabled | Disabled

ImageFormat: JPEG | PNG | WEBP

MicrophoneInfo
  Properties: Name, DeviceId
  ToString() -> Name
```

### CameraView Bindable Properties

```
Cameras              -> ObservableCollection<CameraInfo>
CamerasLoaded        -> event EventHandler
NumCamerasDetected   -> int

Microphones          -> ObservableCollection<MicrophoneInfo>
MicrophonesLoaded    -> event EventHandler

Camera               -> CameraInfo  (MUST be set before StartCameraAsync)
Microphone           -> MicrophoneInfo  (MUST be set before StartRecordingAsync)

FlashMode            -> FlashMode (default: Disabled)
ZoomFactor           -> float (default: 1.0f)
TorchEnabled         -> bool (default: false)
MirroredImage        -> bool (default: false)

SnapShot             -> ImageSource (OneWayToSource binding)
SnapShotStream       -> Stream (OneWayToSource binding)
AutoSnapShotSeconds  -> float (0 = disabled)
AutoSnapShotFormat   -> ImageFormat
Self                 -> CameraView (OneWayToSource, for MVVM)

AutoStartPreview     -> bool (if true, starts preview automatically)
AutoRecordingFile    -> string (file path for auto-record)
AutoStartRecording   -> bool (if true, starts recording automatically)
```

### Methods (Return Types Matter)

```
Task<CameraResult> StartCameraAsync(Size Resolution = default)
Task<CameraResult> StopCameraAsync()
Task<CameraResult> StartRecordingAsync(string file, Size Resolution = default)
Task<CameraResult> StopRecordingAsync()
Task<Stream>       TakePhotoAsync(ImageFormat = JPEG)
ImageSource        GetSnapShot(ImageFormat = PNG)
Task<bool>         SaveSnapShot(ImageFormat, string filePath)
```

### CRITICAL RULES

1. **`cameraView.Cameras`** — NOT `Devices`
2. **`CameraInfo`** — NOT `CameraDevice` or `CameraDeviceInfo`
3. **`CameraResult`** — NOT `bool`
4. **`FlashMode.Enabled`** — NOT `FlashMode.On`
5. **`FlashMode.Disabled`** — NOT `FlashMode.Off`
6. **NO `IsPreviewing`** property on CameraView
7. **NO `IsRecording`** property on CameraView
8. Camera auto-restarts preview when `Camera` property changes — do NOT call `StartCameraAsync` after toggling
9. **Microphone MUST be set** before `StartRecordingAsync` or it will fail
10. `CamerasLoaded` event is the reliable signal — NOT `OnAppearing`

---

## Current Project File Tree

```
TravelCamApp/
├── App.xaml / App.xaml.cs                    # App shell + converters
├── AppShell.xaml / AppShell.xaml.cs           # Single-page routing
├── MauiProgram.cs                             # DI registration
├── TravelCamApp.csproj                        # Version lock here
├── Requirement.txt                            # Feature requirements
├── CodeStyle.txt                              # Coding standards
├── DebuggingGuide.txt                         # Debug methodology
├── Log.txt                                    # Development log (v0.3.0 current)
├── WeeklyReviewChecklist.txt                  # Weekly review template
│
├── Converters/
│   ├── CaptureModeConverters.cs               # Mode->Text, Color, Font converters
│   ├── NullToBoolConverter.cs                 # (unused, kept for future)
│   └── PositionToOptionsConverter.cs          # (unused, kept for future)
│
├── Helpers/
│   ├── CameraHelper.cs                        # Camera.MAUI static wrapper (API-verified)
│   ├── FileHelper.cs                          # MediaStore gallery integration
│   ├── SensorHelper.cs                        # GPS+Compass+Weather polling (10s)
│   ├── Settings.cs                            # Output path with Android fallback
│   └── SettingsHelper.cs                      # JSON sensor settings persistence
│
├── Models/
│   ├── SensorData.cs                          # Sensor data aggregate
│   └── SensorItem.cs                          # Display item (Name, Value, IsVisible)
│
├── Platforms/
│   ├── Android/
│   │   ├── AndroidManifest.xml                # Permissions (API 29-35)
│   │   ├── MainActivity.cs                    # SingleTop launch
│   │   ├── MainApplication.cs                 # Entry point
│   │   └── Resources/xml/file_paths.xml       # FileProvider config
│   ├── iOS/                                   # Scaffold only
│   ├── MacCatalyst/                           # Scaffold only
│   └── Windows/                               # Scaffold + FileProvider
│
├── ViewModels/
│   ├── MainPageViewModel.cs                   # Main coordinator (camera + sensors + UI state)
│   ├── SensorValueViewModel.cs                # Sensor data bridge (legacy, kept)
│   └── SensorValueSettingsViewModel.cs         # Settings list management
│
├── Views/
│   ├── MainPage.xaml / MainPage.xaml.cs       # Camera UI + permission overlay
│   ├── SensorValueView.xaml / .xaml.cs        # Sensor data overlay (bottom-right)
│   └── SensorValueSettingsView.xaml / .xaml.cs # Settings modal (two lists)
│
└── Resources/
    ├── AppIcon/, Fonts/, Images/, Splash/     # Standard MAUI resources
    └── Styles/Colors.xaml, Styles.xaml        # MD3 color tokens
```

---

## Architecture Decisions

1. **DI via `IServiceCollection`** — registered in `MauiProgram.cs`. No manual `new ViewModel()` except for helper classes.
2. **SensorHelper is a singleton** — single source of truth for all sensor data. No ViewModel should call Geolocation/GPS directly.
3. **Window lifecycle events** — `Window.Resumed` and `Window.Stopped` are used for camera/sensor lifecycle, NOT `Page.OnAppearing/Disappearing`.
4. **MediaStore first** — Files saved to gallery via `MediaStore.Images/Video.Media.ExternalContentUri` with `IsPending` flag (Android 10+). Direct file copies only as fallback.
5. **Fire-and-forget initialization** — Constructor `InitializeAsync()` is wrapped in `SafeInitializeAsync()` to prevent constructor-async crashes.

---

## Known Issues / TODO

| Priority | Item | Status |
|---|---|---|
| HIGH | Test camera on physical Android device | Not tested yet |
| MEDIUM | Flash control (FlashMode + UI) | Helper done, no UI |
| MEDIUM | Zoom control (slider) | Helper done, no UI |
| LOW | Weather API verification | Open-Meteo integrated, untested |
| LOW | Map overlay | Planned, not started |
| LOW | Video recording on device | Code complete, untested |
