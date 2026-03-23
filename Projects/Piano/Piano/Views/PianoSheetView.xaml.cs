using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Piano.ViewModels;

namespace Piano.Views;

/// <summary>
/// SkiaSharp-based piano sheet music view
/// Supports Full Sheet mode and Play mode with green highlight
/// </summary>
public partial class PianoSheetView : ContentView
{
    private MainViewModel? _viewModel;
    private float _canvasWidth;
    private float _canvasHeight;
    private SKPaint? _backgroundPaint;
    private SKPaint? _notePaint;
    private SKPaint? _playingNotePaint;
    private SKPaint? _textPaint;
    private SKPaint? _measureLinePaint;
    private SKPaint? _currentMeasurePaint;

    // Layout constants
    private const int NoteHeight = 30;
    private const int NoteSpacing = 8;
    private const int RowSpacing = 40;
    private const int StaffLineSpacing = 10;
    private const float TimeScale = 0.05f; // Pixels per millisecond

    public PianoSheetView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
        this.SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
        {
            _viewModel = vm;
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }
        SetupPaints();
        Canvas.InvalidateSurface();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        _canvasWidth = (float)Canvas.CanvasSize.Width;
        _canvasHeight = (float)Canvas.CanvasSize.Height;

        if (_canvasWidth <= 0 || _canvasHeight <= 0)
        {
            _canvasWidth = (float)this.Width;
            _canvasHeight = (float)this.Height;
        }

        Canvas.InvalidateSurface();
    }

    private void SetupPaints()
    {
        // Note block paint (default blue)
        var noteShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, NoteHeight),
            new[] { SKColor.Parse("#4A90E2"), SKColor.Parse("#357ABD") },
            null,
            SKShaderTileMode.Clamp);

        _notePaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Shader = noteShader
        };

        // Playing note paint (bright green)
        _playingNotePaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#00FF00"),
            IsAntialias = true
        };

        // Text paint
        _textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 14,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };

        // Measure line paint
        _measureLinePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Gray,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
        };

        // Current measure highlight paint
        _currentMeasurePaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#2A2A2A"),
            IsAntialias = true
        };

        // Background paint
        _backgroundPaint = new SKPaint
        {
            Color = SKColor.Parse("#1E1E1E")
        };
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (_viewModel == null || _viewModel.DisplayNotes.Count == 0)
        {
            DrawEmptyState(canvas);
            return;
        }

        var displayNotes = _viewModel.DisplayNotes.ToList();

        // Calculate total height needed
        var measures = displayNotes.GroupBy(n => n.MeasureNumber).ToList();
        var totalHeight = measures.Count * RowSpacing + 50; // 50px padding top for title
        var startY = 50f;

        // Draw title
        if (!string.IsNullOrEmpty(_viewModel.SheetTitle))
        {
            using var titlePaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 24,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText(_viewModel.SheetTitle, 10, 30, titlePaint);
        }

        // Draw mode indicator
        using var modePaint = new SKPaint
        {
            Color = _viewModel.IsPlayMode ? SKColors.LightGreen : SKColors.LightBlue,
            TextSize = 12,
            IsAntialias = true
        };
        var modeText = _viewModel.IsPlayMode ? "PLAY MODE" : "FULL SHEET MODE";
        canvas.DrawText(modeText, _canvasWidth - 150, 30, modePaint);

        // Group by measure
        foreach (var measureGroup in measures)
        {
            var measure = measureGroup.Key;
            var y = startY + (measure - 1) * RowSpacing;

            // Draw measure background (highlight in play mode if currently playing)
            if (_viewModel.IsPlayMode && _viewModel.IsPlaying)
            {
                var isCurrentMeasure = _viewModel.CurrentPlayPosition >= measureGroup.First().Offset &&
                                       _viewModel.CurrentPlayPosition <= measureGroup.Last().Offset + measureGroup.Last().Duration;
                if (isCurrentMeasure)
                {
                    var measureRect = new SKRect(0, y - NoteHeight - 10, _canvasWidth, y + 10);
                    canvas.DrawRect(measureRect, _currentMeasurePaint);
                }
            }

            // Draw measure number
            using var measurePaint = new SKPaint
            {
                Color = SKColors.LightGray,
                TextSize = 12,
                IsAntialias = true
            };
            canvas.DrawText($"M{measure}", 10, y, measurePaint);

            // Draw staff lines (5 lines)
            for (int line = 0; line < 5; line++)
            {
                var lineY = y - NoteHeight - 5 + line * 5;
                canvas.DrawLine(30, lineY, _canvasWidth, lineY, _measureLinePaint);
            }

            // Draw notes
            foreach (var displayNote in measureGroup)
            {
                DrawNote(canvas, displayNote, y);
            }
        }

        // Draw scroll indicator if in play mode
        if (_viewModel.IsPlayMode && _viewModel.IsPlaying)
        {
            var playheadY = startY + GetCurrentMeasureProgress() * RowSpacing;
            using var playheadPaint = new SKPaint
            {
                Color = SKColors.Red,
                StrokeWidth = 2,
                IsAntialias = true
            };
            canvas.DrawLine(0, playheadY, _canvasWidth, playheadY, playheadPaint);
        }
    }

    private void DrawNote(SKCanvas canvas, DisplayNote displayNote, float baselineY)
    {
        // Calculate X position based on offset within measure
        var x = 50 + (float)displayNote.Offset * TimeScale;

        // Use different colors based on playing state and mode
        SKPaint paint = displayNote.IsPlaying ? _playingNotePaint! : _notePaint!;

        // Draw note block
        var rect = new SKRect(x, baselineY - NoteHeight, x + Math.Max(20, displayNote.Duration * TimeScale), baselineY);

        // Rounded rectangle for note
        var path = new SKPath();
        path.AddRoundRect(rect, 4, 4);
        canvas.DrawPath(path, paint);

        // Draw note name on top if in Full mode
        if (!_viewModel.IsPlayMode && displayNote.Duration > 200)
        {
            using var noteNamePaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 10,
                IsAntialias = true
            };
            var textX = x + 4;
            var textY = baselineY - NoteHeight + 12;
            canvas.DrawText(displayNote.NoteName, textX, textY, noteNamePaint);
        }
    }

    private void DrawEmptyState(SKCanvas canvas)
    {
        using var textPaint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = 16,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };
        canvas.DrawText("Load a music sheet to begin", 20, _canvasHeight / 2, textPaint);
    }

    private float GetCurrentMeasureProgress()
    {
        if (_viewModel == null || _viewModel.CurrentSheet == null) return 0;

        var sheet = _viewModel.CurrentSheet;
        foreach (var measure in sheet.Measures)
        {
            if (_viewModel.CurrentPlayPosition >= measure.Offset &&
                _viewModel.CurrentPlayPosition < measure.Offset + measure.Duration)
            {
                var progress = (float)(_viewModel.CurrentPlayPosition - measure.Offset) / measure.Duration;
                return measure.Number - 1 + progress;
            }
        }
        return 0;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.DisplayNotes) ||
            e.PropertyName == nameof(MainViewModel.IsPlayMode) ||
            e.PropertyName == nameof(MainViewModel.IsPlaying) ||
            e.PropertyName == nameof(MainViewModel.CurrentPlayPosition) ||
            e.PropertyName == nameof(MainViewModel.SheetTitle))
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
}
