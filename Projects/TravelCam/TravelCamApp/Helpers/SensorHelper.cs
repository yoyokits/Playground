using System.Text.Json;
using System.Text.Json.Serialization;
using TravelCamApp.Models;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using System.Globalization;
using System.Net.Http.Json;

namespace TravelCamApp.Helpers
{
    /// <summary>
    /// Helper class to manage sensor data collection, storage, and retrieval.
    /// </summary>
    public class SensorHelper
    {
        private const string SENSOR_DATA_FILE = "SensorData.json";
        private static readonly HttpClient HttpClient = new();
        private readonly string _sensorDataPath;
        private SensorData _currentData;
        private Timer? _updateTimer;
        private bool _isUpdating = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorHelper"/> class.
        /// </summary>
        public SensorHelper()
        {
            _sensorDataPath = Path.Combine(FileSystem.AppDataDirectory, SENSOR_DATA_FILE);
            _currentData = LoadSensorData() ?? CreateDefaultSensorData();
        }

        /// <summary>
        /// Occurs when sensor data is updated.
        /// </summary>
        public event EventHandler<SensorData>? SensorDataUpdated;

        /// <summary>
        /// Occurs when sensor data is updated, providing a callback for UI updates.
        /// </summary>
        public event Action<SensorData>? SensorDataUpdatedCallback;

        /// <summary>
        /// Gets the current sensor data.
        /// </summary>
        public SensorData CurrentData => _currentData;

