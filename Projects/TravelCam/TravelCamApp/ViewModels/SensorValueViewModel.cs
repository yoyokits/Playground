// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// SensorValueViewModel: Owns and manages the observable list of
// SensorItem objects shown on the camera overlay. Subscribes to
// SensorHelper for live data updates and handles settings persistence.
//
// MainPageViewModel holds a reference to this VM and exposes
// SensorItems as a passthrough so existing XAML bindings continue
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
    public class SensorValueViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private readonly SensorHelper _sensorHelper;
        private ObservableCollection<SensorItem> _sensorItems = new();
        private float _fontSize = 12f;
        private bool _isMapOverlayVisible;
        private bool _isDisposed;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SensorItem> SensorItems
        {
            get => _sensorItems;
            private set { _sensorItems = value; OnPropertyChanged(); }
        }

        public float FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(); }
        }

        public bool IsMapOverlayVisible
        {
            get => _isMapOverlayVisible;
            set { _isMapOverlayVisible = value; OnPropertyChanged(); }
        }

        #endregion

        #region Constructor

        public SensorValueViewModel(SensorHelper sensorHelper)
        {
            _sensorHelper = sensorHelper;
            InitializeSensorItems();
            _sensorHelper.SensorDataUpdated += OnSensorDataUpdated;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Loads saved sensor visibility settings from persistent storage.
        /// Call once during app initialization.
        /// </summary>
        public async Task ApplyPersistedSettingsAsync()
        {
            try
            {
                var config = await SettingsHelper.LoadSensorItemsConfigurationAsync();
                if (config != null)
                {
                    SettingsHelper.ApplyConfigurationToSensorItems(
                        new List<SensorItem>(_sensorItems), config);
                }
                else
                {
                    // Default visibility
                    UpdateItem("City", isVisible: true);
                    UpdateItem("Country", isVisible: true);
                    UpdateItem("Temperature", isVisible: true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SensorValueViewModel] ApplyPersistedSettings error: {ex.Message}");
            }
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
                    data.Speed.HasValue ? $"{data.Speed.Value:F1} km/h" : "N/A");
            });
        }

        #endregion

        #region Initialization

        private void InitializeSensorItems()
        {
            _sensorItems = new ObservableCollection<SensorItem>
            {
                new SensorItem("City",        "",                                  isVisible: true),
                new SensorItem("Country",     "",                                  isVisible: true),
                new SensorItem("Temperature", "",                                  isVisible: true),
                new SensorItem("Altitude",    "",                                  isVisible: false),
                new SensorItem("Latitude",    "",                                  isVisible: false),
                new SensorItem("Longitude",   "",                                  isVisible: false),
                new SensorItem("Date",        DateTime.Now.ToString("MM/dd/yyyy"), isVisible: false),
                new SensorItem("Time",        DateTime.Now.ToString("HH:mm:ss"),   isVisible: false),
                new SensorItem("Heading",     "",                                  isVisible: false),
                new SensorItem("Speed",       "",                                  isVisible: false),
            };
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
        }

        #endregion
    }
}
