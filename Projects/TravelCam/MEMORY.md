# MEMORY.md — TravelCam Project State & Library API Reference

> **READ THIS AT THE START OF EVERY SESSION.**
> Pinned versions, verified API names, and architecture decisions are documented here.
> Wrong library API assumptions have caused repeated bugs — always cross-check here first.

---

## Locked Library Versions

| Component | Version | Notes |
|---|---|---|
| .NET MAUI | **10.0.30** | Pinned in .csproj |
| Target Framework | **`net10.0-android`** | Primary; Windows is secondary |
| Android Min SDK | **API 29** (Android 10) | Scoped storage with `IsPending` |
| Android Target SDK | **API 35** (Android 15) | Required until Aug 2026 |
| CommunityToolkit.Maui | **14.0.0** | Converters, behaviors, toolkit base |
| CommunityToolkit.Maui.Camera | **6.0.0** | Camera capture — MIGRATED FROM Camera.MAUI 1.5.1 |
| System.Text.Json | **10.0.2** | Via .NET 10 |

> ⚠️ Camera library was **migrated** from `Camera.MAUI 1.5.1` to `CommunityToolkit.Maui.Camera 6.0.0`.
> Old Camera.MAUI API (CameraResult, FlashMode.Enabled, CamerasLoaded event) is **GONE**.
> Use only the API documented below.

---

## CommunityToolkit.Maui.Camera 6.0.0 — VERIFIED API

### MauiProgram Setup
```csharp
builder.UseMauiCommunityToolkit()
       .UseMauiCommunityToolkitCamera()
```

### Namespaces
```csharp
using CommunityToolkit.Maui.Views;   // CameraView
using CommunityToolkit.Maui.Core;    // CameraInfo, CameraPosition, CameraFlashMode
```

### XAML Namespace
```xml
xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
<!-- <toolkit:CameraView ... /> -->
```

### Key Types — EXACT NAMES

```csharp
// Camera device info
public class CameraInfo {
    string Name { get; }
    CameraPosition Position { get; }   // Front or Rear — NOT "Back"
    // NOTE: zoom bounds are NOT reliably exposed on CameraInfo in v6.0.0.
    // Do NOT read MinimumZoomFactor/MaxZoomFactor from CameraInfo at runtime.
    // Manage zoom preset ranges in the ViewModel using known safe values.
}

// Camera position enum — "Rear" NOT "Back"
public enum CameraPosition { Front, Rear }

// Flash enum — "CameraFlashMode" NOT "FlashMode"
public enum CameraFlashMode { Off, On }
// (NOT Enabled/Disabled — those were Camera.MAUI names)
```

### CameraView Properties
```csharp
CameraInfo?      SelectedCamera    { get; set; }   // NOT "Camera" — set BEFORE StartCameraPreview
float            ZoomFactor        { get; set; }   // ABSOLUTE value (2.0 = 2×), NOT normalized 0–1
CameraFlashMode  CameraFlashMode   { get; set; }   // NOT FlashMode
bool             IsAvailable       { get; }
bool             IsBusy            { get; }
```

### CameraView Methods — NO Async suffix on most
```csharp
ValueTask<IReadOnlyList<CameraInfo>> GetAvailableCameras(CancellationToken);
Task StartCameraPreview(CancellationToken);          // starts live preview
void StopCameraPreview();                             // SYNC — no Async suffix, no return value
Task CaptureImage(CancellationToken);                // fires MediaCaptured event; doesn't return Stream
Task StartVideoRecording(CancellationToken);
Task<Stream> StopVideoRecording(CancellationToken);  // returns video stream
```

### CameraView Events
```csharp
event EventHandler<MediaCapturedEventArgs>      MediaCaptured;
event EventHandler<MediaCaptureFailedEventArgs> MediaCaptureFailed;

class MediaCapturedEventArgs {
    Stream Media { get; }   // photo data — save BEFORE returning from handler
}
class MediaCaptureFailedEventArgs {
    string FailureReason { get; }
}
```

### CRITICAL RULES (each learned from a real bug)

