# CLAUDE.md — TravelCam MAUI Camera App

> This file configures Claude Code when working on the TravelCam project.
> Read it on every task before making changes.

---

## REQUIRED SKILLS — Load Before Every Task

The following skills from `~/.claude/commands/` apply to this project. Always consult them before writing or modifying code.

| Skill | When to use |
|---|---|
| `/maui-current-apis` | **Always first** — deprecated API guardrail |
| `/maui-app-lifecycle` | Any camera/sensor lifecycle, Window events, ANR prevention |
| `/maui-data-binding` | Any XAML binding, `x:DataType`, `ObservableCollection` |
| `/maui-collectionview` | Gallery thumbnail strip, any `CollectionView` changes |
| `/maui-permissions` | Camera, mic, location, storage permission flows |
| `/maui-geolocation` | GPS calls — always pass `CancellationToken` |
| `/maui-file-handling` | MediaStore, file paths, `content://` URIs |
| `/maui-platform-invoke` | `#if ANDROID`, partial classes, `Platform.CurrentActivity` |
| `/maui-performance` | Compiled bindings, layout nesting, image sizing |
| `/maui-safe-area` | `.NET 10` edge-to-edge, `SafeAreaEdges` |
| `/maui-dependency-injection` | DI lifetimes, singleton vs transient, registration |
| `/maui-gestures` | Tap/swipe/pan recognizers, `.NET 10` deprecations |

---

## PROJECT OVERVIEW

TravelCam is a .NET MAUI camera application for Android (API 29+) that captures photos and video with real-time sensor data overlays (location, temperature, compass). It follows MVVM architecture with dependency injection.

### Tech Stack

| Component | Version |
|---|---|
| .NET MAUI | 10.0.41 |
| Target Framework | `net10.0-android` (also `net10.0-windows10.0.19041.0`) |
| Android Min SDK | API 29 (Android 10) |
| Android Target SDK | API 36 (Android 16) |
| CommunityToolkit.Maui | 14.1.0 |
| CommunityToolkit.Maui.Camera | 6.0.1 |
| CommunityToolkit.Maui.MediaElement | 8.0.1 |

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
// Singletons — shared across app lifetime
builder.Services.AddSingleton<SensorHelper>();
builder.Services.AddSingleton<CameraSettingsViewModel>();
builder.Services.AddSingleton<DataOverlayViewModel>();

// Transient — new instance per injection
builder.Services.AddTransient<OverlaySettingsViewModel>();
builder.Services.AddTransient<MainPageViewModel>();
builder.Services.AddTransient<MainPage>();
```

### Camera Lifecycle
1. `OnAppearing()` → `await ViewModel.OnViewReady(CameraView)` initializes camera
2. ViewModel calls `CameraHelper.SelectFirstAvailableCamera()` then `StartCameraPreview()`
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
3. After write completes, `IsPending=0` — **temp file is retained** (not deleted) for in-app viewing
4. Gallery uses file paths directly with `ImageSource.FromStream()` via `FilePathToImageSourceConverter`
5. Thumbnail preview uses plain file path from `AppDataDirectory/last_thumb.jpg`

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
| Camera.MAUI 1.5.1 | RESOLVED | Migrated to CommunityToolkit.Maui.Camera 6.0.1 |

## KEY FILES

| File | Description |
|---|---|
| `MauiProgram.cs` | DI registration (SensorHelper singleton, ViewModels, Pages) |
| `MainPageViewModel.cs` | Main coordinator — permissions, camera, sensors, lifecycle |
| `MainPage.xaml` | Camera UI with permission overlay, sensor overlay, mode selector |
| `MainPage.xaml.cs` | OnAppearing camera init, MediaCaptured routing, settings overlay |
| `CameraHelper.cs` | Static CommunityToolkit.Maui.Camera 6.0.0 wrapper |
| `SensorHelper.cs` | GPS+Compass+Weather polling (10s), IDisposable |
| `FileHelper.cs` | MediaStore gallery publishing + cache management |
| `SettingsHelper.cs` | JSON persistence for sensor settings |
| `SensorData.cs` | Sensor data model (raw GPS/weather/compass data from SensorHelper) |
| `OverlayItem.cs` | Observable display item (Name, Value, IsVisible) — shown in camera overlay |
| `OverlaySettingsViewModel.cs` | Manages VisibleOverlayItems / AvailableOverlayItems lists |
| `DataOverlayViewModel.cs` | Owns OverlayItems, subscribes to SensorHelper, LabelFontSize/ValueFontSize |
| `ImageViewerView.xaml` | Full-screen gallery viewer with carousel + thumbnail strip |
| `ImageViewerView.xaml.cs` | Gallery navigation, sharing, delete operations |
| `FilePathToImageSourceConverter.cs` | Converts file paths to `ImageSource` using `FromStream()` for reliable loading from app cache |
| `Models/PhotoCaptureMetadata.cs` | Snapshot of sensor data collected at capture time — written into JPEG EXIF |
| `Models/MediaInfo.cs` | EXIF metadata read back from a captured file — displayed in gallery info panel |
| `Platforms/Android/ExifHelper.cs` | Android-only JPEG EXIF read/write. `ApplyMetadata` writes tags; `ReadMetadata` returns `MediaInfo` |

---

## GALLERY IMAGE BINDING PATTERN

### Loading Images from App Cache Directory

The gallery (`ImageViewerView`) loads images from the app's private cache directory (`ExternalCacheDir/captures/`). Use this pattern:

**1. XAML: Add converter resource and System namespace**
```xml
xmlns:sys="clr-namespace:System;assembly=netstandard"
...
<ContentView.Resources>
    <converters:FilePathToImageSourceConverter x:Key="FilePathToImageSourceConverter" />
