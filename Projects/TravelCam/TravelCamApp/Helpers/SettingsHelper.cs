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
        /// Saves the overlay items configuration to a JSON file
        /// </summary>
        /// <param name="overlayItems">The collection of overlay items to save</param>
        /// <returns>True if the save was successful, false otherwise</returns>
        public static async Task<bool> SaveOverlayItemsConfigurationAsync(IEnumerable<OverlayItem> overlayItems)
        {
            try
            {
                var settingsPath = Path.Combine(FileSystem.AppDataDirectory, SETTINGS_FILE_NAME);

                var config = new OverlayItemsConfiguration
                {
                    Items = new List<OverlayItemConfig>()
                };

                foreach (var item in overlayItems)
                {
                    config.Items.Add(new OverlayItemConfig
                    {
                        Name = item.Name,
                        IsVisible = item.IsVisible,
                        UpdateInterval = item.UpdateInterval
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
        /// Loads the overlay items configuration from a JSON file
        /// </summary>
        /// <returns>The loaded configuration, or null if loading failed</returns>
        public static async Task<OverlayItemsConfiguration?> LoadOverlayItemsConfigurationAsync()
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

                var config = JsonSerializer.Deserialize<OverlayItemsConfiguration>(json, options);
                
                return config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sensor items configuration: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Applies the loaded configuration to the overlay items
        /// </summary>
        /// <param name="allOverlayItems">The collection of all overlay items to update</param>
        /// <param name="config">The configuration to apply</param>
        public static void ApplyConfigurationToOverlayItems(List<OverlayItem> allOverlayItems, OverlayItemsConfiguration? config)
        {
            if (config?.Items == null) return;

            // First, set visibility and update interval based on the configuration
            foreach (var item in allOverlayItems)
            {
                var configItem = config.Items.Find(ci => ci.Name == item.Name);
                if (configItem != null)
                {
                    item.IsVisible = configItem.IsVisible;
                    item.UpdateInterval = configItem.UpdateInterval;
                }
            }
        }

        /// <summary>
        /// Creates an OverlayItemsConfiguration from overlay items
        /// </summary>
        /// <param name="overlayItems">The overlay items to convert</param>
        /// <returns>The created configuration</returns>
        public static OverlayItemsConfiguration CreateOverlayItemsConfiguration(List<OverlayItem> overlayItems)
        {
            var config = new OverlayItemsConfiguration
            {
                Items = new List<OverlayItemConfig>()
            };

            foreach (var item in overlayItems)
            {
                config.Items.Add(new OverlayItemConfig
                {
                    Name = item.Name,
                    IsVisible = item.IsVisible,
                    UpdateInterval = item.UpdateInterval
                });
            }

            return config;
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
    /// Represents the configuration for overlay items
    /// </summary>
    public class OverlayItemsConfiguration
    {
        public List<OverlayItemConfig>? Items { get; set; }
    }

    /// <summary>
    /// Represents the configuration for a single overlay item
    /// </summary>
    public class OverlayItemConfig
    {
        public string? Name { get; set; }
        public bool IsVisible { get; set; }
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(10); // Default to 10 seconds
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