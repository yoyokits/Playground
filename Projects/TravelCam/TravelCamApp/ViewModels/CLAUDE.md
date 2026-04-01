# ViewModels — Coding Conventions

> Read before modifying any file in this directory.

---

## Architecture Rules

1. **No UI references** — ViewModels never import `Microsoft.Maui.Controls` types for views.
2. **Commands over event handlers** — All user actions go through `ICommand`.
3. **INotifyPropertyChanged** — All bindable properties must notify on change.
4. **Constructor injection** — Dependencies received from DI container (MauiProgram.cs).
5. **Fire-and-forget safety** — Async calls in constructors wrapped in try/catch helper.

## MainPageViewModel.cs
- Single coordinator for camera, sensors, and UI state.
- Subscribes to `SensorHelper.SensorDataUpdatedCallback` — the only place sensor data enters the UI.
- Manages `Window.Resumed`/`Window.Stopped` lifecycle events for camera/sensor restart.
- `_isDestroyed` flag prevents callbacks after window cleanup.
- Camera initialized only after `CamerasLoaded` event from the view.

## SensorValueViewModel.cs
- Bridge ViewModel for the SensorValueView overlay.
- Subscribes to SensorHelper updates (legacy — some logic merged into MainPageViewModel now).

## SensorValueSettingsViewModel.cs
- Manages two lists: VisibleSensorItems and AvailableSensorItems.
- `LoadFromSensorItems()` populates lists when settings opens.
- `ApplyToSensorItems()` writes visibility state back to source.
- `SaveSettingsAsync()` persists to JSON via SettingsHelper.

---

## Key Patterns

### Property Change Notification
```csharp
private bool _isRecording;
public bool IsRecording {
    get => _isRecording;
    set { _isRecording = value; OnPropertyChanged(); }
}
```

### Commands
```csharp
public ICommand CaptureCommand { get; }
// In constructor:
CaptureCommand = new Command(async () => await CaptureAsync());
```

### Sensor Updates from SensorHelper
```csharp
private void OnSensorDataUpdated(SensorData data) {
    MainThread.BeginInvokeOnMainThread(() => {
        UpdateSensorItem("Temperature", $"{data.Temperature:F1}°C");
        // ... update all sensor items
    });
}
```
