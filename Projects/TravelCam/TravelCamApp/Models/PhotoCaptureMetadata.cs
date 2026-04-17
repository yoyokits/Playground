// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// Data collected at capture time and written into JPEG EXIF metadata.

namespace TravelCamApp.Models
{
    public class PhotoCaptureMetadata
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Temperature { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Heading { get; set; }
        /// <summary>Device movement speed in metres per second (from GPS).</summary>
        public double? SpeedMps { get; set; }
        public bool FlashFired { get; set; }
        public string AspectRatioLabel { get; set; } = "Full";
        public string ResolutionLabel { get; set; } = "Auto";
    }
}
