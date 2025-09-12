// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using LLM.Settings;
    using WorldMapControls.Services; // for JsonInputExtractor
    using WorldMapControls.Controls;

    public partial class MainWindow : Window
    {
        private DateTime _lastPersist = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            ApplySavedBounds();
            ApplyMapSettings();
            if (Viewer != null) 
            {
                Viewer.OutlineThicknessChanged += Viewer_OutlineThicknessChanged;
                Viewer.OutlineColorChanged += Viewer_OutlineColorChanged;
                Viewer.DefaultFillColorChanged += Viewer_DefaultFillColorChanged;
            }
            Closing += OnClosingSaveWindowBounds;
            LocationChanged += (_, _) => PersistBoundsThrottled();
            SizeChanged += (_, _) => PersistBoundsThrottled();
            StateChanged += (_, _) => PersistBoundsThrottled();
            if (LlmChat != null) LlmChat.OutputSelected += OnChatOutputSelected; // hook chat output
            SettingsService.SettingsChanged += SettingsService_SettingsChanged;
        }

        private void Viewer_OutlineThicknessChanged(object? sender, double e)
        {
            var s = SettingsService.Current;
            if (Math.Abs(s.OutlineThickness - e) > 0.0001)
            {
                s.OutlineThickness = e;
                SettingsService.RaiseChangedAndSave();
            }
        }

        private void Viewer_OutlineColorChanged(object? sender, System.Windows.Media.Color e)
        {
            var hexColor = $"#{e.R:X2}{e.G:X2}{e.B:X2}";
            SettingsService.SetOutlineColor(hexColor);
        }

        private void Viewer_DefaultFillColorChanged(object? sender, System.Windows.Media.Color e)
        {
            var hexColor = $"#{e.R:X2}{e.G:X2}{e.B:X2}";
            SettingsService.SetDefaultFillColor(hexColor);
        }

        private void SettingsService_SettingsChanged(object? sender, EventArgs e)
        {
            ApplyMapSettings();
        }

        private void ApplyMapSettings()
        {
            if (Viewer == null) return;
            var s = SettingsService.Current;
            
            // Apply thickness using the new method
            if (Math.Abs(Viewer.GetOutlineThickness() - s.OutlineThickness) > 0.001)
            {
                Viewer.SetOutlineThickness(s.OutlineThickness);
            }
            
            // Apply outline color
            if (!string.IsNullOrWhiteSpace(s.OutlineColor))
            {
                try
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(s.OutlineColor);
                    Viewer.SetOutlineColor(color);
                }
                catch
                {
                    // Invalid color format, ignore
                }
            }
            
            // Apply default fill color
            if (!string.IsNullOrWhiteSpace(s.DefaultFillColor))
            {
                try
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(s.DefaultFillColor);
                    Viewer.SetDefaultFillColor(color);
                }
                catch
                {
                    // Invalid color format, ignore
                }
            }
        }

        private void OnChatOutputSelected(object? sender, string text)
        {
            // Try extract JSON region mapping and apply to map viewer
            var json = JsonInputExtractor.ExtractJson(text);
            Viewer.Json = json; // can be null -> clears map overrides internally
        }

        private void ApplySavedBounds()
        {
            var s = SettingsService.Current;
            if (s.WindowWidth > 0 && s.WindowHeight > 0)
            { Width = s.WindowWidth; Height = s.WindowHeight; }
            if (s.WindowLeft >= 0 && s.WindowTop >= 0)
            { Left = s.WindowLeft; Top = s.WindowTop; }
            if (s.WindowMaximized)
                WindowState = WindowState.Maximized;
        }

        private void PersistBoundsThrottled()
        {
            if ((DateTime.UtcNow - _lastPersist).TotalMilliseconds < 250) return;
            _lastPersist = DateTime.UtcNow;
            PersistWindowBounds();
            if (!SettingsService.ApplicationOwnsPersistence)
                _ = SettingsService.SaveAsync();
        }

        public void PersistWindowBounds()
        {
            var s = SettingsService.Current;
            s.WindowMaximized = WindowState == WindowState.Maximized;
            if (WindowState == WindowState.Normal)
            {
                s.WindowWidth = Width;
                s.WindowHeight = Height;
                s.WindowLeft = Left;
                s.WindowTop = Top;
            }
            else
            {
                var b = RestoreBounds; // previous normal bounds
                s.WindowWidth = b.Width;
                s.WindowHeight = b.Height;
                s.WindowLeft = b.Left;
                s.WindowTop = b.Top;
            }
        }

        private void OnClosingSaveWindowBounds(object? sender, CancelEventArgs e)
        {
            PersistWindowBounds();
            if (!SettingsService.ApplicationOwnsPersistence)
                _ = SettingsService.SaveAsync();
            SettingsService.SettingsChanged -= SettingsService_SettingsChanged;
            if (Viewer != null) 
            {
                Viewer.OutlineThicknessChanged -= Viewer_OutlineThicknessChanged;
                Viewer.OutlineColorChanged -= Viewer_OutlineColorChanged;
                Viewer.DefaultFillColorChanged -= Viewer_DefaultFillColorChanged;
            }
        }
    }
}