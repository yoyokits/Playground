using Android.Content;
using Android.Media;
using Piano.Models;
using System.Collections.Concurrent;

namespace Piano.Services;

/// <summary>
/// Android-specific audio implementation using AudioTrack
/// Note: For production, consider using a cross-platform audio library like Plugin.Maui.Audio
/// </summary>
public class AndroidAudioService : IAudioService, IDisposable
{
    private const int SampleRate = 44100;
    private const int Amplitude = 16000;
    private readonly ConcurrentDictionary<int, AudioTrack> _activeNotes = new();
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _playbackTokens = new();
    private bool _disposed;

    private static AndroidAudioService? _instance;
    public static AndroidAudioService Instance => _instance ??= new AndroidAudioService();

    private AndroidAudioService()
    {
    }

    public void PlayNote(Note note)
    {
        if (_disposed) return;

        var frequency = note.GetFrequency();
        var key = GetNoteKey(note);

        // Stop existing note if playing
        StopNote(note);

        var cts = new CancellationTokenSource();
        _playbackTokens[key] = cts;

        try
        {
            var minBufferSize = AudioTrack.GetMinBufferSize(
                SampleRate,
                ChannelOut.Mono,
                Android.Media.Encoding.Pcm16bit);

            // Use the deprecated constructor for maximum compatibility
            // This works across all Android API levels
            #pragma warning disable CS0618 // Type or member is obsolete
            var audioTrack = new AudioTrack(
                Android.Media.Stream.Music,
                SampleRate,
                ChannelOut.Mono,
                Android.Media.Encoding.Pcm16bit,
                minBufferSize * 2,
                AudioTrackMode.Stream);
            #pragma warning restore CS0618

            _activeNotes[key] = audioTrack;

            Task.Run(() => PlayTone(audioTrack, frequency, cts.Token), cts.Token);
        }
        catch (Exception)
        {
            // Cleanup on error
            _playbackTokens.TryRemove(key, out _);
            _activeNotes.TryRemove(key, out _);
        }
    }

    private void PlayTone(AudioTrack audioTrack, double frequency, CancellationToken token)
    {
        try
        {
            audioTrack.Play();

            var buffer = new short[1024];
            double phase = 0;
            double phaseIncrement = 2.0 * Math.PI * frequency / SampleRate;
            bool attackPhase = true;
            int attackSamples = (int)(SampleRate * 0.01); // 10ms attack
            int attackCount = 0;

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    // Triangle wave generation
                    double sample = 1.0 - 2.0 * Math.Abs(phase / (2.0 * Math.PI) - 0.5) * 2.0;

                    // Simple ADSR - attack phase
                    double envelope = 1.0;
                    if (attackPhase && attackCount < attackSamples)
                    {
                        envelope = (double)attackCount / attackSamples;
                        attackCount++;
                    }
                    else
                    {
                        attackPhase = false;
                        envelope = 0.7; // Sustain level
                    }

                    sample *= Amplitude * envelope;
                    buffer[i] = (short)sample;

                    phase += phaseIncrement;
                    if (phase >= 2.0 * Math.PI)
                        phase -= 2.0 * Math.PI;
                }

                audioTrack.Write(buffer, 0, buffer.Length);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopped
        }
        catch (Exception)
        {
            // Ignore other errors
        }
        finally
        {
            try
            {
                audioTrack.Stop();
                audioTrack.Release();
            }
            catch { }
        }
    }

    public void StopNote(Note note)
    {
        var key = GetNoteKey(note);

        if (_playbackTokens.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        if (_activeNotes.TryRemove(key, out var audioTrack))
        {
            try
            {
                audioTrack.Stop();
                audioTrack.Release();
            }
            catch { }
        }
    }

    public void StopAll()
    {
        foreach (var key in _playbackTokens.Keys.ToList())
        {
            if (_playbackTokens.TryRemove(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
            if (_activeNotes.TryRemove(key, out var audioTrack))
            {
                try
                {
                    audioTrack.Stop();
                    audioTrack.Release();
                }
                catch { }
            }
        }
    }

    private int GetNoteKey(Note note)
    {
        // Generate unique key based on note name and octave
        return (note.Octave * 100) + note.Name.GetHashCode();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAll();
            _disposed = true;
        }
    }
}