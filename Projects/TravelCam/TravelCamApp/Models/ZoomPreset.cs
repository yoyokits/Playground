// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace TravelCamApp.Models
{
    /// <summary>
    /// Represents a single selectable zoom level in the camera zoom-pill strip.
    /// Each preset holds its own SelectCommand so the BindableLayout DataTemplate
    /// can invoke it without ancestor-binding gymnastics.
    /// The command captures <c>this</c> and calls back into the ViewModel via the
    /// <see cref="Action{ZoomPreset}"/> delegate supplied at construction time.
    /// </summary>
    public class ZoomPreset : INotifyPropertyChanged
    {
        #region Fields

        private bool _isSelected;

        #endregion

        #region Properties

        public string Label { get; }

        /// <summary>
        /// Absolute zoom factor matching <c>CameraView.ZoomFactor</c>
        /// (e.g. 1.0 = 1×, 2.0 = 2×).  Range: camera MinZoomFactor … MaxZoomFactor.
        /// </summary>
        public float AbsoluteZoom { get; }

        /// <summary>Parameterless command — calls onSelect(this) on the ViewModel.</summary>
        public ICommand SelectCommand { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                RaiseChanged(nameof(IsSelected));
                RaiseChanged(nameof(PillBackground));
                RaiseChanged(nameof(LabelColor));
                RaiseChanged(nameof(LabelSize));
            }
        }

        // Bindable visual properties driven by IsSelected
        public Color PillBackground => IsSelected
            ? Color.FromArgb("#55FFFFFF")
            : Colors.Transparent;

        public Color LabelColor => IsSelected
            ? Colors.White
            : Color.FromArgb("#99FFFFFF");

        public double LabelSize => IsSelected ? 12 : 11;

        #endregion

        #region Constructor

        /// <param name="label">Display text, e.g. "1×", "2", ".6".</param>
        /// <param name="absoluteZoom">Camera zoom factor (same unit as CameraView.ZoomFactor).</param>
        /// <param name="onSelect">ViewModel callback invoked when this pill is tapped.</param>
        public ZoomPreset(string label, float absoluteZoom, Action<ZoomPreset> onSelect)
        {
            Label = label;
            AbsoluteZoom = absoluteZoom;
            // Capture 'this' — safe because the command runs after construction completes
            SelectCommand = new Command(() => onSelect(this));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaiseChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
