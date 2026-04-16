// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokitos       //
// ========================================== //
//
// MediaInfo: EXIF metadata read from a captured image file.
// Populated by ExifHelper.ReadMetadata() and displayed in the
// gallery info panel (ToggleMediaInfoCommand).

namespace TravelCamApp.Models
{
    public class MediaInfo
    {
        // ── File section ──────────────────────────────────────────────────────
        public string FileName { get; set; } = string.Empty;
        public string FileSizeText { get; set; } = string.Empty;
        public string CaptureDateText { get; set; } = string.Empty;
        public bool IsVideo { get; set; }

        // ── Camera section ────────────────────────────────────────────────────
        public string DeviceMake { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = string.Empty;
        public string ResolutionText { get; set; } = string.Empty;
        public string MegaPixelText { get; set; } = string.Empty;
        public string FlashText { get; set; } = string.Empty;
        public string AspectRatioText { get; set; } = string.Empty;

        // ── Location section ──────────────────────────────────────────────────
        public string GpsCoordsText { get; set; } = string.Empty;
        public string AltitudeText { get; set; } = string.Empty;
        public string CityText { get; set; } = string.Empty;
        public string CountryText { get; set; } = string.Empty;

        // ── Conditions section ────────────────────────────────────────────────
        public string TemperatureText { get; set; } = string.Empty;
        public string HeadingText { get; set; } = string.Empty;
        public string SpeedText { get; set; } = string.Empty;

        // ── Computed ──────────────────────────────────────────────────────────
        public string DeviceDisplay =>
            string.IsNullOrEmpty(DeviceMake) && string.IsNullOrEmpty(DeviceModel)
                ? string.Empty
                : $"{DeviceMake} {DeviceModel}".Trim();

        public bool HasMegaPixelText => !string.IsNullOrEmpty(MegaPixelText);

        public bool HasCameraInfo =>
            !string.IsNullOrEmpty(ResolutionText) || !string.IsNullOrEmpty(DeviceDisplay);

        public bool HasLocationInfo =>
            !string.IsNullOrEmpty(GpsCoordsText) || !string.IsNullOrEmpty(CityText);

        public bool HasConditionsInfo =>
            !string.IsNullOrEmpty(TemperatureText) || !string.IsNullOrEmpty(HeadingText)
            || !string.IsNullOrEmpty(SpeedText);
    }
}
