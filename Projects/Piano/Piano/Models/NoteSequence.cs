using System.Text.Json.Serialization;

namespace Piano.Models;

/// <summary>
/// Represents a sequence of notes that play together or in sequence within a measure
/// </summary>
public class NoteSequence
{
    /// <summary>
    /// Measure or row number (for display purposes)
    /// </summary>
    [JsonPropertyName("Number")]
    public int Number { get; set; }

    /// <summary>
    /// Time offset in milliseconds from the start of the sequence
    /// </summary>
    [JsonPropertyName("Offset")]
    public int Offset { get; set; }

    /// <summary>
    /// List of notes in this sequence (can be chords or single notes)
    /// </summary>
    [JsonPropertyName("Notes")]
    public List<Note> Notes { get; set; } = new();

    /// <summary>
    /// Duration of this sequence/measure in milliseconds
    /// </summary>
    [JsonPropertyName("Duration")]
    public int Duration { get; set; }

    /// <summary>
    /// Returns a readable description of the notes in this sequence
    /// </summary>
    public override string ToString()
    {
        var noteNames = string.Join(", ", Notes.Select(n => n.ToString()));
        return $"Measure {Number}: [{noteNames}] at {Offset}ms, duration {Duration}ms";
    }
}