| Rule | Wrong | Correct |
|---|---|---|
| Camera position enum | `CameraPosition.Back` | `CameraPosition.Rear` |
| Flash mode enum type | `FlashMode` | `CameraFlashMode` |
| Flash mode values | `FlashMode.Enabled / Disabled` | `CameraFlashMode.On / Off` |
| Active camera property | `Camera` | `SelectedCamera` |
| ZoomFactor semantics | normalized 0–1 | set directly; caller controls valid range |
| Camera init trigger | `CamerasLoaded` event | `OnAppearing` → `await ViewModel.OnViewReady(CameraView)` |
| Stop preview | `StopCameraAsync()` (returns Task) | `StopCameraPreview()` (void, sync) |
| Photo capture | returns Stream | fires `MediaCaptured` event with `Stream Media` |
| Toggle camera | change property, auto-restarts | must call Stop, set SelectedCamera, call Start |
| After StopVideoRecording | done | MUST call `StartCameraPreview` or screen stays black |
| `IsPreviewing` property | exists | does NOT exist — track manually |
| `IsRecording` property | exists | does NOT exist — track manually |

---

## XAML Patterns Used in This Project

### RelativeSource binding inside DataTemplate (to reach parent ViewModel)
```xml
<!-- Inside CollectionView DataTemplate where DataContext is OverlayItem,
     but we need MainPageViewModel.DataOverlayViewModel.LabelFontSize -->
<Label FontSize="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}},
                           Path=BindingContext.DataOverlayViewModel.LabelFontSize}" />
```

### BindableLayout for dynamic item strip
```xml
<HorizontalStackLayout BindableLayout.ItemsSource="{Binding ZoomPresets}">
    <BindableLayout.ItemTemplate>
        <DataTemplate x:DataType="models:ZoomPreset">
            <Border Background="{Binding PillBackground}">
                <Label Text="{Binding Label}" />
            </Border>
        </DataTemplate>
    </BindableLayout.ItemTemplate>
</HorizontalStackLayout>
```

### Path element for vector icons (not BoxView + Rotation)
```xml
<!-- Diagonal slash line — use Path, not a rotated BoxView -->
<Path Data="M8,8 L36,36"
      Stroke="White" StrokeThickness="2"
      WidthRequest="44" HeightRequest="44" />
```

### Rule-of-thirds grid (no margin — overlaid in same Grid cell as CameraView)
```xml
<!-- Place in same Grid cell as CameraView; no Margin; InputTransparent="True" -->
<Grid ColumnDefinitions="*,*,*" RowDefinitions="*,*,*" InputTransparent="True"
      IsVisible="{Binding CameraSettings.ShowRuleOfThirds}">
    <BoxView Grid.Column="1" Grid.RowSpan="3" WidthRequest="0.5"
             Color="{Binding CameraSettings.GridLineColor}"
             HorizontalOptions="Start" />
    <!-- ... more BoxViews ... -->
</Grid>
```

### AbsoluteLayout overlay panel (right-anchored)
```xml
<AbsoluteLayout.LayoutBounds>1, 0, 300, 1</AbsoluteLayout.LayoutBounds>
<AbsoluteLayout.LayoutFlags>PositionProportional, SizeProportional</AbsoluteLayout.LayoutFlags>
```

---

## Android Storage (API 29+)

- Write to `ExternalCacheDir/captures/` (app-private temp) first
- Copy to MediaStore with `IsPending=1`, then set `IsPending=0`
- **CRITICAL**: Save private thumbnail copy BEFORE calling MediaStore publish (which deletes the temp)
- Thumbnail: `FileSystem.AppDataDirectory/last_thumb.jpg` — plain file, never `content://`
- Load thumbnail: `ImageSource.FromFile(path)` — never `ImageSource.FromUri("content://...")`
- Persist thumbnail path in `Preferences.Set("LastThumbPath", thumbPath)`

---

## FileHelper Return Type

```csharp
// SavePhotoAsync returns a TUPLE — not just a string
Task<(string GalleryPath, string ThumbPath)> SavePhotoAsync(Stream, string city)
// Usage:
var (galleryPath, thumbPath) = await FileHelper.SavePhotoAsync(stream, city);
```

---

## ZoomPreset Model

