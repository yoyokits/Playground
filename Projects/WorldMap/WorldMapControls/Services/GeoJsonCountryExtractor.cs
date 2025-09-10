// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Diagnostic tool to extract and analyze all country names from the actual GeoJSON file.
    /// This helps identify exact spellings used in the map data.
    /// </summary>
    public static class GeoJsonCountryExtractor
    {
        /// <summary>
        /// Extracts all unique country names found in the GeoJSON file
        /// </summary>
        public static List<string> ExtractAllCountryNames(string geoJson)
        {
            var countryNames = new HashSet<string>();
            
            try
            {
                using var doc = JsonDocument.Parse(geoJson);
                var root = doc.RootElement;
                
                if (!root.TryGetProperty("features", out var features) || 
                    features.ValueKind != JsonValueKind.Array)
                {
                    return new List<string>();
                }

                foreach (var feature in features.EnumerateArray())
                {
                    if (!feature.TryGetProperty("properties", out var props))
                        continue;

                    // Try different common property names for country names
                    var possibleNames = new[]
                    {
                        "NAME", "ADMIN", "name", "admin", "NAME_EN", 
                        "COUNTRY", "Country", "country", "NAME_LONG"
                    };

                    foreach (var propName in possibleNames)
                    {
                        if (props.TryGetProperty(propName, out var nameProp) && 
                            nameProp.ValueKind == JsonValueKind.String)
                        {
                            var countryName = nameProp.GetString();
                            if (!string.IsNullOrWhiteSpace(countryName))
                            {
                                countryNames.Add(countryName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting country names: {ex.Message}");
            }

            return countryNames.OrderBy(n => n).ToList();
        }

        /// <summary>
        /// Analyzes all country names in the GeoJSON and reports which ones are mapped vs unmapped
        /// </summary>
        public static GeoJsonAnalysisResult AnalyzeCountryNames(string geoJson)
        {
            var allNames = ExtractAllCountryNames(geoJson);
            var mapped = new List<(string Name, Country MappedTo)>();
            var unmapped = new List<string>();

            foreach (var name in allNames)
            {
                var country = GeoJsonNameMapper.MapGeoJsonNameToCountry(name);
                if (country != Country.Unknown)
                {
                    mapped.Add((name, country));
                }
                else
                {
                    unmapped.Add(name);
                }
            }

            return new GeoJsonAnalysisResult
            {
                AllCountryNames = allNames,
                MappedCountries = mapped,
                UnmappedCountries = unmapped,
                TotalCountries = allNames.Count,
                MappedCount = mapped.Count,
                UnmappedCount = unmapped.Count
            };
        }

        /// <summary>
        /// Generates a comprehensive report of all country names found in the GeoJSON
        /// and their mapping status
        /// </summary>
        public static string GenerateCountryNamesReport(string geoJson)
        {
            var analysis = AnalyzeCountryNames(geoJson);
            var report = new System.Text.StringBuilder();

            report.AppendLine("=== GEOJSON COUNTRY NAMES ANALYSIS ===");
            report.AppendLine($"Total countries found: {analysis.TotalCountries}");
            report.AppendLine($"Mapped: {analysis.MappedCount}");
            report.AppendLine($"Unmapped: {analysis.UnmappedCount}");
            report.AppendLine($"Coverage: {(double)analysis.MappedCount / analysis.TotalCountries * 100:F1}%");
            report.AppendLine();

            if (analysis.UnmappedCountries.Any())
            {
                report.AppendLine("? UNMAPPED COUNTRIES (need to be added to GeoJsonNameMapper):");
                foreach (var name in analysis.UnmappedCountries)
                {
                    report.AppendLine($"   \"{name}\" = Country.???, // TODO: Map this");
                }
                report.AppendLine();
            }

            report.AppendLine("? SUCCESSFULLY MAPPED COUNTRIES:");
            foreach (var (name, mappedTo) in analysis.MappedCountries)
            {
                report.AppendLine($"   \"{name}\" -> {mappedTo}");
            }

            report.AppendLine();
            report.AppendLine("=== ALL COUNTRY NAMES IN GEOJSON ===");
            foreach (var name in analysis.AllCountryNames)
            {
                var status = analysis.MappedCountries.Any(m => m.Name == name) ? "?" : "?";
                report.AppendLine($"{status} \"{name}\"");
            }

            return report.ToString();
        }
    }

    public class GeoJsonAnalysisResult
    {
        public List<string> AllCountryNames { get; set; } = new();
        public List<(string Name, Country MappedTo)> MappedCountries { get; set; } = new();
        public List<string> UnmappedCountries { get; set; } = new();
        public int TotalCountries { get; set; }
        public int MappedCount { get; set; }
        public int UnmappedCount { get; set; }
    }
}