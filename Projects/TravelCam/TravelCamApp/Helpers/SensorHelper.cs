// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// SensorHelper: Single source of sensor truth for the app.
// Collects location, compass, weather data on a 5-second timer.
// Publishes SensorData via event to any subscriber.

using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using TravelCamApp.Models;

namespace TravelCamApp.Helpers
{
    public class SensorHelper : IDisposable
    {
        private readonly HttpClient _httpClient = new();
        private bool _disposed;
        private System.Timers.Timer? _updateTimer;
        private CancellationTokenSource? _cts;
        private bool _isUpdating;

        public SensorData CurrentData { get; private set; } = new()
        {
            Timestamp = DateTime.UtcNow,
            Temperature = null,
            City = "",
            Country = "",
            Latitude = 0,
            Longitude = 0,
            Altitude = null,
            Heading = null,
            Speed = null,
        };

        /// <summary>
        /// Raised when new sensor data is available. Subscribe to receive updates.
        /// </summary>
        public event Action<SensorData>? SensorDataUpdated;

        public async Task StartAsync()
        {
            // Stop any existing timer before starting a new one
            Stop();

            _cts = new CancellationTokenSource();

            // Fire-and-forget initial sensor poll on a background thread so the
            // caller (often the main thread) is not blocked by geolocation/HTTP.
            var token = _cts.Token;
            _ = Task.Run(async () =>
            {
                try { await UpdateSensorDataAsync(token); }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[SensorHelper] Initial update error: {ex.Message}");
                }
            });

            if (_cts.IsCancellationRequested) return;

            _updateTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
            _updateTimer.Elapsed += OnTimerElapsed;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }

        public void Stop()
        {
            // Cancel all in-flight async work first
            try { _cts?.Cancel(); } catch { /* already disposed */ }
            _cts?.Dispose();
            _cts = null;

            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            _updateTimer = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }

        private async void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var token = _cts?.Token ?? default;
            if (token.IsCancellationRequested) return;

            try
            {
                await UpdateSensorDataAsync(token);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SensorHelper] Timer callback error: {ex.Message}");
            }
        }

        private async Task UpdateSensorDataAsync(CancellationToken ct)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                ct.ThrowIfCancellationRequested();

                var data = new SensorData { Timestamp = DateTime.UtcNow };

                // Location (primary data source)
                var location = await GetLocationAsync(ct);
                ct.ThrowIfCancellationRequested();

                if (location != null)
                {
                    data.Latitude = location.Latitude;
                    data.Longitude = location.Longitude;

                    // Keep previous altitude if current is invalid
                    if (location.Altitude.HasValue
                        && !double.IsNaN(location.Altitude.Value)
                        && location.Altitude.Value > -1000 && location.Altitude.Value < 9000)
                    {
                        data.Altitude = location.Altitude;
                    }
                    else
                    {
                        data.Altitude = CurrentData.Altitude;
                    }

                    data.Speed = location.Speed;

                    // Reverse geocoding
                    var placemark = await GetPlacemarkAsync(location.Latitude, location.Longitude);
                    if (placemark != null)
                    {
                        data.City = placemark.Locality
                            ?? placemark.SubAdminArea
                            ?? placemark.AdminArea
                            ?? CurrentData.City;
                        data.Country = placemark.CountryName ?? CurrentData.Country;
                    }
                }
                else
                {
                    data.Latitude = CurrentData.Latitude;
                    data.Longitude = CurrentData.Longitude;
                    data.Altitude = CurrentData.Altitude;
                    data.Speed = CurrentData.Speed;
                    data.City = CurrentData.City;
                    data.Country = CurrentData.Country;
                }

                ct.ThrowIfCancellationRequested();

                // Temperature (weather API)
                if (data.Latitude != 0 || data.Longitude != 0)
                {
                    data.Temperature = await GetTemperatureAsync(
                        data.Latitude, data.Longitude, ct);
                }
                else
                {
                    data.Temperature = CurrentData.Temperature;
                }

                ct.ThrowIfCancellationRequested();

                // Compass heading
                if (Compass.Default.IsSupported)
                {
                    data.Heading = await GetCompassReadingAsync();
                }
                else
                {
                    data.Heading = CurrentData.Heading;
                }

                ct.ThrowIfCancellationRequested();

                CurrentData = data;
                SensorDataUpdated?.Invoke(data);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown, don't log
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SensorHelper] Update error: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private async Task<Location?> GetLocationAsync(CancellationToken ct)
        {
            try
            {
                var request = new GeolocationRequest(
                    GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                return await Geolocation.Default.GetLocationAsync(request, ct);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch
            {
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

        private async Task<Placemark?> GetPlacemarkAsync(double lat, double lon)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(lat, lon);
                return placemarks?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private async Task<double?> GetCompassReadingAsync()
        {
            try
            {
                if (!Compass.Default.IsSupported) return null;

                var tcs = new TaskCompletionSource<CompassChangedEventArgs>();
                EventHandler<CompassChangedEventArgs> handler = (_, e) =>
                    tcs.TrySetResult(e);

                Compass.Default.ReadingChanged += handler;
                Compass.Default.Start(SensorSpeed.Fastest);

                var result = await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(2));

                Compass.Default.Stop();
                Compass.Default.ReadingChanged -= handler;

                return result.Reading.HeadingMagneticNorth;
            }
            catch
            {
                return null;
            }
        }

        private async Task<double?> GetTemperatureAsync(double lat, double lon, CancellationToken ct)
        {
            try
            {
                var url = string.Format(
                    CultureInfo.InvariantCulture,
                    "https://api.open-meteo.com/v1/forecast?"
                    + "latitude={0}&longitude={1}&current=temperature_2m",
                    lat, lon);

                using var response = await _httpClient.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                    return CurrentData.Temperature;

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, default, ct);
                var root = doc.RootElement;

                if (root.TryGetProperty("current", out var current)
                    && current.TryGetProperty("temperature_2m", out var temp)
                    && temp.TryGetDouble(out var temperature))
                {
                    return temperature;
                }

                return CurrentData.Temperature;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SensorHelper] Temperature API error: {ex.Message}");
                return CurrentData.Temperature;
            }
        }
    }

    /// <summary>
    /// Extension: adds timeout to any task.
    /// </summary>
    public static class TaskExtensions
    {
        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var delay = Task.Delay(timeout, cts.Token);
            var completed = await Task.WhenAny(task, delay);

            if (completed == delay)
                throw new TimeoutException(
                    $"Task timed out after {timeout.TotalSeconds}s");

            cts.Cancel();
            return await task;
        }
    }
}
