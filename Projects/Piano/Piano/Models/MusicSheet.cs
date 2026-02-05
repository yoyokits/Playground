using System.Text.Json;
using System.Text.Json.Serialization;

namespace Piano.Models;

/// <summary>
/// Represents a complete music sheet with title, tempo, and note sequences
/// </summary>
public class MusicSheet
{
    /// <summary>
    /// Title of the music sheet
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; set; } = "Untitled";

    /// <summary>
    /// Tempo in beats per minute (BPM)
    /// </summary>
    [JsonPropertyName("Tempo")]
    public int Tempo { get; set; } = 120;

    /// <summary>
    /// List of measures/note sequences in this sheet
    /// </summary>
    [JsonPropertyName("Measures")]
    public List<NoteSequence> Measures { get; set; } = new();

    /// <summary>
    /// Loads a MusicSheet from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>Loaded MusicSheet instance</returns>
    public static MusicSheet LoadFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var sheet = JsonSerializer.Deserialize<MusicSheet>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize music sheet");

        // Validate and sort measures by offset or number
        sheet.Measures = sheet.Measures.OrderBy(m => m.Number).ToList();
        return sheet;
    }

    /// <summary>
    /// Creates a MusicSheet from a JSON string
    /// </summary>
    public static MusicSheet Parse(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var sheet = JsonSerializer.Deserialize<MusicSheet>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize music sheet");

        sheet.Measures = sheet.Measures.OrderBy(m => m.Number).ToList();
        return sheet;
    }

    /// <summary>
    /// Gets the total duration of the sheet in milliseconds
    /// </summary>
    public int GetTotalDuration()
    {
        if (!Measures.Any())
            return 0;

        var lastMeasure = Measures.Last();
        return lastMeasure.Offset + lastMeasure.Duration;
    }

    public override string ToString()
    {
        return $"{Title} - Tempo: {Tempo} BPM, {Measures.Count} measures";
    }
}
