using NAudio.Wave;
using Piano.Models;
using System.Collections.Concurrent;

namespace Piano.Services;

/// <summary>
/// Windows-specific audio implementation using NAudio
/// </summary>
public class WindowsAudioService : IAudioService, IDisposable
{
    private readonly ConcurrentDictionary<int, (WaveOutEvent waveOut, AdsrWaveProvider provider)> _activeNotes = new();
    private bool _disposed;
    private const int SampleRate = 44100;
    private const int Amplitude = 16384;

    private static WindowsAudioService? _instance;
    public static WindowsAudioService Instance => _instance ??= new WindowsAudioService();

    private WindowsAudioService() { }

    public void PlayNote(Note note)
    {
        if (_disposed) return;

        var key = GetNoteKey(note);
        StopNote(note); // Stop existing

        var frequency = note.GetFrequency();
        var waveOut = new WaveOutEvent();
        var provider = new TriangleWaveProvider(frequency, SampleRate, Amplitude);
        var adsrProvider = new AdsrWaveProvider(provider, SampleRate,
            TimeSpan.FromSeconds(0.01), TimeSpan.FromSeconds(0.05), 0.7, TimeSpan.FromSeconds(0.1));

        waveOut.Init(adsrProvider);
        waveOut.Play();

        _activeNotes[key] = (waveOut, adsrProvider);
    }

    public void StopNote(Note note)
    {
        var key = GetNoteKey(note);
        if (_activeNotes.TryRemove(key, out var player))
        {
            player.waveOut.Stop();
            player.waveOut.Dispose();
        }
    }

    public void StopAll()
    {
        foreach (var kvp in _activeNotes)
        {
            kvp.Value.waveOut.Stop();
            kvp.Value.waveOut.Dispose();
        }
        _activeNotes.Clear();
    }

    private int GetNoteKey(Note note) => (note.Octave * 100) + note.Name.GetHashCode();

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAll();
            _disposed = true;
        }
    }
}

// NAudio providers (moved here for Windows)
internal class AdsrWaveProvider : IWaveProvider
{
    private readonly IWaveProvider _source;
    private readonly int _sampleRate;
    private readonly double _attackTime, _decayTime, _sustainLevel, _releaseTime;
    private int _samplesPlayed;
    private bool _noteReleased;
    private int _releaseSamplesPlayed;

    public WaveFormat WaveFormat { get; }

    public AdsrWaveProvider(IWaveProvider source, int sampleRate, TimeSpan attack, TimeSpan decay, double sustain, TimeSpan release)
    {
        _source = source;
        _sampleRate = sampleRate;
        _attackTime = attack.TotalSeconds;
        _decayTime = decay.TotalSeconds;
        _sustainLevel = sustain;
        _releaseTime = release.TotalSeconds;
        WaveFormat = source.WaveFormat;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        int samplesNeeded = count / 2;
        int samplesWritten = 0;

        while (samplesWritten < samplesNeeded)
        {
            var tempBuffer = new byte[2];
            int bytesRead = _source.Read(tempBuffer, 0, 2);
            if (bytesRead < 2) break;

            short sample = BitConverter.ToInt16(tempBuffer, 0);
            double sampleDouble = sample / 32768.0;
            double envelope = CalculateEnvelope();
            sampleDouble *= envelope;
            sample = (short)(sampleDouble * 32767);

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
            double releaseSamples = _releaseTime * _sampleRate;
            double releaseProgress = (double)_releaseSamplesPlayed / releaseSamples;
            if (releaseProgress >= 1.0) return 0.0;
            _releaseSamplesPlayed++;
            return 1.0 - releaseProgress;
        }

        double attackSamples = _attackTime * _sampleRate;
        if (_samplesPlayed < attackSamples) return _samplesPlayed / attackSamples;

        double decaySamples = _decayTime * _sampleRate;
        int decayStart = (int)attackSamples;
        if (_samplesPlayed < decayStart + decaySamples)
        {
            double decayProgress = (_samplesPlayed - decayStart) / decaySamples;
            return 1.0 - (1.0 - _sustainLevel) * decayProgress;
        }
        return _sustainLevel;
    }

    public void Release()
    {
        if (!_noteReleased) _noteReleased = true;
    }
}

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
            double phaseIncrement = 2.0 * Math.PI * _frequency / _sampleRate;
            _phase += phaseIncrement;
            if (_phase >= 2.0 * Math.PI) _phase -= 2.0 * Math.PI;

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