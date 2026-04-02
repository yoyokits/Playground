using System.Collections.Concurrent;
using NAudio.Wave;
using Piano.Models;

namespace Piano.Services;

/// <summary>
/// Singleton audio engine for synthesizing and playing piano notes
/// Uses NAudio to generate waveforms with ADSR envelope
/// </summary>
public class AudioEngine : IDisposable
{
    private readonly ConcurrentDictionary<Note, WaveOutEvent> _activeNotes = new();
    private WaveFormat _waveFormat;
    private bool _disposed;

    // Audio parameters
    private const int SampleRate = 44100;
    private const int Amplitude = 16384; // 16-bit audio amplitude (max 32767)
    private const double AttackTime = 0.01; // 10ms
    private const double DecayTime = 0.05; // 50ms
    private const double SustainLevel = 0.7;
    private const double ReleaseTime = 0.1; // 100ms

    private static AudioEngine? _instance;
    public static AudioEngine Instance => _instance ??= new AudioEngine();

    private AudioEngine()
    {
        _waveFormat = new WaveFormat(SampleRate, 16, 1); // 16-bit mono
    }

    /// <summary>
    /// Plays a note with ADSR envelope
    /// </summary>
    public void PlayNote(Note note)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AudioEngine));

        var frequency = note.GetFrequency();
        var waveOut = new WaveOutEvent();
        var provider = new TriangleWaveProvider(frequency, SampleRate, Amplitude);

        // Apply ADSR envelope manually via custom provider
        var envelopeProvider = new AdsrWaveProvider(provider, SampleRate,
            attack: TimeSpan.FromSeconds(AttackTime),
            decay: TimeSpan.FromSeconds(DecayTime),
            sustain: SustainLevel,
            release: TimeSpan.FromSeconds(ReleaseTime));

        waveOut.Init(envelopeProvider);
        waveOut.Play();

        _activeNotes[note] = waveOut;
    }

    /// <summary>
    /// Stops a specific note with release envelope
    /// </summary>
    public void StopNote(Note note)
    {
        if (_activeNotes.TryRemove(note, out var waveOut))
        {
            // Trigger release
            waveOut.Stop();
            waveOut.Dispose();
        }
    }

    /// <summary>
    /// Stops all currently playing notes
    /// </summary>
    public void StopAll()
    {
        foreach (var kvp in _activeNotes)
        {
            kvp.Value.Stop();
            kvp.Value.Dispose();
        }
        _activeNotes.Clear();
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

/// <summary>
/// Custom wave provider that applies ADSR envelope to an underlying provider
/// </summary>
internal class AdsrWaveProvider : IWaveProvider
{
    private readonly IWaveProvider _source;
    private readonly int _sampleRate;
    private readonly double _attackTime;
    private readonly double _decayTime;
    private readonly double _sustainLevel;
    private readonly double _releaseTime;

    private int _samplesPlayed;
    private bool _noteReleased;
    private int _releaseSamplesPlayed;
    private bool _isPlaying;

    public AdsrWaveProvider(IWaveProvider source, int sampleRate,
        TimeSpan attack, TimeSpan decay, double sustain, TimeSpan release)
    {
        _source = source;
        _sampleRate = sampleRate;
        _attackTime = attack.TotalSeconds;
        _decayTime = decay.TotalSeconds;
        _sustainLevel = sustain;
        _releaseTime = release.TotalSeconds;
        WaveFormat = source.WaveFormat;
    }

    public WaveFormat WaveFormat { get; }

    public int Read(byte[] buffer, int offset, int count)
    {
        int samplesNeeded = count / 2; // 16-bit = 2 bytes per sample
        int samplesWritten = 0;

        while (samplesWritten < samplesNeeded)
        {
            // Read from source
            var tempBuffer = new byte[2];
            int bytesRead = _source.Read(tempBuffer, 0, 2);
            if (bytesRead < 2)
                break;

            short sample = BitConverter.ToInt16(tempBuffer, 0);
            double sampleDouble = sample / 32768.0; // Convert to [-1, 1]

            // Calculate envelope
            double envelope = CalculateEnvelope();

            // Apply envelope
            sampleDouble *= envelope;
            sample = (short)(sampleDouble * 32767);

            // Write to output buffer
            buffer[offset + samplesWritten * 2] = (byte)(sample & 0xFF);
            buffer[offset + samplesWritten * 2 + 1] = (byte)((sample >> 8) & 0xFF);
            samplesWritten++;
            _samplesPlayed++;
        }

        return samplesWritten * 2;
    }

    private double CalculateEnvelope()
    {
        if (_noteReleased)
        {
            // Release phase
            double releaseSamples = _releaseTime * _sampleRate;
            double releaseProgress = (double)_releaseSamplesPlayed / releaseSamples;
            if (releaseProgress >= 1.0)
                return 0.0;

            _releaseSamplesPlayed++;
            return 1.0 - releaseProgress; // Linear decay from current level to 0
        }

        // Attack phase
        double attackSamples = _attackTime * _sampleRate;
        if (_samplesPlayed < attackSamples)
        {
            return _samplesPlayed / attackSamples;
        }

        // Decay phase
        double decaySamples = _decayTime * _sampleRate;
        int decayStart = (int)attackSamples;
        if (_samplesPlayed < decayStart + decaySamples)
        {
            double decayProgress = (_samplesPlayed - decayStart) / decaySamples;
            return 1.0 - (1.0 - _sustainLevel) * decayProgress;
        }

        // Sustain phase
        return _sustainLevel;
    }

    public void Release()
    {
        if (!_noteReleased)
        {
            _noteReleased = true;
            _isPlaying = false;
        }
    }
}

/// <summary>
/// Triangle wave provider for generating smooth waveforms
/// </summary>
internal class TriangleWaveProvider : IWaveProvider
{
    private readonly double _frequency;
    private readonly int _sampleRate;
    private readonly int _amplitude;
    private double _phase;

    public WaveFormat WaveFormat { get; }

    public TriangleWaveProvider(double frequency, int sampleRate, int amplitude)
    {
        _frequency = frequency;
        _sampleRate = sampleRate;
        _amplitude = amplitude;
        _phase = 0;
        WaveFormat = new WaveFormat(sampleRate, 16, 1);
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        int samplesNeeded = count / 2;
        int samplesWritten = 0;

        while (samplesWritten < samplesNeeded)
        {
            // Generate triangle wave: goes from -1 to 1 and back
            double phaseIncrement = 2.0 * Math.PI * _frequency / _sampleRate;
            _phase += phaseIncrement;
            if (_phase >= 2.0 * Math.PI)
                _phase -= 2.0 * Math.PI;

            // Triangle wave: absolute value of sawtooth, then inverted
            double sample = 1.0 - 2.0 * Math.Abs(_phase / (2.0 * Math.PI) - 0.5) * 2.0;
            sample *= _amplitude;

            short sampleShort = (short)sample;
            buffer[offset + samplesWritten * 2] = (byte)(sampleShort & 0xFF);
            buffer[offset + samplesWritten * 2 + 1] = (byte)((sampleShort >> 8) & 0xFF);
            samplesWritten++;
        }

        return samplesWritten * 2;
    }
}
