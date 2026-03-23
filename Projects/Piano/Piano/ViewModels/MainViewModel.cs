using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Dispatching;
using Piano.Models;
using Piano.Services;
using System.Collections.ObjectModel;

namespace Piano.ViewModels;

/// <summary>
/// Main ViewModel for the Piano application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private MusicSheet? _currentSheet = null;

    private IDispatcherTimer? _playbackTimer;

    [ObservableProperty]
    private bool _isPlaying = false;

    [ObservableProperty]
    private int _currentPlayPosition = 0;

    private bool _isPlayMode;

    [ObservableProperty]
    private string _sheetTitle = "No sheet loaded";

    [ObservableProperty]
    private ObservableCollection<PianoKeyViewModel> _pianoKeys = new();

    [ObservableProperty]
    private ObservableCollection<NoteSequence> _noteSequences = new();

    [ObservableProperty]
    private ObservableCollection<Note> _currentPlayingNotes = new();

    // The collection of all notes currently being displayed in the sheet (for highlighting)
    [ObservableProperty]
    private ObservableCollection<DisplayNote> _displayNotes = new();

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public MainViewModel(IAudioService audioService)
    {
        _audioService = audioService;
    }

    /// <summary>
    /// Default constructor for XAML (uses platform service)
    /// </summary>
    public MainViewModel() : this(GetPlatformAudioService())
    {
    }

    private static IAudioService GetPlatformAudioService()
    {
#if ANDROID
        return AndroidAudioService.Instance;
#elif WINDOWS
        return WindowsAudioService.Instance;
#else
        throw new PlatformNotSupportedException("Audio service not supported on this platform");
#endif
    }

    public bool IsPlayMode
    {
        get => _isPlayMode;
        set
        {
            if (SetProperty(ref _isPlayMode, value))
            {
                // Refresh display when mode changes
                UpdateDisplayNotes();
            }
        }
    }

    [RelayCommand]
    private async Task LoadSheetAsync()
    {
        try
        {
            // For now, load the sample file from assets
            // In a full implementation, this would use a file picker
            var assembly = typeof(MainViewModel).Assembly;
            var resourceId = "Piano.Assets.Samples.twinkle.json";

            using var stream = assembly.GetManifestResourceStream(resourceId);
            if (stream == null)
            {
                // Try loading from file system as fallback
                var filePath = Path.Combine("Assets", "Samples", "twinkle.json");
                if (File.Exists(filePath))
                {
                    _currentSheet = MusicSheet.LoadFromFile(filePath);
                }
                else
                {
                    // Hard-coded sample for testing
                    _currentSheet = CreateSampleSheet();
                }
            }
            else
            {
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                _currentSheet = MusicSheet.Parse(json);
            }

            SheetTitle = _currentSheet.Title;
            LoadNoteSequences();
            InitializePianoKeys();
            UpdateDisplayNotes();
        }
        catch (Exception ex)
        {
            SheetTitle = $"Error loading sheet: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsPlayMode = !IsPlayMode;
    }

    [RelayCommand]
    private void Play()
    {
        if (_currentSheet == null || _isPlaying)
            return;

        _playbackTimer = Application.Current?.Dispatcher.CreateTimer();
        _playbackTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
        _playbackTimer.Tick += PlaybackTimer_Tick;
        _currentPlayPosition = 0;
        _isPlaying = true;
        _playbackTimer.Start();
    }

    [RelayCommand]
    private void Stop()
    {
        _playbackTimer?.Stop();
        _playbackTimer = null;
        _isPlaying = false;
        _audioService.StopAll();
        _currentPlayPosition = 0;
        UpdateDisplayNotes();
    }

    [RelayCommand]
    private void PianoKeyPressed(int noteIndex)
    {
        if (noteIndex < 0 || noteIndex >= PianoKeys.Count)
            return;

        var key = PianoKeys[noteIndex];
        key.IsPressed = true;

        // Play the note
        var note = new Note
        {
            Name = key.NoteName,
            Octave = key.Octave,
            Duration = 0 // For interactive play, duration is indefinite
        };
        _audioService.PlayNote(note);
    }

    [RelayCommand]
    private void PianoKeyReleased(int noteIndex)
    {
        if (noteIndex < 0 || noteIndex >= PianoKeys.Count)
            return;

        var key = PianoKeys[noteIndex];
        key.IsPressed = false;

        var note = new Note
        {
            Name = key.NoteName,
            Octave = key.Octave,
            Duration = 0
        };
        _audioService.StopNote(note);
    }

    private void PlaybackTimer_Tick(object? sender, EventArgs e)
    {
        if (_currentSheet == null)
            return;

        // Update playback position (assuming ms)
        _currentPlayPosition += 16;

        var totalDuration = _currentSheet.GetTotalDuration();
        if (_currentPlayPosition >= totalDuration)
        {
            Stop();
            return;
        }

        // Find which notes are currently playing
        var playingNotes = new List<Note>();
        foreach (var measure in _currentSheet.Measures)
        {
            var measureStart = measure.Offset;
            var measureEnd = measureStart + measure.Duration;

            if (_currentPlayPosition >= measureStart && _currentPlayPosition < measureEnd)
            {
                foreach (var note in measure.Notes)
                {
                    var noteEnd = note.Offset + note.Duration;
                    if (_currentPlayPosition >= note.Offset && _currentPlayPosition < noteEnd)
                    {
                        playingNotes.Add(note);
                    }
                }
            }
        }

        // Update piano key visuals
        UpdatePianoKeyStates(playingNotes);

        // Update CurrentPlayingNotes for binding
        CurrentPlayingNotes.Clear();
        foreach (var note in playingNotes)
        {
            CurrentPlayingNotes.Add(note);
        }

        // Update display notes for sheet view
        UpdateDisplayNotes();
    }

    private void LoadNoteSequences()
    {
        NoteSequences.Clear();
        if (_currentSheet != null)
        {
            foreach (var measure in _currentSheet.Measures)
            {
                NoteSequences.Add(measure);
            }
        }
    }

    private void InitializePianoKeys()
    {
        PianoKeys.Clear();

        // Create placeholder keys - positions will be recalculated by PianoKeysView
        // based on actual canvas dimensions
        var octaves = new[] { 3, 4, 5 };
        var whiteNotes = new[] { "C", "D", "E", "F", "G", "A", "B" };
        var blackNotes = new[] { "C#", "D#", null, "F#", "G#", "A#" };

        foreach (var octave in octaves)
        {
            // White keys - use placeholder positions (will be recalculated by View)
            for (int i = 0; i < whiteNotes.Length; i++)
            {
                var key = new PianoKeyViewModel(whiteNotes[i], octave, false, i * 60)
                {
                    IsPressed = false
                };
                PianoKeys.Add(key);
            }

            // Black keys
            for (int i = 0; i < blackNotes.Length; i++)
            {
                if (blackNotes[i] == null) continue;
                // Placeholder position - will be recalculated by View
                var keyX = (i + 1) * 60 - 15;
                var key = new PianoKeyViewModel(blackNotes[i], octave, true, keyX)
                {
                    IsPressed = false
                };
                PianoKeys.Add(key);
            }
        }
    }

    /// <summary>
    /// Updates piano key positions from the View (called after layout)
    /// </summary>
    public void UpdateKeyPositions(ObservableCollection<PianoKeyViewModel> keys)
    {
        PianoKeys.Clear();
        foreach (var key in keys)
        {
            PianoKeys.Add(key);
        }
    }

    private void UpdatePianoKeyStates(List<Note> playingNotes)
    {
        // Reset all keys
        foreach (var key in PianoKeys)
        {
            key.IsPressed = playingNotes.Any(n =>
                string.Equals(n.Name, key.NoteName, StringComparison.OrdinalIgnoreCase) &&
                n.Octave == key.Octave);
        }
    }

    private void UpdateDisplayNotes()
    {
        DisplayNotes.Clear();

        if (_currentSheet == null)
            return;

        foreach (var measure in _currentSheet.Measures)
        {
            foreach (var note in measure.Notes)
            {
                var displayNote = new DisplayNote
                {
                    NoteName = $"{note.Name}{note.Octave}",
                    Offset = measure.Offset + note.Offset,
                    Duration = note.Duration,
                    MeasureNumber = measure.Number,
                    IsPlaying = false
                };

                if (IsPlayMode && _isPlaying)
                {
                    // In Play mode, highlight if currently playing
                    displayNote.IsPlaying = _currentPlayPosition >= displayNote.Offset &&
                                           _currentPlayPosition < (displayNote.Offset + note.Duration);
                }
                else if (!IsPlayMode)
                {
                    // In Full mode, show all notes with default colors
                    displayNote.IsPlaying = false;
                }

                DisplayNotes.Add(displayNote);
            }
        }
    }

    private MusicSheet CreateSampleSheet()
    {
        return new MusicSheet
        {
            Title = "Twinkle Twinkle Little Star",
            Tempo = 100,
            Measures = new List<NoteSequence>
            {
                new()
                {
                    Number = 1,
                    Offset = 0,
                    Duration = 7000,
                    Notes = new List<Note>
                    {
                        new() { Name = "C", Octave = 4, Duration = 500, Offset = 0 },
                        new() { Name = "C", Octave = 4, Duration = 500, Offset = 500 },
                        new() { Name = "G", Octave = 4, Duration = 1000, Offset = 1000 },
                        new() { Name = "G", Octave = 4, Duration = 1000, Offset = 2000 },
                        new() { Name = "A", Octave = 4, Duration = 1000, Offset = 3000 },
                        new() { Name = "A", Octave = 4, Duration = 1000, Offset = 4000 },
                        new() { Name = "G", Octave = 4, Duration = 2000, Offset = 5000 }
                    }
                },
                new()
                {
                    Number = 2,
                    Offset = 7000,
                    Duration = 7000,
                    Notes = new List<Note>
                    {
                        new() { Name = "F", Octave = 4, Duration = 1000, Offset = 0 },
                        new() { Name = "F", Octave = 4, Duration = 1000, Offset = 1000 },
                        new() { Name = "E", Octave = 4, Duration = 1000, Offset = 2000 },
                        new() { Name = "E", Octave = 4, Duration = 1000, Offset = 3000 },
                        new() { Name = "D", Octave = 4, Duration = 1000, Offset = 4000 },
                        new() { Name = "D", Octave = 4, Duration = 1000, Offset = 5000 },
                        new() { Name = "C", Octave = 4, Duration = 1000, Offset = 6000 }
                    }
                }
            }
        };
    }
}

/// <summary>
/// Helper class for displaying notes in the sheet view
/// </summary>
public partial class DisplayNote : ObservableObject
{
    [ObservableProperty]
    private string _noteName = string.Empty;

    [ObservableProperty]
    private int _offset;

    [ObservableProperty]
    private int _duration;

    [ObservableProperty]
    private int _measureNumber;

    [ObservableProperty]
    private bool _isPlaying;
}
