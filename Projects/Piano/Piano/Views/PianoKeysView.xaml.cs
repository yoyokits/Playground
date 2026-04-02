using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Piano.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace Piano.Views;

/// <summary>
/// SkiaSharp-based piano keys view with professional rendering
/// Renders 3 octaves (C3-B5) with touch/mouse interaction
/// </summary>
public partial class PianoKeysView : ContentView
{
    private MainViewModel? _viewModel;
    private float _canvasWidth;
    private float _canvasHeight;
    private float _whiteKeyWidth;
    private float _whiteKeyHeight;
    private float _blackKeyWidth;
    private float _blackKeyHeight;
    private SKPaint? _whiteKeyPaint;
    private SKPaint? _blackKeyPaint;
    private SKPaint? _pressedWhiteKeyPaint;
    private SKPaint? _pressedBlackKeyPaint;
    private SKPaint? _shadowPaint;

    // Piano configuration - 3 octaves: C3 to B5
    private const int NumberOfOctaves = 3;
    private const int StartOctave = 3; // C3
    private const int EndOctave = 5;   // B5
    private const float KeySpacing = 2f;
    private const float WhiteKeyWidthBase = 60f;
    private const float WhiteKeyHeightBase = 200f;
    private const float BlackKeyWidthRatio = 0.6f;
    private const float BlackKeyHeightRatio = 0.6f;

    // Note names for 3 octaves
    private readonly string[] _whiteNotes = { "C", "D", "E", "F", "G", "A", "B" };
    private readonly string?[] _blackNotes = { "C#", "D#", null, "F#", "G#", "A#" };

    public PianoKeysView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
        this.SizeChanged += OnSizeChanged;