</ContentView.Resources>
```

**2. DataTemplate: Override inherited x:DataType**
```xml
<!-- If parent has x:DataType="vm:MainPageViewModel", explicitly set item type -->
<CarouselView.ItemTemplate>
    <DataTemplate x:DataType="sys:String">
        <Image Source="{Binding ., Converter={StaticResource FilePathToImageSourceConverter}}"
               Aspect="AspectFit" />
    </DataTemplate>
</CarouselView.ItemTemplate>
```

**⚠️ Critical: Compiled Bindings Inheritance**
- Root element's `x:DataType` is inherited by child `DataTemplate` elements
- Without `x:DataType="sys:String"`, `{Binding .}` binds to the **ViewModel**, not the string item
- Result: converter receives ViewModel object → returns null → blank images

**3. Converter Implementation (FilePathToImageSourceConverter.cs)**
```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is string filePath && File.Exists(filePath))
    {
        return ImageSource.FromStream(() =>
            new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }
    return null;
}
```

**Why not `file://{0}` URI or `ImageSource.FromFile()`?**
- `file://` URIs don't reliably resolve to app-private `ExternalCacheDir` on Android
- `FromFile()` has limited support for app-specific directories
- `FromStream()` with direct file path is the most reliable approach

**4. ViewModel: Populate GalleryImagePaths**
```csharp
public List<string> GalleryImagePaths { get; set; } = new();
// Set to list of file paths from FileHelper.GetAllGalleryMediaPaths()
```

---

## WHAT TO CHECK BEFORE BUILDING

When asked to build or fix, run:
```bash
cd C:\Users\yoyok\Git\Playground\Projects\TravelCam\TravelCamApp
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

## RECENT FIXES (2026-04-16)

### 1. Sensor Overlay + Settings Panel in ImageViewerView (Gallery)
**Files:** `Views/ImageViewerView.xaml`, `Views/ImageViewerView.xaml.cs`, `Views/MainPage.xaml`, `Views/MainPage.xaml.cs`, `ViewModels/MainPageViewModel.cs`

- `DataOverlayView` (sensor pill) added to gallery — positioned at bottom-right of the **actual displayed image**, not the container. Margin is computed at runtime via `UpdateOverlayPosition()`.
- `OverlaySettingsView` added to gallery — same settings panel as camera view, shared `OverlaySettingsViewModel` instance.
- `IsGallerySettingsVisible` added to `MainPageViewModel` — dedicated flag for gallery settings overlay, independent of camera's `IsSettingsVisible`.
- `OpenSettingsCommand` is now context-aware: sets `IsGallerySettingsVisible` when gallery is open, `IsSettingsVisible` otherwise.
- Wired from `MainPage.xaml.cs` via `GalleryView.WireSensorSettings(_sensorSettingsVm, viewModel)`.

**⚠️ Critical pattern: BindingContext override breaks compiled XAML bindings.**
`WireSensorSettings` sets `SensorSettingsOverlay.BindingContext = OverlaySettingsViewModel`.
Any `{Binding IsGallerySettingsVisible}` on that element is compiled against `MainPageViewModel` but runtime context becomes `OverlaySettingsViewModel` → binding mismatch → overlay stuck.
**Fix:** `IsVisible="False"` in XAML; drive from code via `mainVm.PropertyChanged` subscription in `WireSensorSettings`.

### 2. Overlay Position Respects EXIF Orientation
**File:** `Views/ImageViewerView.xaml.cs` — `UpdateOverlayPosition()`

`BitmapFactory.DecodeFile` with `InJustDecodeBounds` returns raw stored dimensions, ignoring EXIF rotation. A portrait photo stored landscape (e.g. 4000×3000, Orientation=6) gives aspect 1.33 instead of 0.75 → wrong margin → pill lands in black bars.
**Fix:** After `DecodeFile`, read `ExifInterface.TagOrientation`; if orientation ≥ 5 (values 5–8 = any 90°/270° rotation), swap imageW and imageH before aspect ratio math.

```csharp
using var exif = new Android.Media.ExifInterface(filePath);
int orientation = exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation,
    (int)Android.Media.Orientation.Normal);