        /// <summary>
        /// Starts the sensor data collection and periodic updates.
        /// </summary>
        public async Task StartAsync()
        {
            // Ensure location permissions are granted
            await RequestLocationPermissionAsync();

            // Start collecting data immediately
            await UpdateSensorDataAsync();

            // Set up periodic updates every 5 seconds
            _updateTimer = new Timer(async _ => await UpdateSensorDataAsync(), null,
                TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Requests location permission from the user.
        /// </summary>
        private async Task RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status != PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("Location permission not granted");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error requesting location permission: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the periodic updates.
        /// </summary>
        public void Stop()
        {
            _updateTimer?.Dispose();
        }

        /// <summary>
        /// Updates the sensor data with current values from sensors and external sources.
        /// </summary>
        public async Task UpdateSensorDataAsync()
        {
            if (_isUpdating) return; // Prevent overlapping updates
            _isUpdating = true;

            try
            {
                var newData = new SensorData
                {
                    Timestamp = DateTime.UtcNow,
                    Latitude = _currentData.Latitude,
                    Longitude = _currentData.Longitude,
                    Altitude = _currentData.Altitude,
                    Temperature = _currentData.Temperature,
                    City = _currentData.City,
                    Country = _currentData.Country,
                    Heading = _currentData.Heading,
                    Speed = _currentData.Speed
                };

                // Collect fast data in parallel
                var fastDataTask = Task.Run(async () =>
                {
                    // Get location data
                    var location = await GetLocationAsync();
                    if (location != null)
                    {
                        if (location.Latitude != 0 || location.Longitude != 0)
                        {
                            newData.Latitude = location.Latitude;
                            newData.Longitude = location.Longitude;
                        }

                        // Only update altitude if it's a valid value (not NaN or unrealistic)
                        if (location.Altitude.HasValue && !double.IsNaN(location.Altitude.Value) && location.Altitude.Value > -10000 && location.Altitude.Value < 9000)
                        {
                            newData.Altitude = location.Altitude;
                        }
                        else
                        {
                            // Keep the previous altitude value if current one is invalid
                            newData.Altitude = _currentData.Altitude;
                        }

                        // Convert coordinates to city name
                        if (location.Latitude != 0 || location.Longitude != 0)
                        {
                            var placemark = await GetPlacemarkAsync(location.Latitude, location.Longitude);
                            if (placemark != null)
                            {
                                var city = placemark.Locality ?? placemark.SubAdminArea ?? placemark.AdminArea;
                                if (!string.IsNullOrWhiteSpace(city))
                                {
                                    newData.City = city;
                                }

                                if (!string.IsNullOrWhiteSpace(placemark.CountryName))
                                {
                                    newData.Country = placemark.CountryName;
                                }
                            }
                        }
                    }

                    // Get compass heading
                    if (Compass.Default.IsSupported)
                    {
                        var heading = await GetCompassReadingAsync();
                        newData.Heading = heading;
                    }

                    if (location?.Speed.HasValue == true)
                    {
                        newData.Speed = location.Speed.Value;
                    }
                });

                // Collect slow data in parallel
                var slowDataTask = Task.Run(async () =>
                {
                    // Get temperature from external API (simulated)
                    if (newData.Latitude != 0 || newData.Longitude != 0)
                    {
                        newData.Temperature = await GetTemperatureAsync(newData.Latitude, newData.Longitude);
                    }
                });

                // Wait for both tasks to complete
                await Task.WhenAll(fastDataTask, slowDataTask);

                // Update current data
                _currentData = newData;

                // Trigger the event to notify listeners
                SensorDataUpdated?.Invoke(this, _currentData);

                // Call the callback for UI updates
                SensorDataUpdatedCallback?.Invoke(_currentData);

                // Save to file
                SaveSensorData(_currentData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating sensor data: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Loads sensor data from the JSON file.
        /// </summary>
        /// <returns>The loaded sensor data, or null if the file doesn't exist.</returns>
        private SensorData? LoadSensorData()
        {
            try
            {
                if (File.Exists(_sensorDataPath))
                {
                    var json = File.ReadAllText(_sensorDataPath);
                    return JsonSerializer.Deserialize<SensorData>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sensor data: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Saves sensor data to the JSON file.
        /// </summary>
        /// <param name="data">The sensor data to save.</param>
        private void SaveSensorData(SensorData data)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(_sensorDataPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving sensor data: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates default sensor data for Jakarta, Indonesia.
        /// </summary>
        /// <returns>Default sensor data.</returns>
        private SensorData CreateDefaultSensorData()
        {
            return new SensorData
            {
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Gets the current location.
        /// </summary>
        /// <returns>The location, or null if not available.</returns>
        private async Task<Location?> GetLocationAsync()
        {
            try
            {
                // First try to get the last known location quickly
                var lastKnownLocation = await Geolocation.Default.GetLastKnownLocationAsync();

                // Then try to get a fresh location with high accuracy
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15));
                var freshLocation = await Geolocation.Default.GetLocationAsync(request);

                // Return the freshest location available
                return freshLocation ?? lastKnownLocation;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");

                // Try to get last known location as fallback
                try
                {
                    return await Geolocation.Default.GetLastKnownLocationAsync();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the placemark (address) for the given coordinates.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>The placemark, or null if not available.</returns>
        private async Task<Placemark?> GetPlacemarkAsync(double latitude, double longitude)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
                return placemarks?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Geocoding error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the current compass heading.
        /// </summary>
        /// <returns>The heading in degrees, or null if not available.</returns>
        private async Task<double?> GetCompassReadingAsync()
        {
            try
            {
                if (!Compass.Default.IsSupported)
                {
                    return null;
                }

                var tcs = new TaskCompletionSource<CompassChangedEventArgs>();
                EventHandler<CompassChangedEventArgs> handler = (s, e) => tcs.TrySetResult(e);

                Compass.Default.ReadingChanged += handler;
                Compass.Default.Start(SensorSpeed.Fastest);

                var result = await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(2));

                Compass.Default.Stop();
                Compass.Default.ReadingChanged -= handler;

                return result.Reading.HeadingMagneticNorth;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Compass error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the temperature for the given location (simulated).
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>The temperature in Celsius.</returns>
        private async Task<double?> GetTemperatureAsync(double latitude, double longitude)
        {
            try
            {
                var url = string.Format(CultureInfo.InvariantCulture,
                    "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current=temperature_2m",
                    latitude,
                    longitude);

                using var response = await HttpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return _currentData.Temperature;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var json = await JsonDocument.ParseAsync(stream);
                if (json.RootElement.TryGetProperty("current", out var current) &&
                    current.TryGetProperty("temperature_2m", out var temperatureElement) &&
                    temperatureElement.TryGetDouble(out var temperature))
                {
                    return temperature;
                }

                return _currentData.Temperature;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Temperature API error: {ex.Message}");
                return _currentData.Temperature;
            }
        }
    }

    /// <summary>
    /// Extension method to add timeout functionality to tasks.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Adds a timeout to a task.
        /// </summary>
        /// <typeparam name="T">The return type of the task.</typeparam>
        /// <param name="task">The task to add timeout to.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <returns>The task result, or throws TimeoutException if timeout is reached.</returns>
        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var delayTask = Task.Delay(timeout, cts.Token);
            var resultTask = await Task.WhenAny(task, delayTask);
            
            if (resultTask == delayTask)
            {
                throw new TimeoutException($"Task timed out after {timeout.TotalSeconds} seconds");
            }
            
            cts.Cancel();
            return await task;
        }
    }
}