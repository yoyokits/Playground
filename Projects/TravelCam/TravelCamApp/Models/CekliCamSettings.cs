// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// CekliCamSettings is the single persistent settings model for the app.
// Stored at: AppDataDirectory/CekliCamSettings.json
// Written whenever the user closes the Overlay Settings panel,
// or whenever SaveAsync is called explicitly.

using System.Collections.Generic;

namespace TravelCamApp.Models
{
    /// <summary>
    /// Root model for CekliCamSettings.json — all app settings in one place.
    /// </summary>
    public class CekliCamSettings
    {
        /// <summary>
        /// Sensor overlay label font size (pt). Controlled by the Label Size slider.
        /// </summary>
        public float OverlayFontSize { get; set; } = 12f;

        /// <summary>
        /// All sensor overlay items in display order.
        /// Visible items appear first; hidden items follow.
        /// </summary>
        public List<CekliCamOverlayItem> OverlayItems { get; set; } = new();
    }

    /// <summary>
    /// Persisted record for a single sensor overlay item.
    /// </summary>
    public class CekliCamOverlayItem
    {
        public string Name { get; set; } = "";
        public bool IsVisible { get; set; }
    }
}
