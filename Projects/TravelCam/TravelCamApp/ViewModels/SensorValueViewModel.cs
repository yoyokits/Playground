// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TravelCamApp.Models;

namespace TravelCamApp.ViewModels
{
    /// <summary>
    /// Represents a sensor display item for the UI
    /// </summary>
    public class SensorDisplayItem : INotifyPropertyChanged
    {
        #region Fields

        private static readonly IReadOnlyDictionary<string, string> DefaultValues =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["City"] = "Jakarta",
                ["Country"] = "Indonesia",
                ["Temperature"] = "28.0°C",
                ["Altitude"] = "12m",
                ["Latitude"] = "-6.2088",
                ["Longitude"] = "106.8456",
                ["Date"] = DateTime.Now.ToString("MM/dd/yyyy"),
                ["Time"] = DateTime.Now.ToString("HH:mm:ss"),
                ["Heading"] = "0",
                ["Speed"] = "0"
            };

        private string _value;

        #endregion Fields

        #region Constructors

        public SensorDisplayItem(string name, string value)
        {
            Name = name;
            _value = value;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Events

        #region Properties

        public string DisplayName => FormattedValue;

        public string FormattedValue
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Value))
                {
                    return Value;
                }

                return DefaultValues.TryGetValue(Name, out var fallback)
                    ? fallback
                    : string.Empty;
            }
        }

        public string Name { get; }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                    OnPropertyChanged(nameof(FormattedValue));
                }
            }
        }

        #endregion Properties

        #region Methods

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }

    /// <summary>
    /// ViewModel for sensor value display
    /// </summary>
    public class SensorValueViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly Dictionary<string, SensorDisplayItem> _sensorDisplayItemsMap;
        private ObservableCollection<SensorDisplayItem> _visibleSensorDisplayItems;

        #endregion Fields

        #region Constructors

        public SensorValueViewModel()
        {
            _sensorDisplayItemsMap = new Dictionary<string, SensorDisplayItem>(StringComparer.OrdinalIgnoreCase);
            InitializeSensorDisplayItems();
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Events

        #region Properties

        public IReadOnlyDictionary<string, SensorDisplayItem> AvailableSensorDisplayItems => _sensorDisplayItemsMap;

        public ObservableCollection<SensorDisplayItem> VisibleSensorDisplayItems
        {
            get => _visibleSensorDisplayItems;
            set
            {
                if (_visibleSensorDisplayItems != value)
                {
                    _visibleSensorDisplayItems = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets all visible sensor display items as a formatted string for display
        /// </summary>
        /// <returns>Formatted string of visible sensor values</returns>
        public string GetVisibleSensorValuesAsString()
        {
            if (_visibleSensorDisplayItems == null || !_visibleSensorDisplayItems.Any()) return "";

            return string.Join("\n", _visibleSensorDisplayItems.Select(item => item.FormattedValue));
        }

        /// <summary>
        /// Updates sensor display items from sensor data
        /// </summary>
        /// <param name="sensorData">The sensor data to update from</param>
        public void UpdateFromSensorData(SensorData sensorData)
        {
            if (sensorData == null) return;

            UpdateSensorDisplayItemValue("City", sensorData.City ?? string.Empty);
            UpdateSensorDisplayItemValue("Country", sensorData.Country ?? string.Empty);
            UpdateSensorDisplayItemValue("Temperature", sensorData.Temperature.HasValue ? $"{sensorData.Temperature.Value:F1}°C" : string.Empty);
            UpdateSensorDisplayItemValue("Altitude", sensorData.Altitude.HasValue ? $"{sensorData.Altitude.Value:F0}m" : string.Empty);
            UpdateSensorDisplayItemValue("Latitude", sensorData.Latitude != 0 ? sensorData.Latitude.ToString() : string.Empty);
            UpdateSensorDisplayItemValue("Longitude", sensorData.Longitude != 0 ? sensorData.Longitude.ToString() : string.Empty);
            UpdateSensorDisplayItemValue("Date", sensorData.Timestamp.ToString("MM/dd/yyyy"));
            UpdateSensorDisplayItemValue("Time", sensorData.Timestamp.ToString("HH:mm:ss"));
            UpdateSensorDisplayItemValue("Heading", sensorData.Heading.HasValue ? sensorData.Heading.Value.ToString("F0") : string.Empty);
            UpdateSensorDisplayItemValue("Speed", sensorData.Speed.HasValue ? sensorData.Speed.Value.ToString("F1") : string.Empty);
        }

        /// <summary>
        /// Updates the visibility of a sensor display item by name
        /// </summary>
        /// <param name="name">The name of the sensor display item to update</param>
        /// <param name="isVisible">Whether the sensor display item should be visible</param>
        public void UpdateSensorDisplayItemVisibility(string name, bool isVisible)
        {
            if (_sensorDisplayItemsMap.TryGetValue(name, out var sensorItem))
            {
                if (isVisible)
                {
                    if (!_visibleSensorDisplayItems.Contains(sensorItem))
                    {
                        _visibleSensorDisplayItems.Add(sensorItem);
                    }
                }
                else
                {
                    _visibleSensorDisplayItems.Remove(sensorItem);
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddSensorDisplayItem(SensorDisplayItem item, bool isVisible)
        {
            _sensorDisplayItemsMap[item.Name] = item;
            if (isVisible)
            {
                _visibleSensorDisplayItems.Add(item);
            }
        }

        private void InitializeSensorDisplayItems()
        {
            _visibleSensorDisplayItems = new ObservableCollection<SensorDisplayItem>();
            AddSensorDisplayItem(new SensorDisplayItem("City", "Jakarta"), true);
            AddSensorDisplayItem(new SensorDisplayItem("Country", "Indonesia"), true);
            AddSensorDisplayItem(new SensorDisplayItem("Temperature", "28.0°C"), true);
            AddSensorDisplayItem(new SensorDisplayItem("Altitude", "12m"), false);
            AddSensorDisplayItem(new SensorDisplayItem("Latitude", ""), false);
            AddSensorDisplayItem(new SensorDisplayItem("Longitude", ""), false);
            AddSensorDisplayItem(new SensorDisplayItem("Date", DateTime.Now.ToString("MM/dd/yyyy")), false);
            AddSensorDisplayItem(new SensorDisplayItem("Time", DateTime.Now.ToString("HH:mm:ss")), false);
            AddSensorDisplayItem(new SensorDisplayItem("Heading", ""), false);
            AddSensorDisplayItem(new SensorDisplayItem("Speed", ""), false);
        }

        /// <summary>
        /// Updates the value of a sensor display item by name
        /// </summary>
        /// <param name="name">The name of the sensor display item to update</param>
        /// <param name="value">The new value for the sensor display item</param>
        private void UpdateSensorDisplayItemValue(string name, string value)
        {
            if (_sensorDisplayItemsMap.TryGetValue(name, out var sensorItem))
            {
                sensorItem.Value = value;
            }
        }

        #endregion Methods
    }
}