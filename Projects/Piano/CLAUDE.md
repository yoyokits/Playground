# Piano App - Development Guidelines

## Current Status (2026-03-23)
⚠️ **Build Blocked** - The app has compilation errors preventing runtime testing. See "Known Build Issues" below.

## Project Overview
.NET MAUI Piano App with MVVM pattern. Features:
- Piano sheet music display (Full Sheet mode & Play mode with green highlight)
- Professional piano keys (3 octaves: C3-B5) rendered with SkiaSharp
- Synthesized audio using NAudio
- Load/play JSON-formatted sheet music

## Tech Stack
- **Framework**: .NET 10.0 / .NET MAUI
- **MVVM**: CommunityToolkit.Mvvm
- **Graphics**: SkiaSharp
- **Audio**: NAudio (wave generation)
- **Platforms**: Android, iOS, Windows

## Coding Conventions
- **Naming**: PascalCase for classes/methods, camelCase for locals/params, _camelCase for private fields
- **Async**: Use async/await, suffix async methods with `Async`
- **Properties**: Auto-properties preferred; use INotifyPropertyChanged via [ObservableProperty]
- **Commands**: Use IRelayCommand/IAsyncRelayCommand from CommunityToolkit
- **Files**: One class per file; filename matches class name
- **Regions**: Avoid #region; keep files under 300 lines

## Architecture
- **Models**: Plain C# classes, no dependencies on MAUI/SkiaSharp
- **Services**: Singletons registered in MauiProgram; platform-agnostic interfaces
- **ViewModels**: Observable properties + relay commands; no direct view references
- **Views**: .xaml + code-behind minimal; use binding contexts set automatically viaViewModelLocator (if implemented) or manually

## Project Structure
```
Piano/
├── Models/
│   ├── Note.cs
│   ├── MusicSheet.cs
│   └── NoteSequence.cs
├── Services/
│   └── AudioEngine.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   └── PianoKeyViewModel.cs
├── Views/
│   ├── MainPage.xaml(.cs)
│   ├── PianoSheetView.xaml(.cs)
│   └── PianoKeysView.xaml(.cs)
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   └── Windows/
├── Assets/
│   ├── Samples/
│   │   └── twinkle.json
│   └── Fonts/
├── Piano.csproj
└── App.xaml(.cs)
```

## Key Implementation Details

### Piano Keys (SkiaSharp)
- 3 octaves: C3 to B5
- White keys: 50-60px wide, rounded corners, ivory-to-white gradient
- Black keys: 30px wide, higher position, black-to-gray gradient
- Touch/mouse handling via TouchEffect or gesture recognizers
- Key press visual: gold color + slight scale (0.98x)
- Redraw on key state changes only

### Sheet Display
- Horizontal timeline; each measure as a row
- Notes as colored blocks (width = duration)
- Play mode: scroll to current row, highlight active notes green
- Full mode: show all rows, title visible
- Auto-scroll smooth during playback

### Audio (Synthesized)
- Frequency calculation: `440 * 2^((semitoneIndex)/12.0)`
- Waveform: Triangle or sine wave for piano-like tone
- ADSR envelope: Attack (10ms), Decay (50ms), Sustain (0.7), Release (100ms)
- Polyphony: Multiple simultaneous notes
- Target latency: <50ms

### JSON Sheet Format
```json
{
  "Title": "string",
  "Tempo": 120,
  "Measures": [
    {
      "Number": 1,
      "Notes": [
        { "Name": "C4", "Octave": 4, "Offset": 0, "Duration": 500 }
      ]
    }
  ]
}
```

## NuGet Packages
- CommunityToolkit.Mvvm (latest)
- SkiaSharp.Views.Maui.Controls (latest)
- NAudio (latest or platform-specific alternative if latency issues)

## Build & Run
- Ensure .NET 10 SDK and MAUI workload installed: `dotnet workload install maui`
- Restore: `dotnet restore`
- Build: `dotnet build`
- Run: `dotnet run` or use Visual Studio 2022+ (17.8+)

✅ **Build Status** (2026-03-23): All critical errors fixed. Build succeeds with warnings only (MVVMTK0034 style warnings about direct field access).

## Testing
- Manual test: Load sample JSON, press Play, verify:
  - Audio plays correctly with correct pitch
  - Piano keys highlight in sync
  - Sheet scrolls smoothly to current measure
  - Mode toggle switches display (Full/Play)
- Future: Add unit tests for frequency calculation, sheet parsing

## Performance Tips
- SkiaSharp: Cache key bitmaps if stationary; invalidate only touched keys
- Timer: Use high-resolution timer (Stopwatch) for playback accuracy
- Audio: Pre-generate waveforms for each note if needed
- UI: Disable unnecessary animations on low-end devices

## Common Pitfalls
- Audio latency: Test on all platforms; may need platform-specific optimizations
- SkiaSharp touch events: Coordinates mapping on different screen densities
- Threading: Audio on background thread; UI updates via Dispatcher
- Memory: Dispose audio buffers properly; avoid leaks in long play sessions

## Future Enhancements
- MIDI file import/export
- Volume control, sustain pedal
- Adjustable tempo during playback
- Recording and playback of user performance
- Multiple sheet file support (MusicXML)
- Dark/Light theme toggle

---

*This file provides project-specific guidance. See MEMORY.md for evolving technical decisions and lessons learned.*
