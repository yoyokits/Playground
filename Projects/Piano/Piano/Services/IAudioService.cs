using Piano.Models;

namespace Piano.Services;

/// <summary>
/// Cross-platform audio service interface
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Plays a note with the given frequency
    /// </summary>
    void PlayNote(Note note);

    /// <summary>
    /// Stops a specific note
    /// </summary>
    void StopNote(Note note);

    /// <summary>
    /// Stops all currently playing notes
    /// </summary>
    void StopAll();

    /// <summary>
    /// Releases resources
    /// </summary>
    void Dispose();
}