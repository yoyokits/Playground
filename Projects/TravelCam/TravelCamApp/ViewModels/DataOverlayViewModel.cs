// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// DataOverlayViewModel: Owns and manages the observable list of
// OverlayItem objects shown on the camera overlay. Subscribes to
// SensorHelper for live data updates and handles settings persistence.
//
// MainPageViewModel holds a reference to this VM and exposes
// OverlayItems as a passthrough so existing XAML bindings continue
// to work unchanged.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TravelCamApp.Helpers;
using TravelCamApp.Models;

namespace TravelCamApp.ViewModels
{
    public class DataOverlayViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private readonly SensorHelper _sensorHelper;
        private ObservableCollection<OverlayItem> _sensorItems = new();
        private ObservableCollection<OverlayItem> _visibleItems = new();
        private float _fontSize = 12f;
        private bool _isMapOverlayVisible;
        private bool _isDisposed;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<OverlayItem> OverlayItems
        {
            get => _sensorItems;
            private set { _sensorItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<OverlayItem> VisibleOverlayItems
        {
            get => _visibleItems;
            private set { _visibleItems = value; OnPropertyChanged(); }
        }

        /// <summary>Base font size from the Label Size slider (8–24 pt).</summary>
        public float FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) return;
                _fontSize = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LabelFontSize));
                OnPropertyChanged(nameof(ValueFontSize));
            }
        }

        /// <summary>Font size for sensor name labels (≈70 % of base).</summary>
        public float LabelFontSize => MathF.Round(_fontSize * 0.70f, 0);

        /// <summary>Font size for sensor value labels (equals the base size).</summary>
        public float ValueFontSize => _fontSize;

        public bool IsMapOverlayVisible
        {
            get => _isMapOverlayVisible;
            set { _isMapOverlayVisible = value; OnPropertyChanged(); }
        }

        #endregion

        #region Constructor

        public DataOverlayViewModel(SensorHelper sensorHelper)
        {
            _sensorHelper = sensorHelper;
            InitializeOverlayItems();
            _sensorHelper.SensorDataUpdated += OnSensorDataUpdated;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Rebuilds VisibleOverlayItems from OverlayItems in their current order.
        /// Call after the settings panel reorders or changes visibility.
        /// </summary>
        public void RefreshVisibleItems()
        {
            _visibleItems.Clear();
            foreach (var item in _sensorItems)
            {
                if (item.IsVisible)
                    _visibleItems.Add(item);
            }
        }

        /// <summary>
        /// Loads saved sensor visibility settings from persistent storage.
        /// Call once during app initialization.
        /// </summary>
        public async Task ApplyPersistedSettingsAsync()
        {
            try
            {
                var config = await SettingsHelper.LoadOverlayItemsConfigurationAsync();
                if (config != null)
                {
                    // Pass _sensorItems directly so Move() reorders the live collection.
                    SettingsHelper.ApplyConfigurationToOverlayItems(_sensorItems, config);

                    // Restore font size from saved settings.
                    if (config.FontSize > 0)
                        FontSize = config.FontSize;
                }
                else
                {
                    // Default visibility on first run
                    UpdateItem("City", isVisible: true);
                    UpdateItem("Country", isVisible: true);
                    UpdateItem("Temperature", isVisible: true);
                }

                // Rebuild VisibleOverlayItems in the current (possibly reordered) sequence.
                RefreshVisibleItems();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[DataOverlayViewModel] ApplyPersistedSettings error: {ex.Message}");
            }
        }

        #endregion

        #region Overlay Item Event Handling

        private void OnOverlayItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not OverlayItem item || e.PropertyName != nameof(OverlayItem.IsVisible))
                return;

            if (item.IsVisible && !_visibleItems.Contains(item))
                _visibleItems.Add(item);
            else if (!item.IsVisible && _visibleItems.Contains(item))
                _visibleItems.Remove(item);
        }

        #endregion

        #region Sensor Data Updates

        private void OnSensorDataUpdated(Models.SensorData data)
        {
            if (_isDisposed) return;

            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_isDisposed) return;

                UpdateItem("Temperature",
                    data.Temperature.HasValue ? $"{data.Temperature.Value:F1}\u00b0C" : "N/A");
                UpdateItem("City", data.City ?? "Unknown");
                UpdateItem("Country", data.Country ?? "Unknown");
                UpdateItem("Altitude",
                    data.Altitude.HasValue ? $"{data.Altitude.Value:F0}m" : "N/A");
                UpdateItem("Latitude",
                    data.Latitude.ToString(CultureInfo.InvariantCulture));
                UpdateItem("Longitude",
                    data.Longitude.ToString(CultureInfo.InvariantCulture));
                UpdateItem("Date", data.Timestamp.ToString("MM/dd/yyyy"));
                UpdateItem("Time", data.Timestamp.ToString("HH:mm:ss"));
                UpdateItem("Heading",
                    data.Heading.HasValue ? $"{data.Heading.Value:F0}\u00b0" : "N/A");
                UpdateItem("Speed",
                    data.Speed.HasValue ? $"{data.Speed.Value * 3.6:F1} km/h" : "N/A");
            });
        }

        #endregion

        #region Initialization

        private void InitializeOverlayItems()
        {
            _sensorItems = new ObservableCollection<OverlayItem>
            {
                new OverlayItem("City",        "",                                  isVisible: true),
                new OverlayItem("Country",     "",                                  isVisible: true),
                new OverlayItem("Temperature", "",                                  isVisible: true),
                new OverlayItem("Altitude",    "",                                  isVisible: false),
                new OverlayItem("Latitude",    "",                                  isVisible: false),
                new OverlayItem("Longitude",   "",                                  isVisible: false),
                new OverlayItem("Date",        DateTime.Now.ToString("MM/dd/yyyy"), isVisible: false),
                new OverlayItem("Time",        DateTime.Now.ToString("HH:mm:ss"),   isVisible: false),
                new OverlayItem("Heading",     "",                                  isVisible: false),
                new OverlayItem("Speed",       "",                                  isVisible: false),
            };

            // Subscribe to visibility changes and populate visible items
            foreach (var item in _sensorItems)
            {
                item.PropertyChanged += OnOverlayItemPropertyChanged;
                if (item.IsVisible)
                    _visibleItems.Add(item);
            }
        }

        #endregion

        #region Helpers

        private void UpdateItem(string name, string? value = null, bool? isVisible = null)
        {
            var item = _sensorItems.FirstOrDefault(si => si.Name == name);
            if (item == null) return;
            if (value != null) item.Value = value;
            if (isVisible.HasValue) item.IsVisible = isVisible.Value;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _sensorHelper.SensorDataUpdated -= OnSensorDataUpdated;
            foreach (var item in _sensorItems)
            {
                item.PropertyChanged -= OnOverlayItemPropertyChanged;
            }
        }

        #endregion
    }
}
       