if (orientation >= 5)
    (imageW, imageH) = (imageH, imageW);
```

### 3. Gallery Crash on Rapid Thumbnail Taps — Fixed
**File:** `Views/ImageViewerView.xaml.cs` — `OnCarouselPositionChanged`

`async void` + debounce: `_scrollDebounceCancel.Cancel()` caused previous `await Task.Delay(..., token)` to throw `OperationCanceledException` with no handler → unhandled exception on UI thread → **crash**.
**Fix:** Added `catch (OperationCanceledException)` — expected cancellation, silently ignored.
Moved `StopSharedVideoPlayer()`, `IsMediaInfoVisible=false`, and `UpdateOverlayPosition()` inside the `try` block so they only run when debounce settles.

**Rule:** Any `async void` using debounce cancellation MUST catch `OperationCanceledException`.

### 4. Camera Container Layout — Always Uses Native Resolution
**File:** `Views/MainPage.xaml.cs` — `ApplyCameraLayout()`

`CameraViewChildrenContainer` was sized using the user-selected capture resolution instead of the camera's native (largest) resolution. The live preview always streams the full sensor feed at its native aspect ratio, so the capture resolution setting must not affect the layout.
**Fix:** Always use `SupportedResolutions.OrderByDescending(pixel count).First()` for the layout calculation.

---

## RECENT FIXES (2026-04-15)

### 1. EXIF Metadata Write on Photo Capture
**Files:** `Platforms/Android/ExifHelper.cs` (new), `Models/PhotoCaptureMetadata.cs` (new), `MainPageViewModel.cs`

- `BuildPhotoCaptureMetadata()` snapshots current sensor state (GPS, temperature, heading, speed, city, country, flash, aspect ratio, resolution) into a `PhotoCaptureMetadata` at capture time.
- `ExifHelper.ApplyMetadata(stream, meta)` writes all standard EXIF tags (GPS, date, device, flash) plus a `JSON:` prefixed `UserComment` payload for custom fields.
- **⚠️ Heavy I/O — must be on background thread.** Both `CropStreamToAspectRatio` and `ApplyMetadata` are combined in a single `await Task.Run(...)` inside `OnMediaCaptured`. Never call these on the main thread.

### 2. Gallery Info Panel ("i" button)
**Files:** `Models/MediaInfo.cs` (new), `MainPageViewModel.cs`, `Views/ImageViewerView.xaml`

- `ToggleMediaInfoCommand` shows/hides an info overlay on the current gallery image.
- `ExifHelper.ReadMetadata(path)` reads tags back and populates a `MediaInfo` object — also wrapped in `await Task.Run(...)` (disk I/O on tap).
- `_isLoadingMediaInfo` guard prevents double-tap launching concurrent reads.
- `MediaInfo` exposes computed `HasCameraInfo`, `HasLocationInfo`, `HasConditionsInfo` for section visibility.

### 3. Gallery Delete-then-Click Crash — Fixed
**File:** `MainPageViewModel.cs`

- After deleting the last item, `GalleryImagePaths` becomes empty → `CurrentImageIndex` clamp + null check guards prevent indexing into an empty list → no crash.

### 4. Main Thread Freeze — Fixed
**File:** `MainPageViewModel.cs`

- `ExifInterface` write + re-read (~500–2000 ms) and bitmap decode/rotate were running on the main thread → ANR/freeze.
- Fix: single `await Task.Run(...)` wrapping both `CropStreamToAspectRatio` + `ExifHelper.ApplyMetadata` in `OnMediaCaptured`; separate `await Task.Run(...)` for `ExifHelper.ReadMetadata` in `ExecuteToggleMediaInfo`.

### 5. Aspect Ratio Crop Bugs — 3-Part Fix
**Files:** `MainPageViewModel.cs`, `MainPage.xaml.cs`, `MainPage.xaml`

- **Bug A (1:1 rotated 90°):** `BitmapFactory.DecodeStream` ignores EXIF orientation. Fix: buffer → temp file → read `TagOrientation` → `Matrix.PostRotate()` → crop upright bitmap.
- **Bug B (16:9 landscape band in portrait):** `SixteenNine => 9.0/16.0` gave `r = 0.5625` → landscape crop. Fix: `isPortrait ? 16.0/9.0 : 9.0/16.0`.
- **Bug C (3:4 = 9:16 on 4:3 sensors):** `desiredH > naturalH` was clamped. Fix: pillarbox bars (`AspectLeftBarWidth`, `AspectRightBarWidth`, `HasAspectSideBars` + two `BoxView` in XAML).

### 6. Gallery Crash (SelectionChanged re-entrancy) — Fixed
**File:** `ImageViewerView.xaml.cs`

- `_isSyncingThumbnail` bool flag blocks re-entrant calls; `if (!IsVisible)` guard discards binding-driven events when gallery panel is hidden.

### 7. Camera Layout Refactor
**File:** `MainPage.xaml`

- Top toolbar removed; Flash, Sensor, Camera Settings moved inside `CameraViewChildrenContainer` as right-edge `VerticalStackLayout`. Recording timer remains centered, visible only when `IsRecording`.

---

## CRITICAL PATTERNS

### Heavy I/O Must Run on Background Thread
`ExifInterface`, `BitmapFactory.DecodeStream`, file writes — all block for 500–2000 ms on mobile. **Always wrap in `await Task.Run()`.**

```csharp
// Capture — crop + EXIF write in one hop
stream = await Task.Run(() =>
{
    var cropped = CropStreamToAspectRatio(inStream, ratio);
    return (Stream)ExifHelper.ApplyMetadata(cropped, meta);
});

