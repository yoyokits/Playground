// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WorldMapControls.Models.Enums;
using WorldMapControls.Models; // <-- Add this using for CountryColorMapping
using WorldMapControls.Extensions;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Maps ISO country codes to country names as listed in world-countries.geo.json.
    /// </summary>
    public static class GeoJsonCountryCodeMapper
    {
        /// <summary>
        /// Parses the GeoJSON and returns a dictionary of CountryCode to country name.
        /// </summary>
        public static Dictionary<CountryCode, string> BuildCountryCodeToNameMap(string geoJson)
        {
            var dict = new Dictionary<CountryCode, string>();
            using var doc = JsonDocument.Parse(geoJson);
            var root = doc.RootElement;
            if (!root.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("GeoJSON missing 'features' array");

            foreach (var feature in features.EnumerateArray())
            {
                if (!feature.TryGetProperty("properties", out var props)) continue;
                string? code = null;
                string? name = null;

                // Try common property names for code and name
                if (props.TryGetProperty("ISO_A2", out var codeProp) && codeProp.ValueKind == JsonValueKind.String)
                    code = codeProp.GetString();
                else if (props.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                    code = idProp.GetString();
                else if (props.TryGetProperty("iso_a2", out var codeProp2) && codeProp2.ValueKind == JsonValueKind.String)
                    code = codeProp2.GetString();

                if (props.TryGetProperty("ADMIN", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    name = nameProp.GetString();
                else if (props.TryGetProperty("NAME", out var nameProp2) && nameProp2.ValueKind == JsonValueKind.String)
                    name = nameProp2.GetString();
                else if (props.TryGetProperty("name", out var nameProp3) && nameProp3.ValueKind == JsonValueKind.String)
                    name = nameProp3.GetString();

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) continue;

                if (Enum.TryParse<CountryCode>(code, true, out var cc) && cc != CountryCode.Unknown)
                {
                    dict[cc] = name;
                }
            }
            return dict;
        }

        /// <summary>
        /// Verifies that all countries in the GeoJSON are present in the color map.
        /// Returns missing codes and names.
        /// </summary>
        public static List<(CountryCode code, string name)> FindMissingCountries(
            Dictionary<CountryCode, string> geoJsonDict,
            IReadOnlyList<CountryColorMapping> colorMappings)
        {
            var mappedCodes = new HashSet<CountryCode>(colorMappings.Select(m => CountryCodeExtensions.ToCountryCode(m.Country)));
            var missing = new List<(CountryCode, string)>();
            foreach (var kvp in geoJsonDict)
            {
                if (!mappedCodes.Contains(kvp.Key))
                    missing.Add((kvp.Key, kvp.Value));
            }
            return missing;
        }
    }
}
