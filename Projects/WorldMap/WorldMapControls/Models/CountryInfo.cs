// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Models
{
    using System.Text.Json.Nodes;
    using WorldMapControls.Models.Enums;

    public record CountryInfo(
        string Name,
        Country Country,
        CountryGeometryType GeometryType,
        JsonNode? Geometry);
}