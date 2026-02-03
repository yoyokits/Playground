using System.Text.Json.Serialization;

namespace TravelCamApp.Models
{
    /// <summary>
    /// Represents sensor data collected from device sensors and external sources.
    /// </summary>
    public class SensorData
    {
        /// <summary>
        /// Gets or sets the latitude coordinate.
        /// </summary>
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; } = -6.2088; // Jakarta default

        /// <summary>
        /// Gets or sets the longitude coordinate.
        /// </summary>
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; } = 106.8456; // Jakarta default

        /// <summary>
        /// Gets or sets the altitude in meters.
        /// </summary>
        [JsonPropertyName("altitude")]
        public double? Altitude { get; set; } = 12.0; // Jakarta average altitude

        /// <summary>
        /// Gets or sets the temperature in Celsius.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; } = 28.0; // Jakarta average temperature

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = "Jakarta";

        /// <summary>
        /// Gets or sets the country name.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; } = "Indonesia";

        /// <summary>
        /// Gets or sets the timestamp of when the data was collected.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the heading direction in degrees.
        /// </summary>
        [JsonPropertyName("heading")]
        public double? Heading { get; set; }

        /// <summary>
        /// Gets or sets the speed in meters per second.
        /// </summary>
        [JsonPropertyName("speed")]
        public double? Speed { get; set; }
    }
}