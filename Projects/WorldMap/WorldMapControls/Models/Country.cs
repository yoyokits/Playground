// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp.Models
{
    using System.Text.Json.Nodes;

    public record Country(string Name, CountryGeometryType GeometryType, JsonNode? Geometry);
}