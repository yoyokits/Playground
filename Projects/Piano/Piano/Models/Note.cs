using System.Text.Json.Serialization;

namespace Piano.Models;

/// <summary>
/// Represents a single musical note with name, octave, and duration
/// </summary>
public class Note
{
    /// <summary>
    /// Note name (C, C#, D, D#, E, F, F#, G, G#, A, A#, B)
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Octave number (1-7, typically 3-6 for our piano)
    /// </summary>
    [JsonPropertyName("Octave")]
    public int Octave { get; set; }

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    [JsonPropertyName("Duration")]
    public int Duration { get; set; }

    /// <summary>
    /// Time offset in milliseconds from the start of the measure
    /// </summary>
    [JsonPropertyName("Offset")]
    public int Offset { get; set; }

    /// <summary>
    /// Calculates the frequency of this note in Hz (A4 = 440Hz)
    /// </summary>
    public double GetFrequency()
    {
        // Semitone indices: C=0, C#=1, D=2, D#=3, E=4, F=5, F#=6, G=7, G#=8, A=9, A#=10, B=11
        var noteIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["C"] = 0, ["C#"] = 1, ["DB"] = 1,
            ["D"] = 2, ["D#"] = 3, ["EB"] = 3,
            ["E"] = 4, ["F"] = 5, ["F#"] = 6, ["GB"] = 6,
            ["G"] = 7, ["G#"] = 8, ["AB"] = 8,
            ["A"] = 9, ["A#"] = 10, ["BB"] = 10,
            ["B"] = 11
        };

        if (!noteIndices.TryGetValue(Name, out int semitoneIndex))
            throw new ArgumentException($"Invalid note name: {Name}");

        // A4 is at index 9 in octave 4. Calculate semitones from A4.
        int semitonesFromA4 = semitoneIndex + (Octave - 4) * 12 - 9;
        return 440.0 * Math.Pow(2, semitonesFromA4 / 12.0);
    }

    public override string ToString()
    {
        return $"{Name}{Octave}";
    }
}
