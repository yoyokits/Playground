using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TravelCamApp.Models;

namespace TravelCamApp.Helpers
{
    /// <summary>
    /// Helper class to manage application settings including sensor item configurations
    /// </summary>
    public class SettingsHelper
    {
        private const string SETTINGS_FILE_NAME = "sensor_settings.json";
        
        /// <summary>
        /// Saves the sensor items configuration to a JSON file
        /// </summary>
        /// <param name="sensorItems">The collection of sensor items to save</param>
        /// <returns>True if the save was successful, false otherwise</returns>
        public static async Task<bool> SaveSensorItemsConfigurationAsync(IEnumerable<SensorItem> sensorItems)
        {
            try
            {
                var settingsPath = Path.Combine(FileSystem.AppDataDirectory, SETTINGS_FILE_NAME);
                
                var config = new SensorItemsConfiguration
                {
                    Items = new List<SensorItemConfig>()
                };

                foreach (var item in sensorItems)
                {
                    config.Items.Add(new SensorItemConfig
                    {
                        Name = item.Name,
                        IsVisible = item.IsVisible
                    });
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(config, options);
                
                await File.WriteAllTextAsync(settingsPath, json);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving sensor items configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the sensor items configuration from a JSON file
        /// </summary>
        /// <returns>The loaded configuration, or null if loading failed</returns>
        public static async Task<SensorItemsConfiguration?> LoadSensorItemsConfigurationAsync()
        {
            try
            {
                var settingsPath = Path.Combine(FileSystem.AppDataDirectory, SETTINGS_FILE_NAME);
                
                if (!File.Exists(settingsPath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(settingsPath);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var config = JsonSerializer.Deserialize<SensorItemsConfiguration>(json, options);
                
                return config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sensor items configuration: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Applies the loaded configuration to the sensor items
        /// </summary>
        /// <param name="sensorItems">The collection of sensor items to update</param>
        /// <param name="config">The configuration to apply</param>
        public static void ApplyConfigurationToSensorItems(IEnumerable<SensorItem> sensorItems, SensorItemsConfiguration? config)
        {
            if (config?.Items == null) return;

            foreach (var item in sensorItems)
            {
                var configItem = config.Items.Find(ci => ci.Name == item.Name);
                if (configItem != null)
                {
                    item.IsVisible = configItem.IsVisible;
                }
            }
        }
    }

    /// <summary>
    /// Represents the configuration for sensor items
    /// </summary>
    public class SensorItemsConfiguration
    {
        public List<SensorItemConfig>? Items { get; set; }
    }

    /// <summary>
    /// Represents the configuration for a single sensor item
    /// </summary>
    public class SensorItemConfig
    {
        public string? Name { get; set; }
        public bool IsVisible { get; set; }
    }
}