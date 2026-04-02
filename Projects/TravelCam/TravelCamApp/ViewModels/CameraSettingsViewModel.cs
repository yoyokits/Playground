// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// CameraSettingsViewModel: Manages all camera display/behaviour
// settings that are not sensor-related.
//
// Currently supported:
//   • ShowRuleOfThirds  — toggle the overlay grid
//   • ShowSensorOverlay — toggle the sensor data panel
//   • GridLineOpacity   — how visible the rule-of-thirds lines are
//
// Planned (requires future API support or platform-specific code):
//   • Resolution / AspectRatio picker

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;

namespace TravelCamApp.ViewModels
{
    public class CameraSettingsViewModel : INotifyPropertyChanged
    {
        #region Fields

        private bool _showRuleOfThirds = true;
        private bool _showSensorOverlay = true;
        private double _gridLineOpacity = 0.18;

        private const string PrefShowGrid    = "CamShowGrid";
        private const string PrefShowSensors = "CamShowSensors";
        private const string PrefGridOpacity = "CamGridOpacity";

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

        #region Constructor / Persistence

        public CameraSettingsViewModel()
        {
            LoadPersistedSettings();
        }

        private void LoadPersistedSettings()
        {
            _showRuleOfThirds = Preferences.Get(PrefShowGrid,    true);
            _showSensorOverlay = Preferences.Get(PrefShowSensors, true);
            _gridLineOpacity  = Preferences.Get(PrefGridOpacity,  0.18);
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