        // Set binding context to MainViewModel (assume injected or set by parent)
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
        {
            _viewModel = vm;
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }
        SetupPaints();
        CalculateKeyDimensions();
        InitializeKeyPositions();
        Canvas.InvalidateSurface();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        CalculateKeyDimensions();
        InitializeKeyPositions();
        SetupPaints(); // Recreate paints with new dimensions
        Canvas.InvalidateSurface();
    }

    private void InitializeKeyPositions()
    {
        if (_viewModel == null || _whiteKeyWidth <= 0) return;

        // Rebuild piano keys with correct positions based on actual canvas dimensions
        _viewModel.PianoKeys.Clear();

        double xPos = 0;
        var octaves = new[] { 3, 4, 5 };

        foreach (var octave in octaves)
        {
            // White keys
            for (int i = 0; i < _whiteNotes.Length; i++)
            {
                var key = new PianoKeyViewModel(_whiteNotes[i], octave, false, xPos)
                {
                    IsPressed = false
                };
                _viewModel.PianoKeys.Add(key);
                xPos += _whiteKeyWidth + KeySpacing;
            }

            // Black keys (positioned between white keys)
            double blackXOffset = _whiteKeyWidth - (_blackKeyWidth / 2);
            for (int i = 0; i < _blackNotes.Length; i++)
            {
                if (_blackNotes[i] == null) continue;

                // Calculate position relative to previous white key
                double keyX = (xPos - _whiteKeyWidth - KeySpacing) + blackXOffset;
                var key = new PianoKeyViewModel(_blackNotes[i], octave, true, keyX)
                {
                    IsPressed = false
                };
                _viewModel.PianoKeys.Add(key);
            }
        }
    }

    private void CalculateKeyDimensions()
    {
        var width = (float)Canvas.CanvasSize.Width;
        var height = (float)Canvas.CanvasSize.Height;

        if (width <= 0 || height <= 0)
        {
            width = (float)this.Width;
            height = (float)this.Height;
        }

        if (width <= 0 || height <= 0) return;

        _canvasWidth = width;
        _canvasHeight = height;

        // Calculate white key width based on available width and number of white keys (7 per octave * 3 = 21)
        var totalWhiteKeys = 7 * NumberOfOctaves;
        // Account for spacing
        var totalSpacing = (totalWhiteKeys - 1) * KeySpacing;
        _whiteKeyWidth = (width - totalSpacing) / totalWhiteKeys;
        _whiteKeyHeight = height;

        _blackKeyWidth = _whiteKeyWidth * BlackKeyWidthRatio;
        _blackKeyHeight = _whiteKeyHeight * BlackKeyHeightRatio;
    }

    private void SetupPaints()
    {
        // White key gradient paint (ivory to white)
        var whiteShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, _whiteKeyHeight),
            new[] { SKColors.Ivory, SKColors.White, SKColors.LightGray },
            null,
            SKShaderTileMode.Clamp);

        _whiteKeyPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Shader = whiteShader
        };

        // Black key gradient paint
        var blackShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, _blackKeyHeight),
            new[] { SKColors.DarkSlateGray, SKColors.Black, SKColors.DimGray },
            null,
            SKShaderTileMode.Clamp);

        _blackKeyPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Shader = blackShader
        };

        // Pressed white key (gold)
        _pressedWhiteKeyPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#FFD700"),
            IsAntialias = true
        };

        // Pressed black key (gold)
        _pressedBlackKeyPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#FFD700"),
            IsAntialias = true
        };

        // Shadow paint for depth
        _shadowPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0, 0, 0, 50),
            IsAntialias = true
        };
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.DarkGray);

        if (_viewModel?.PianoKeys == null || _viewModel.PianoKeys.Count == 0)
            return;

        // Ensure dimensions are calculated
        if (_whiteKeyWidth <= 0 || _canvasWidth <= 0)
        {
            CalculateKeyDimensions();
            InitializeKeyPositions();
        }

        // Draw white keys first (background layer)
        foreach (var key in _viewModel.PianoKeys.Where(k => !k.IsBlackKey))
        {
            var x = (float)key.XPosition;
            DrawKey(canvas, x, 0, _whiteKeyWidth, _whiteKeyHeight, _whiteKeyPaint!, _pressedWhiteKeyPaint!, key.IsPressed, cornerRadius: 6);
            }

        // Draw black keys on top
        foreach (var key in _viewModel.PianoKeys.Where(k => k.IsBlackKey))
            {
            var x = (float)key.XPosition;
            DrawKey(canvas, x, 0, _blackKeyWidth, _blackKeyHeight, _blackKeyPaint!, _pressedBlackKeyPaint!, key.IsPressed, cornerRadius: 4);
        }
    }

    private void DrawKey(SKCanvas canvas, float x, float y, float width, float height, SKPaint normalPaint, SKPaint pressedPaint, bool isPressed, float cornerRadius)
    {
        // Draw shadow for 3D effect
        var shadowRect = new SKRect(x + 3, y + 3, x + width - 3, y + height - 5);
        canvas.DrawRoundRect(shadowRect, cornerRadius, cornerRadius, _shadowPaint);

        // Draw key fill
        var keyRect = new SKRect(x, y, x + width, y + height);
        var paint = isPressed ? pressedPaint : normalPaint;
        canvas.DrawRoundRect(keyRect, cornerRadius, cornerRadius, paint);

        // Draw key edge (optional)
        if (!isPressed)
        {
            using var edgePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.LightGray,
                StrokeWidth = 1,
                IsAntialias = true
            };
            canvas.DrawRoundRect(keyRect, cornerRadius, cornerRadius, edgePaint);
        }
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        if (_viewModel == null) return;

        // Map touch X coordinate to piano key
        var x = (float)e.Location.X;

        // Find the key at this X position
        var key = FindKeyAtPosition(x);
        if (key != null)
        {
            var index = _viewModel.PianoKeys.IndexOf(key);
            if (index >= 0)
            {
                if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
                {
                    _viewModel.PianoKeyPressedCommand.Execute(index);
                }
                else if (e.ActionType == SKTouchAction.Released)
                {
                    _viewModel.PianoKeyReleasedCommand.Execute(index);
                }
            }
        }

        e.Handled = true;
    }

    private PianoKeyViewModel? FindKeyAtPosition(float x)
    {
        if (_viewModel == null) return null;

        // Find the key whose XPosition and width contain the touch point
        foreach (var key in _viewModel.PianoKeys)
        {
            float width = key.IsBlackKey ? _blackKeyWidth : _whiteKeyWidth;
            if (x >= key.XPosition && x < key.XPosition + width)
                return key;
        }
        return null;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.PianoKeys) ||
            (e.PropertyName?.StartsWith("PianoKeys[") == true))
        {
            MainThread.BeginInvokeOnMainThread(() => Canvas.InvalidateSurface());
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is MainViewModel vm)
        {
            _viewModel = vm;
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        // Ensure paints are set up after control is fully loaded
        if (_whiteKeyPaint == null)
            SetupPaints();
    }
}
