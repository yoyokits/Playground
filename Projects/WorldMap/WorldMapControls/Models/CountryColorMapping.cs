// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Models
{
    using System.Windows.Media;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Maps a Country enum to a Brush fill override.
    /// </summary>
    public record CountryColorMapping(Country Country, Brush Fill);
}