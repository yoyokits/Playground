# Piano App Memory

## Current Development Status (2026-03-23)
**Status**: ✅ Build Successful - Ready for Runtime Testing
- All critical compilation errors have been fixed
- Build succeeds with 146 warnings (mostly MVVMTK0034 style warnings)
- Application is ready for deployment and testing

### Fixed Issues (2026-03-23)
1. ✅ Added `using System.Text.Json.Serialization;` to `MusicSheet.cs`
2. ✅ Changed `DisplayNote` to `public partial class DisplayNote` in `MainViewModel.cs`
3. ✅ Added missing `Offset` property to `Note` model (required for playback)
4. ✅ Added missing observable properties: `IsPlaying`, `CurrentPlayPosition`, `CurrentSheet` to `MainViewModel`
5. ✅ Replaced non-existent `SignalGenerator` with custom `TriangleWaveProvider` in `AudioEngine`
6. ✅ Removed invalid `SKPaint.CornerOrSize` property in `PianoSheetView`
7. ✅ Removed `Dispose()` call on `IDispatcherTimer` (not supported)
8. ✅ Restored missing `_sheetTitle` field with `[ObservableProperty]`

### Feature Implementation Status
✅ **Models Layer** (100%)
- `Note.cs` - Complete with frequency calculation and Offset property
- `NoteSequence.cs` - Complete with JSON serialization
- `MusicSheet.cs` - Complete with JSON deserialization

✅ **Services Layer** (100%)
- `AudioEngine.cs` - Complete with ADSR envelope synthesis using custom wave provider

✅ **ViewModels Layer** (100%)
- `MainViewModel.cs` - Complete with all observable properties
- `PianoKeyViewModel.cs` - Complete

✅ **Views Layer** (100%)
- `MainPage.xaml` - Complete with full UI and bindings
- `PianoKeysView.xaml/.xaml.cs` - Complete with SkiaSharp rendering and touch handling
- `PianoSheetView.xaml/.xaml.cs` - Complete with SkiaSharp rendering, dual modes

✅ **Sample Data**
- Programmatic sample (Twinkle Twinkle Little Star) available as fallback
- JSON asset loading implemented for "Assets/Samples/twinkle.json"

## Lessons Learned
- **2026-03-23**: Source generators in .NET 10 require explicit `partial` modifier on all partial class declarations. Forgetting it causes CS0260 errors.
- **2026-03-23**: JSON serialization attributes require specific using: `JsonPropertyName` needs `System.Text.Json.Serialization`, not just `System.Text.Json`.
- **2026-03-23**: NAudio does not include `SignalGenerator` class; custom wave providers must be implemented for synthesis.
- **2026-03-23**: SkiaSharp's `SKPaint` doesn't have `CornerOrSize` property; rounded corners must be applied during path/draw operations.
- **2026-03-23**: `IDispatcherTimer` doesn't implement `IDisposable`; remove Dispose calls.
- **2026-03-23**: CommunityToolkit.Mvvm's `[ObservableProperty]` generates a property; direct field access within the class is allowed but generates warnings. Access the generated property when outside the class or to avoid MVVMTK0034 warnings.

## Architecture Decisions
- **2025-03-23**: .NET MAUI with MVVM (CommunityToolkit.Mvvm) chosen for cross-platform support and maintainable code structure.
- **2025-03-23**: SkiaSharp selected for piano key rendering to achieve professional gradients and smooth performance.
- **2025-03-23**: Synthesized audio via NAudio (no external sample files) to keep distributable small and avoid licensing issues.
- **2025-03-23**: 3-octave range (C3-B5) covers most beginner/intermediate songs and balances UI space.

## UI Design Decisions
- **2025-03-23**: Piano sheet display as simplified horizontal timeline (not traditional notation) for easier parsing and clear visualization.
- **2025-03-23**: Two display modes: Full Sheet (all notes) and Play mode (current row focus with green highlight).
- **2025-03-23**: Piano keys with white:ivory->white gradient, black:black->dark-gray gradient, plus drop shadows for 3D effect.
- **2025-03-23**: Gold highlight on pressed keys for high contrast against white/black.

## Technical Implementation Notes
- **Frequency Calculation**: A4=440Hz, semitone steps = 2^(1/12). Formula: `freq = 440 * Math.Pow(2, (semitonesFromA4)/12.0)`
- **Audio Envelope**: ADSR - Attack 10ms, Decay 50ms, Sustain 0.7, Release 100ms
- **Key Layout**: 36 white keys (C3-C6), 21 black keys. Coordinate mapping: X position divided by total width, modulo 7 white key widths.
- **Synchronization**: DispatcherTimer at 16ms (60fps) for UI updates; separate high-resolution timer for audio scheduling.
- **Sheet JSON**: Measures collection with Number property; Notes within each Measure have Offset (ms from measure start) and Duration (ms).

## Open Questions / Risks
- Audio latency on mobile: May need platform-specific audio APIs (OpenSL ES on Android, AVAudioEngine on iOS) if NAudio is insufficient.
- SkiaSharp touch handling: Need to test on actual devices for accurate coordinate mapping.
- Memory: Pre-generate all waveforms? Or generate on-demand? TBD based on performance testing.

## Lessons Learned
- **2026-03-23**: Source generators in .NET 10 require explicit `partial` modifier on all partial class declarations. Forgetting it causes CS0260 errors.
- **2026-03-23**: JSON serialization attributes require specific using statements. `JsonPropertyName` needs `System.Text.Json.Serialization`, not just `System.Text.Json`.
- **2026-03-23**: Build errors can slip through if not testing on all target platforms (errors appeared on iOS/MacCatalyst/Android/Windows).

## Sample Songs
- Twinkle Twinkle Little Star (first test)