// Info tap — EXIF read
var info = await Task.Run(() => ExifHelper.ReadMetadata(path));
```

Use a guard bool (`_isLoadingMediaInfo`) to prevent double-tap launching concurrent reads.

---

## PRIOR FIXES (2026-04-12) — Camera Reopen Crash

> **Complete solution in master memory file:**
> `~/.claude/projects/[...]/memory/MASTER-XAML-AND-LIFECYCLE-CRASH-FIX-2026-04-12.md`

**Status:** ✅ Implemented & tested on real Android devices.

Summary of 5-part fix:
1. Inverted `_isAppInitialized` flag (false by default, set true after init)
2. Removed `while (!_isAppInitialized)` wait loop from `OnViewReady`
3. `linker.xml` preserving CommunityToolkit.Maui.Camera types from Release linker
4. `OnDisappearing()` stops camera preview to release CameraView resources
5. 9 `CameraHelper` methods wrapped in `MainThread.BeginInvokeOnMainThread()`
6. Removed invalid `SafeAreaEdges="..."` XAML attributes

**Temporary workaround:** Custom fonts disabled in `MauiProgram.cs` (font assets not deploying to APK). App uses system sans-serif. TODO: fix font deployment.

---

## TODO / INCOMPLETE FEATURES

- [ ] **Test camera + video recording on physical Android device**
- [ ] **Fix font asset deployment** — currently disabled; app uses system fonts
- [ ] **Test Release build** — verify linker.xml prevents code stripping
- [ ] Map overlay — not implemented yet
- [ ] Weather API verification — Open-Meteo integrated, untested on device
- [ ] Upgrade Target SDK to API 36 before Aug 2026 Google Play deadline
- [ ] Verify CommunityToolkit.Maui.Camera 6.0.1+ compatibility; check for upgrades
- [ ] iOS support — scaffold only, not targeted
- [x] **EXIF metadata write** — GPS, date, device, flash + JSON UserComment payload written on every capture (2026-04-15)
- [x] **Gallery info panel** — "i" button shows EXIF data overlay; async `ReadMetadata` with guard (2026-04-15)
- [x] **Gallery delete crash** — empty list guard after deleting last item (2026-04-15)
- [x] **Main thread freeze** — crop + EXIF write and EXIF read both moved to `Task.Run` (2026-04-15)
- [x] **Gallery stability** — SelectionChanged re-entrancy crash + hidden-view ScrollTo crash (2026-04-15); rapid thumbnail tap crash via OperationCanceledException in async void (2026-04-16)
- [x] **Aspect ratio crop** — EXIF rotation, 16:9 portrait preview, pillarbox bars (2026-04-15)
- [x] **Camera layout** — Flash, Sensor, Camera Settings inside CameraViewChildrenContainer right column (2026-04-15); container sized from native resolution not capture resolution (2026-04-16)
- [x] **Gallery sensor overlay** — DataOverlayView pill + OverlaySettingsView in ImageViewerView; position computed from EXIF-corrected aspect ratio (2026-04-16)
- [x] Flash control — icon path, yellow when on / slash when off
- [x] Zoom control — 5 preset pills (.6×, 1×, 2, 3, 10) overlaying bottom of preview
- [x] Migrated from Camera.MAUI 1.5.1 to CommunityToolkit.Maui.Camera 6.0.1
- [x] DataOverlayViewModel rewired — subscribes to SensorHelper, owns OverlayItems
- [x] Premium Samsung-style camera UI — shutter ring+circle, flip path icon, grid lines, mode dots
- [x] Camera reopen crash — 5-part fix (init flag, wait loop, linker, cleanup, MainThread wrapping)