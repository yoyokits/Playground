using CommunityToolkit.Mvvm.ComponentModel;

namespace Piano.ViewModels;

/// <summary>
/// ViewModel for an individual piano key
/// </summary>
public partial class PianoKeyViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isPressed;

    [ObservableProperty]
    private string _noteName = string.Empty;

    [ObservableProperty]
    private int _octave;

    [ObservableProperty]
    private bool _isBlackKey;

    [ObservableProperty]
    private double _xPosition;

    /// <summary>
    /// Gets or sets the visual color of the key (used for highlighting)
    /// </summary>
    public string KeyColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Full note name (e.g., "C4")
    /// </summary>
    public string FullNoteName => $"{NoteName}{Octave}";

    /// <summary>
    /// Creates a new PianoKeyViewModel
    /// </summary>
    public PianoKeyViewModel(string noteName, int octave, bool isBlackKey, double xPosition)
    {
        NoteName = noteName;
        Octave = octave;
        IsBlackKey = isBlackKey;
        XPosition = xPosition;
        UpdateColor();
    }

    /// <summary>
    /// Updates the visual color based on press state
    /// </summary>
    partial void OnIsPressedChanged(bool value)
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (IsPressed)
        {
            // Gold/yellow highlight when pressed
            KeyColor = "#FFD700";
        }
        else if (IsBlackKey)
        {
            KeyColor = "#1a1a1a"; // Dark black
        }
        else
        {
            KeyColor = "#F5F5DC"; // Ivory/off-white
        }
    }
}
