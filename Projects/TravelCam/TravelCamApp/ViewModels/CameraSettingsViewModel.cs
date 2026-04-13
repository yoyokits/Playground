// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokids       //
// ========================================== //
//
// CameraSettingsViewModel: Manages all camera display/behaviour
// settings that are not sensor-related.
//
// Supported:
//   • ShowRuleOfThirds  — toggle the overlay grid
//   • ShowSensorOverlay — toggle the sensor data panel
//   • GridLineOpacity   — how visible the rule-of-thirds lines are
//   • SelectedAspectRatio — visual crop applied to the camera preview

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace TravelCamApp.ViewModels
{
    public enum AspectRatioOption { FullScreen, FourThree, SixteenNine, OneOne }

    public class CameraSettingsViewModel : INotifyPropertyChanged
    {
        #region Fields

        private bool _showRuleOfThirds = true;
        private bool _showSensorOverlay = true;
        private double _gridLineOpacity = 0.18;
        private AspectRatioOption _selectedAspectRatio = AspectRatioOption.FullScreen;

        private const string PrefShowGrid      = "CamShowGrid";
        private const string PrefShowSensors   = "CamShowSensors";
        private const string PrefGridOpacity   = "CamGridOpacity";
        private const string PrefAspectRatio   = "CamAspectRatio";

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

        /// <summary>Show or hide the sensor data overlay panel.</summary>
        public bool ShowSensorOverlay
        {
            get => _showSensorOverlay;
            set
            {
                if (_showSensorOverlay == value) return;
                _showSensorOverlay = value;
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

        /// <summary>Opacity of the rule-of-thirds grid lines (0.05 – 0.50).</summary>
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
            _showRuleOfThirds  = Preferences.Get(PrefShowGrid,    true);
            _showSensorOverlay = Preferences.Get(PrefShowSensors, true);
            _gridLineOpacity   = Preferences.Get(PrefGridOpacity,  0.18);
            _selectedAspectRatio = (AspectRatioOption)Preferences.Get(PrefAspectRatio, (int)AspectRatioOption.FullScreen);
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
