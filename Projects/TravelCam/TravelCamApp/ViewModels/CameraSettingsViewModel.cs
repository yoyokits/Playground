// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //
//
// CameraSettingsViewModel: All camera display / capture settings.
// Every property is persisted immediately to Android SharedPreferences.
//
// Settings:
//   • ShowRuleOfThirds   — rule-of-thirds guide lines toggle
//   • ShowDataOverlay  — sensor data panel toggle
//   • GridLineOpacity    — guide line opacity (5–50 %)
//   • SelectedAspectRatio — crop overlay applied to the preview
//   • SelectedResolutionIndex — index into AvailableResolutionLabels (0 = Auto)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace TravelCamApp.ViewModels
{
    public enum AspectRatioOption { FullScreen, FourThree, SixteenNine, OneOne }

    public class CameraSettingsViewModel : INotifyPropertyChanged
    {
        #region Fields

        private bool _showRuleOfThirds = true;
        private bool _showDataOverlay = true;
        private double _gridLineOpacity = 0.18;
        private AspectRatioOption _selectedAspectRatio = AspectRatioOption.FullScreen;

        private List<string> _availableResolutionLabels = new() { "Auto" };
        private List<Size> _resolutionSizes = new();
        private int _selectedResolutionIndex = 0;

        private const string PrefShowGrid         = "CamShowGrid";
        private const string PrefShowSensors      = "CamShowSensors";
        private const string PrefGridOpacity      = "CamGridOpacity";
        private const string PrefAspectRatio      = "CamAspectRatio";
        private const string PrefResolutionIndex  = "CamResolutionIndex";

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Show or hide the rule-of-thirds guide lines on the camera preview.</summary>
        public bool ShowRuleOfThirds
        {
            get => _showRuleOfThirds;
            set
            {
                if (_showRuleOfThirds == value) return;
                _showRuleOfThirds = value;
                OnPropertyChanged();
                Preferences.Set(PrefShowGrid, value);
            }
        }

        /// <summary>Show or hide the data overlay panel.</summary>
        public bool ShowDataOverlay
        {
            get => _showDataOverlay;
            set
            {
                if (_showDataOverlay == value) return;
                _showDataOverlay = value;
                OnPropertyChanged();
                Preferences.Set(PrefShowSensors, value);
            }
        }

        /// <summary>Selected aspect ratio for the camera preview crop overlay.</summary>
        public AspectRatioOption SelectedAspectRatio
        {
            get => _selectedAspectRatio;
            set
            {
                if (_selectedAspectRatio == value) return;
                _selectedAspectRatio = value;
                OnPropertyChanged();
                Preferences.Set(PrefAspectRatio, (int)value);
            }
        }

        /// <summary>Opacity of the rule-of-thirds grid lines (0.05–0.50).</summary>
        public double GridLineOpacity
        {
            get => _gridLineOpacity;
            set
            {
                if (_gridLineOpacity == value) return;
                _gridLineOpacity = System.Math.Clamp(value, 0.05, 0.50);
                OnPropertyChanged();
                OnPropertyChanged(nameof(GridLineColor));
                Preferences.Set(PrefGridOpacity, _gridLineOpacity);
            }
        }

        /// <summary>Computed ARGB color for the grid lines, driven by GridLineOpacity.</summary>
        public Microsoft.Maui.Graphics.Color GridLineColor =>
            Microsoft.Maui.Graphics.Color.FromRgba(1f, 1f, 1f, (float)_gridLineOpacity);

        /// <summary>
        /// Resolution labels shown in the Picker.
        /// Index 0 is always "Auto" (highest available). Populated when the camera is ready.
        /// </summary>
        public List<string> AvailableResolutionLabels
        {
            get => _availableResolutionLabels;
            private set { _availableResolutionLabels = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Index into AvailableResolutionLabels. 0 = Auto (largest resolution).
        /// Saved to SharedPreferences immediately on change.
        /// </summary>
        public int SelectedResolutionIndex
        {
            get => _selectedResolutionIndex;
            set
            {
                var clamped = System.Math.Clamp(value, 0, System.Math.Max(0, _availableResolutionLabels.Count - 1));
                if (_selectedResolutionIndex == clamped) return;
                _selectedResolutionIndex = clamped;
                OnPropertyChanged();
                Preferences.Set(PrefResolutionIndex, clamped);
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Sets the selected aspect ratio from a string CommandParameter
        /// (e.g. "FullScreen", "FourThree", "SixteenNine", "OneOne").
        /// </summary>
        public ICommand SetAspectRatioCommand { get; }

        #endregion

        #region Constructor / Persistence

        public CameraSettingsViewModel()
        {
            SetAspectRatioCommand = new Command<string>(p =>
            {
                if (System.Enum.TryParse<AspectRatioOption>(p, out var ratio))
                    SelectedAspectRatio = ratio;
            });
            LoadPersistedSettings();
        }

        private void LoadPersistedSettings()
        {
            _showRuleOfThirds    = Preferences.Get(PrefShowGrid,       true);
            _showDataOverlay   = Preferences.Get(PrefShowSensors,    true);
            _gridLineOpacity     = Preferences.Get(PrefGridOpacity,     0.18);
            _selectedAspectRatio = (AspectRatioOption)Preferences.Get(PrefAspectRatio, (int)AspectRatioOption.FullScreen);
            // Resolution index is restored in SetAvailableResolutions once the
            // camera reports its supported sizes — we cannot restore it here
            // because the list isn't populated yet.
        }

        #endregion

        #region Resolution API

        /// <summary>
        /// Populates the resolution picker from the camera's supported sizes.
        /// Resolutions are sorted highest-first. Index 0 is "Auto" (= highest).
        /// Call this whenever a new camera is selected (front/rear toggle or first init).
        /// </summary>
        public void SetAvailableResolutions(IReadOnlyList<Size> sizes)
        {
            if (sizes == null || sizes.Count == 0)
            {
                _resolutionSizes = new List<Size>();
                AvailableResolutionLabels = new List<string> { "Auto" };
                _selectedResolutionIndex = 0;
                OnPropertyChanged(nameof(SelectedResolutionIndex));
                return;
            }

            // Sort highest total pixels first so index 0 (Auto) = best quality
            _resolutionSizes = sizes
                .OrderByDescending(s => (long)s.Width * (long)s.Height)
                .ToList();

            var labels = new List<string>(capacity: _resolutionSizes.Count + 1) { "Auto" };
            labels.AddRange(_resolutionSizes.Select(s => $"{(int)s.Width} × {(int)s.Height}"));

            // Set backing field and notify so Picker gets the new list before index is set
            _availableResolutionLabels = labels;
            OnPropertyChanged(nameof(AvailableResolutionLabels));

            // Restore saved index, clamped to the new list size
            var saved = Preferences.Get(PrefResolutionIndex, 0);
            _selectedResolutionIndex = System.Math.Clamp(saved, 0, labels.Count - 1);
            OnPropertyChanged(nameof(SelectedResolutionIndex));
        }

        /// <summary>
        /// Returns the explicitly selected resolution size, or null when "Auto" is chosen.
        /// "Auto" means the caller should use the camera's highest available resolution.
        /// </summary>
        public Size? GetSelectedResolutionSize()
        {
            // Index 0 = Auto → return null (caller uses default)
            if (_selectedResolutionIndex <= 0 || _resolutionSizes.Count == 0)
                return null;

            var listIndex = _selectedResolutionIndex - 1; // offset because index 0 is "Auto"
            return listIndex < _resolutionSizes.Count ? _resolutionSizes[listIndex] : null;
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