```csharp
// AbsoluteZoom — actual camera zoom factor (e.g., 2.0 = 2×)
// Populated from CameraInfo.MinZoomFactor / MaxZoomFactor at runtime
public class ZoomPreset : INotifyPropertyChanged {
    string Label { get; }            // "1×", "2×", etc.
    float AbsoluteZoom { get; }      // passed directly to CameraView.ZoomFactor
    bool IsSelected { get; set; }    // drives PillBackground, LabelColor, LabelSize
    ICommand SelectCommand { get; }  // closure: () => onSelect(this)
    Color PillBackground { get; }    // white when selected, transparent when not
    Color LabelColor { get; }        // black when selected, white when not
    double LabelSize { get; }        // 13 when selected, 12 when not
}
```

---

## Dependency Injection (MauiProgram.cs)

```csharp
// Singletons — shared for entire app lifetime
builder.Services.AddSingleton<SensorHelper>();
builder.Services.AddSingleton<CameraSettingsViewModel>();

// Transient — new instance per injection
builder.Services.AddTransient<DataOverlayViewModel>();
builder.Services.AddTransient<OverlaySettingsViewModel>();
builder.Services.AddTransient<MainPageViewModel>();
builder.Services.AddTransient<MainPage>();
```

---

## Open-Meteo Weather API

- **Free, no API key**
- URL: `https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true`
- Response: `current_weather.temperature` (°C), `current_weather.windspeed` (km/h)

## Reverse Geocoding

- `await Geocoding.Default.GetPlacemarksAsync(lat, lon)`
- `Placemark.Locality` = city, `Placemark.CountryName` = country
- No API key needed

---

## Current Project File Tree

```
TravelCamApp/
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── MauiProgram.cs                             # DI: SensorHelper+CameraSettingsViewModel singletons
├── TravelCamApp.csproj
│
├── Converters/
│   └── CaptureModeConverters.cs              # FlashIconColorConverter, CaptureModeToTextConverter, etc.
│
├── Helpers/
│   ├── CameraHelper.cs                       # CommunityToolkit.Maui.Camera 6.0.0 wrapper
│   ├── FileHelper.cs                         # MediaStore integration; returns (GalleryPath, ThumbPath)
│   ├── SensorHelper.cs                       # GPS+Compass+Weather polling (10s), singleton
│   └── SettingsHelper.cs                     # JSON sensor settings persistence
│
├── Models/
│   ├── SensorData.cs
│   ├── OverlayItem.cs                         # Observable display item (Name, Value, IsVisible)
│   └── ZoomPreset.cs                         # Dynamic zoom pill model with ICommand closure
│
├── Platforms/Android/
│   ├── AndroidManifest.xml                   # Camera, Mic, Location, Storage permissions
│   └── MainActivity.cs
│
├── ViewModels/
│   ├── CameraSettingsViewModel.cs            # ShowRuleOfThirds, ShowSensorOverlay, GridLineOpacity
│   ├── MainPageViewModel.cs                  # Main coordinator (camera, sensors, commands, lifecycle)
│   ├── OverlaySettingsViewModel.cs       # Visible/Available sensor lists + FontSize slider
│   └── DataOverlayViewModel.cs              # OverlayItems, FontSize, LabelFontSize, ValueFontSize
│
└── Views/
    ├── CameraSettingsView.xaml/cs            # Right-side camera settings overlay panel
    ├── MainPage.xaml/cs                      # Main camera UI
    ├── OverlaySettingsView.xaml/cs       # Dark sensor settings slide-in panel
    └── DataOverlayView.xaml/cs              # Sensor data overlay (FontSize bound to DataOverlayViewModel)
```

---

## Known Issues / TODO

| Priority | Item | Status |
|---|---|---|
| HIGH | Test camera on physical Android device | Not tested yet |
| HIGH | Upgrade Target SDK to API 36 before Aug 2026 | Planned |
| MEDIUM | Weather API — Open-Meteo integrated, untested on device | Untested |
| MEDIUM | Map overlay | Not started |
| LOW | iOS support | Scaffold only |
| LOW | Video recording on physical device | Code complete, untested |

## Changelog

| Date | Change |
|---|---|
| 2026-04-01 | Migrated camera library: Camera.MAUI 1.5.1 → CommunityToolkit.Maui.Camera 6.0.0 |
| 2026-04-01 | Added dynamic zoom presets (ZoomPreset model, ObservableCollection, BindableLayout) |
| 2026-04-01 | Added CameraSettingsViewModel + CameraSettingsView (rule of thirds, sensor o