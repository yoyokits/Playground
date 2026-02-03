// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

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
        /// <param name="allSensorItems">The collection of all sensor items to update</param>
        /// <param name="config">The configuration to apply</param>
        public static void ApplyConfigurationToSensorItems(List<SensorItem> allSensorItems, SensorItemsConfiguration? config)
        {
            if (config?.Items == null) return;

            // First, set visibility based on the configuration
            foreach (var item in allSensorItems)
            {
                var configItem = config.Items.Find(ci => ci.Name == item.Name);
                if (configItem != null)
                {
                    item.IsVisible = configItem.IsVisible;
                }
            }
        }

        /// <summary>
        /// Applies the loaded configuration to separate available and visible collections
        /// </summary>
        /// <param name="availableSensorItems">Collection of available sensor items</param>
        /// <param name="visibleSensorItems">Collection of visible sensor items</param>
        /// <param name="config">The configuration to apply</param>
        public static void ApplyConfigurationToSensorItemsCollections(
            System.Collections.ObjectModel.ObservableCollection<SensorItem> availableSensorItems,
            System.Collections.ObjectModel.ObservableCollection<SensorItem> visibleSensorItems,
            SensorItemsConfiguration? config)
        {
            if (config?.Items == null) return;

            System.Diagnostics.Debug.WriteLine($"[SettingsHelper] ApplyConfigurationToSensorItemsCollections - Config has {config.Items.Count} items");

            // Clear existing items to prevent duplicates
            availableSensorItems.Clear();
            visibleSensorItems.Clear();

            // Track which items have been added to prevent duplicates
            var addedItems = new HashSet<string>();

            // Create a dictionary of all possible sensor items by name
            var allPossibleItems = new Dictionary<string, SensorItem>
            {
                ["City"] = new SensorItem("City", "Jakarta", false),
                ["Country"] = new SensorItem("Country", "Indonesia", false),
                ["Temperature"] = new SensorItem("Temperature", "28°C", false),
                ["Altitude"] = new SensorItem("Altitude", "12m", false),
                ["Latitude"] = new SensorItem("Latitude", "-6.2088", false),
                ["Longitude"] = new SensorItem("Longitude", "106.8456", false),
                ["Date"] = new SensorItem("Date", DateTime.Now.ToString("MM/dd/yyyy"), false),
                ["Time"] = new SensorItem("Time", DateTime.Now.ToString("HH:mm:ss"), false),
                ["Heading"] = new SensorItem("Heading", "0°", false),
                ["Speed"] = new SensorItem("Speed", "0 m/s", false),
                ["Map"] = new SensorItem("Map", "Map View", false) // Add map overlay option
            };

            // Process each item in the configuration (preserve order)
            foreach (var configItem in config.Items)
            {
                // Skip if already added (prevent duplicates)
                if (addedItems.Contains(configItem.Name))
                {
                    System.Diagnostics.Debug.WriteLine($"[SettingsHelper] Skipping duplicate item: {configItem.Name}");
                    continue;
                }

                if (allPossibleItems.TryGetValue(configItem.Name, out var item))
                {
                    item.IsVisible = configItem.IsVisible;
                    addedItems.Add(configItem.Name);

                    if (configItem.IsVisible)
                    {
                        visibleSensorItems.Add(item);
                        System.Diagnostics.Debug.WriteLine($"[SettingsHelper] Added {configItem.Name} to visible");
                    }
                    else
                    {
                        availableSensorItems.Add(item);
                        System.Diagnostics.Debug.WriteLine($"[SettingsHelper] Added {configItem.Name} to available");
                    }
                }
            }

            // Add any remaining items that weren't in the config
            foreach (var kvp in allPossibleItems)
            {
                var itemName = kvp.Key;
                var item = kvp.Value;

                // Check if this item is already added
                if (!addedItems.Contains(itemName))
                {
                    // Add to available by default
                    item.IsVisible = false;
                    availableSensorItems.Add(item);
                    addedItems.Add(itemName);
                    System.Diagnostics.Debug.WriteLine($"[SettingsHelper] Added {itemName} to available (not in config)");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[SettingsHelper] Final counts - Available: {availableSensorItems.Count}, Visible: {visibleSensorItems.Count}");
        }

        /// <summary>
        /// Saves additional settings to a JSON file
        /// </summary>
        /// <param name="fontSize">The font size to save</param>
        /// <param name="isMapOverlayVisible">Whether map overlay is visible</param>
        /// <returns>True if the save was successful, false otherwise</returns>
        public static async Task<bool> SaveAdditionalSettingsAsync(double fontSize, bool isMapOverlayVisible)
        {
            try
            {
                var settingsPath = Path.Combine(FileSystem.AppDataDirectory, "additional_settings.json");

                var settings = new AdditionalSettings
                {
                    FontSize = fontSize,
                    IsMapOverlayVisible = isMapOverlayVisible
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(settings, options);

                await File.WriteAllTextAsync(settingsPath, json);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving additional settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads additional settings from a JSON file
        /// </summary>
        /// <returns>The loaded additional settings, or null if loading failed</returns>
        public static async Task<AdditionalSettings?> LoadAdditionalSettingsAsync()
        {
            try
            {
                var settingsPath = Path.Combine(FileSystem.AppDataDirectory, "additional_settings.json");

                if (!File.Exists(settingsPath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(settingsPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var settings = JsonSerializer.Deserialize<AdditionalSettings>(json, options);

                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading additional settings: {ex.Message}");
                return null;
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

    /// <summary>
    /// Represents additional settings for sensor display
    /// </summary>
    public class AdditionalSettings
    {
        public double FontSize { get; set; } = 14.0;
        public bool IsMapOverlayVisible { get; set; } = false;
    }
}