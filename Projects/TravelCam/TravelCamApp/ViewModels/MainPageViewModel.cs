using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using TravelCamApp.Models;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Essentials;

namespace TravelCamApp.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ImageSource? _lastCaptureImage = ImageSource.FromFile("dotnet_bot.png");
        private bool _isRecording;
        private string _recordingTimeText = "00:00";
        private CaptureMode _selectedMode = CaptureMode.Photo;
        private string _location;
        private string _sensorData;
        private string _temperature = "28°C";
        private string _city = "Jakarta";
        private string _altitude = "12m";
        private string _country = "Indonesia";
        private ObservableCollection<SensorItem> _sensorItems;
        private SensorValueViewModel _sensorValueViewModel;

        #endregion

        #region Properties

        public ImageSource? LastCaptureImage
        {
            get => _lastCaptureImage;
            set
            {
                if (Equals(_lastCaptureImage, value))
                {
                    return;
                }

                _lastCaptureImage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (_isRecording == value)
                {
                    return;
                }

                _isRecording = value;
                OnPropertyChanged();
            }
        }

        public CaptureMode SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (_selectedMode == value)
                {
                    return;
                }

                _selectedMode = value;
                OnPropertyChanged();
            }
        }

        public ICommand SetPhotoModeCommand { get; }

        public ICommand SetVideoModeCommand { get; }

        public string RecordingTimeText
        {
            get => _recordingTimeText;
            set
            {
                if (_recordingTimeText == value)
                {
                    return;
                }

                _recordingTimeText = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if (_location == value)
                {
                    return;
                }

                _location = value;
                OnPropertyChanged();
            }
        }

        public string SensorData
        {
            get => _sensorData;
            set
            {
                if (_sensorData == value)
                {
                    return;
                }

                _sensorData = value;
                OnPropertyChanged();
            }
        }

        public string Temperature
        {
            get => _temperature;
            set
            {
                if (_temperature == value)
                {
                    return;
                }

                _temperature = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (_city == value)
                {
                    return;
                }

                _city = value;
                OnPropertyChanged();
            }
        }

        public string Altitude
        {
            get => _altitude;
            set
            {
                if (_altitude == value)
                {
                    return;
                }

                _altitude = value;
                OnPropertyChanged();
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (_country == value)
                {
                    return;
                }

                _country = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SensorItem> SensorItems
        {
            get => _sensorItems;
            set
            {
                if (_sensorItems != value)
                {
                    _sensorItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public SensorValueViewModel SensorValueViewModel
        {
            get => _sensorValueViewModel;
            set
            {
                if (_sensorValueViewModel != value)
                {
                    _sensorValueViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public MainPageViewModel()
        {
            SetPhotoModeCommand = new Command(() => SelectedMode = CaptureMode.Photo);
            SetVideoModeCommand = new Command(() => SelectedMode = CaptureMode.Video);

            // Initialize sensor items with default visible sensors
            InitializeSensorItems();

            // Initialize sensor value view model
            _sensorValueViewModel = new SensorValueViewModel();

            // Load sensor items configuration from settings
            _ = LoadSensorItemsConfigurationAsync();
        }

        #endregion

        #region Methods

        private void InitializeSensorItems()
        {
            _sensorItems = new ObservableCollection<SensorItem>
            {
                new SensorItem("City", _city, true),           // Visible by default
                new SensorItem("Country", _country, true),     // Visible by default
                new SensorItem("Temperature", _temperature, true), // Visible by default
                new SensorItem("Altitude", _altitude, false),  // Hidden by default
                new SensorItem("Latitude", "", false),         // Will be updated with GPS data
                new SensorItem("Longitude", "", false),        // Will be updated with GPS data
                new SensorItem("Date", DateTime.Now.ToString("MM/dd/yyyy"), false),
                new SensorItem("Time", DateTime.Now.ToString("HH:mm:ss"), false),
                new SensorItem("Heading", "", false),          // Compass heading
                new SensorItem("Speed", "", false)             // Movement speed
            };
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task RequestPermissionsAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                {
                    locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (locationStatus == PermissionStatus.Granted)
                {
                    await GetLocationAsync();
                }
                else
                {
                    Location = "Location permission denied.";
                }

                // Add similar permission handling for other sensors if needed.
            });
        }

        private async Task GetLocationAsync()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    Location = $"Lat: {location.Latitude}, Long: {location.Longitude}";

                    // Update sensor items with location data
                    UpdateSensorItemValue("Latitude", location.Latitude.ToString());
                    UpdateSensorItemValue("Longitude", location.Longitude.ToString());
                    UpdateSensorItemValue("Altitude", location.Altitude?.ToString() ?? "N/A");

                    // Perform reverse geocoding to get city and country
                    if (location.Latitude != 0 || location.Longitude != 0)
                    {
                        var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                        var placemark = placemarks?.FirstOrDefault();

                        if (placemark != null)
                        {
                            var city = placemark.Locality ?? placemark.SubAdminArea ?? placemark.AdminArea;
                            if (!string.IsNullOrWhiteSpace(city))
                            {
                                City = city;
                                UpdateSensorItemValue("City", city);
                            }

                            if (!string.IsNullOrWhiteSpace(placemark.CountryName))
                            {
                                Country = placemark.CountryName;
                                UpdateSensorItemValue("Country", placemark.CountryName);
                            }
                        }
                    }
                }
                else
                {
                    Location = "Unable to retrieve location.";
                }
            }
            catch (Exception ex)
            {
                Location = $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Updates the value of a sensor item by name
        /// </summary>
        /// <param name="name">The name of the sensor item to update</param>
        /// <param name="value">The new value for the sensor item</param>
        private void UpdateSensorItemValue(string name, string value)
        {
            var sensorItem = _sensorItems?.FirstOrDefault(si => si.Name == name);
            if (sensorItem != null)
            {
                sensorItem.Value = value;
            }
        }

        /// <summary>
        /// Updates the value of a sensor item by name and optionally sets visibility
        /// </summary>
        /// <param name="name">The name of the sensor item to update</param>
        /// <param name="value">The new value for the sensor item</param>
        /// <param name="isVisible">Whether the sensor item should be visible</param>
        public void UpdateSensorItem(string name, string value, bool? isVisible = null)
        {
            var sensorItem = _sensorItems?.FirstOrDefault(si => si.Name == name);
            if (sensorItem != null)
            {
                sensorItem.Value = value;
                if (isVisible.HasValue)
                {
                    sensorItem.IsVisible = isVisible.Value;
                }
            }
        }

        /// <summary>
        /// Updates all sensor items based on the provided sensor data
        /// </summary>
        /// <param name="sensorData">The sensor data to update from</param>
        public void UpdateSensorItemsFromData(SensorData sensorData)
        {
            if (sensorData == null) return;

            UpdateSensorItem("City", sensorData.City);
            UpdateSensorItem("Country", sensorData.Country);
            UpdateSensorItem("Temperature", sensorData.Temperature?.ToString("F1") + "°C"); // 1 decimal place accuracy
            UpdateSensorItem("Altitude", sensorData.Altitude?.ToString("F0") + "m");
            UpdateSensorItem("Latitude", sensorData.Latitude.ToString());
            UpdateSensorItem("Longitude", sensorData.Longitude.ToString());
            UpdateSensorItem("Date", sensorData.Timestamp.ToString("MM/dd/yyyy"));
            UpdateSensorItem("Time", sensorData.Timestamp.ToString("HH:mm:ss"));

            // Also update the individual properties to ensure UI synchronization
            City = sensorData.City ?? "Unknown";
            Country = sensorData.Country ?? "Unknown";
            Temperature = sensorData.Temperature.HasValue ? $"{sensorData.Temperature:F1}°C" : "N/A";
            Altitude = sensorData.Altitude.HasValue ? $"{sensorData.Altitude:F0}m" : "N/A";

            // Update the SensorValueViewModel as well
            _sensorValueViewModel?.UpdateFromSensorData(sensorData);
        }

        /// <summary>
        /// Loads the sensor items configuration from settings
        /// </summary>
        private async Task LoadSensorItemsConfigurationAsync()
        {
            try
            {
                var config = await Helpers.SettingsHelper.LoadSensorItemsConfigurationAsync();
                if (config != null)
                {
                    Helpers.SettingsHelper.ApplyConfigurationToSensorItems(_sensorItems, config);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sensor items configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current sensor items configuration to settings
        /// </summary>
        public async Task SaveSensorItemsConfigurationAsync()
        {
            try
            {
                await Helpers.SettingsHelper.SaveSensorItemsConfigurationAsync(_sensorItems);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving sensor items configuration: {ex.Message}");
            }
        }

        #endregion
    }

    public enum CaptureMode
    {
        Photo,
        Video
    }
}
