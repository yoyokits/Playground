// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// SettingsHelper — all persistent app settings stored in Android SharedPreferences.
//
// Overlay items (visibility + order) are serialized to a single JSON string
// and stored under the key "OverlayItemsJson". This avoids a settings file
// while still preserving item order across sessions.
//
// Camera settings (rule of thirds, aspect ratio, resolution, etc.) are stored
// as individual keys directly in CameraSettingsViewModel via Preferences.Set().

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TravelCamApp.Models;

namespace TravelCamApp.Helpers
{
    public static class SettingsHelper
    {
        // ── Preference keys ────────────────────────────────────────────────────
        private const string PrefOverlayFontSize  = "OverlayFontSize";
        private const string PrefOverlayItemsJson = "OverlayItemsJson";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // ── Overlay items API ─────────────────────────────────────────────────

        /// <summary>
        /// Saves overlay items (in current order) and font size to SharedPreferences.
        /// Visible items must be first in <paramref name="overlayItems"/> so order is
        /// restored correctly on the next load.
        /// Safe to call at any time — not limited to app exit.
        /// </summary>
        public static Task<bool> SaveOverlayItemsConfigurationAsync(
            IEnumerable<OverlayItem> overlayItems, float fontSize = 12f)
        {
            try
            {
                Preferences.Set(PrefOverlayFontSize, fontSize);

                var records = overlayItems
                    .Select(i => new OverlayItemRecord { Name = i.Name, IsVisible = i.IsVisible })
                    .ToList();

                var json = JsonSerializer.Serialize(records, JsonOptions);
                Preferences.Set(PrefOverlayItemsJson, json);

                System.Diagnostics.Debug.WriteLine(
                    $"[SettingsHelper] Overlay items saved to SharedPreferences ({records.Count} items)");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SettingsHelper] SaveOverlayItemsConfigurationAsync error: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Loads overlay configuration from SharedPreferences.
        /// Returns null if no settings have been saved yet.
        /// </summary>
        public static Task<OverlayItemsConfiguration?> LoadOverlayItemsConfigurationAsync()
        {
            try
            {
                var json = Preferences.Get(PrefOverlayItemsJson, string.Empty);
                if (string.IsNullOrEmpty(json))
                    return Task.FromResult<OverlayItemsConfiguration?>(null);

                var records = JsonSerializer.Deserialize<List<OverlayItemRecord>>(json, JsonOptions);
                if (records == null)
                    return Task.FromResult<OverlayItemsConfiguration?>(null);

                return Task.FromResult<OverlayItemsConfiguration?>(new OverlayItemsConfiguration
                {
                    FontSize = Preferences.Get(PrefOverlayFontSize, 12f),
                    Items = records
                        .Select(r => new OverlayItemConfig { Name = r.Name, IsVisible = r.IsVisible })
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SettingsHelper] LoadOverlayItemsConfigurationAsync error: {ex.Message}");
                return Task.FromResult<OverlayItemsConfiguration?>(null);
            }
        }

        /// <summary>
        /// Applies the loaded configuration to <paramref name="source"/>.
        /// Sets IsVisible on each item AND reorders the collection to match the saved order,
        /// so the camera overlay reflects the user's last arrangement immediately on startup.
        /// </summary>
        public static void ApplyConfigurationToOverlayItems(
            ObservableCollection<OverlayItem> source, OverlayItemsConfiguration? config)
        {
            if (config?.Items == null || config.Items.Count == 0) return;

            // ── Step 1: update IsVisible flags ───────────────────────────────
            foreach (var item in source)
            {
                var saved = config.Items.FirstOrDefault(c => c.Name == item.Name);
                if (saved != null)
                    item.IsVisible = saved.IsVisible;
            }

            // ── Step 2: reorder source to match saved order ──────────────────
            // config.Items is in saved order (visible first, then hidden).
            // Use Move() so CollectionView receives targeted notifications instead of a reset.
            for (int i = 0; i < config.Items.Count; i++)
            {
                var targetName = config.Items[i].Name;
                for (int j = i; j < source.Count; j++)
                {
                    if (source[j].Name == targetName)
                    {
                        if (j != i) source.Move(j, i);
                        break;
                    }
                }
            }
        }

        // ── Cached sensor data API ────────────────────────────────────────────
        private const string PrefCachedSensorData = "CachedSensorData";

        /// <summary>
        /// Saves the last known sensor values to SharedPreferences so they can be
        /// displayed immediately on the next app launch, before the first sensor tick.
        /// Thread-safe: Android SharedPreferences is thread-safe.
        /// Called on every sensor update and on app stop/destroy.
        /// </summary>
        public static void SaveCachedSensorData(Models.SensorData data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, JsonOptions);
                Preferences.Set(PrefCachedSensorData, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SettingsHelper] SaveCachedSensorData error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the last known sensor values from SharedPreferences.
        /// Returns null on first launch (no cached data yet).
        /// </summary>
        public static Models.SensorData? LoadCachedSensorData()
        {
            try
            {
                var json = Preferences.Get(PrefCachedSensorData, string.Empty);
                if (string.IsNullOrEmpty(json))
                    return null;
                return JsonSerializer.Deserialize<Models.SensorData>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SettingsHelper] LoadCachedSensorData error: {ex.Message}");
                return null;
            }
        }

        // ── Camera layout cache API ───────────────────────────────────────────
        private const string PrefCameraLayout = "CameraLayout";

        /// <summary>
        /// Saves the computed camera layout to SharedPreferences so it can be
        /// applied immediately on the next app launch, before the first render.
        /// Called synchronously from ApplyCameraLayout — SharedPreferences is non-blocking.
        /// </summary>
        public static void SaveCameraLayout(CameraLayoutCache cache)
        {
            try
            {
                var json = JsonSerializer.Serialize(cache, JsonOptions);
                Preferences.Set(PrefCameraLayout, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SettingsHelper] SaveCameraLayout error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the last computed camera layout from SharedPreferences.
        /// Returns null on first launch (no cache yet).
        /// </summary>
        public static CameraLayoutCache? LoadCameraLayout()
        {
            try
            {
                var json = Preferences.Get(PrefCameraLayout, string.Empty);
                if (string.IsNullOrEmpty(json)) return null;
                return JsonSerializer.Deserialize<CameraLayoutCache>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SettingsHelper] LoadCameraLayout error: {ex.Message}");
                return null;
            }
        }

        // ── Private serialization record ──────────────────────────────────────
        // Used only for JSON within the OverlayItemsJson preference string.
        private sealed class OverlayItemRecord
        {
            public string Name { get; set; } = "";
            public bool IsVisible { get; set; }
        }
    }

    // ── Result model types ────────────────────────────────────────────────────

    /// <summary>Overlay configuration returned by LoadOverlayItemsConfigurationAsync.</summary>
    public class OverlayItemsConfiguration
    {
        public float FontSize { get; set; } = 12f;
        public List<OverlayItemConfig>? Items { get; set; }
    }

    /// <summary>Single item record in OverlayItemsConfiguration.</summary>
    public class OverlayItemConfig
    {
        public string? Name { get; set; }
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// Cached output of ApplyCameraLayout — all values needed to restore the
    /// container and crop bars to their last correct position without recomputing.
    /// </summary>
    public class CameraLayoutCache
    {
        // Input dimensions — used to detect orientation changes between sessions.
        public double CameraViewWidth  { get; set; }
        public double CameraViewHeight { get; set; }

        // CameraViewChildrenContainer
        public double ContainerLeft   { get; set; }
        public double ContainerTop    { get; set; }
        public double ContainerWidth  { get; set; }
        public double ContainerHeight { get; set; }

        // Letterbox bars (top / bottom)
        public double TopBarTop     { get; set; }
        public double TopBarHeight  { get; set; }
        public bool   TopBarVisible { get; set; }
        public double BottomBarTop     { get; set; }
        public double BottomBarHeight  { get; set; }
        public bool   BottomBarVisible { get; set; }

        // Pillarbox bars (left / right)
        public double LeftBarLeft    { get; set; }
        public double LeftBarTop     { get; set; }
        public double LeftBarWidth   { get; set; }
        public double LeftBarHeight  { get; set; }
        public bool   LeftBarVisible { get; set; }
        public double RightBarRight  { get; set; }
        public double RightBarTop    { get; set; }
        public double RightBarWidth  { get; set; }
        public double RightBarHeight { get; set; }
        public bool   RightBarVisible { get; set; }
    }
}
