// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using WorldMapControls.Models.Enums;
using WorldMapControls.Extensions; // Add this for CountryCode extensions

namespace WorldMapControls.Services
{
    /// <summary>
    /// Utility to verify that the GeoJSON country name mapping fixes are working correctly.
    /// </summary>
    public static class CountryMappingVerifier
    {
        /// <summary>
        /// Tests all the problematic country names mentioned in the user's issue
        /// to verify they are now properly mapped to Country enum values.
        /// </summary>
        public static Dictionary<string, (Country MappedTo, bool IsFixed)> VerifyProblematicCountries()
        {
            var problematicNames = new[]
            {
                // Congo variations
                "Congo",
                "Congo, Rep.", 
                "Congo, Dem. Rep.",
                "Democratic Republic of the Congo",
                "Republic of the Congo",
                "DRC",
                "Zaire",
                "Congo-Kinshasa",
                "Congo-Brazzaville",
                
                // Other mentioned countries
                "Central African Rep.",
                "Central African Republic",
                "CAR",
                "S. Sudan",
                "South Sudan",
                "Sudan",
                "Turkmenistan",
                "Georgia",
                "Armenia",
                "Somalia",
                "Iceland",
                "Ireland",
                
                // Additional common variations
                "Timor-Leste",
                "East Timor",
                "Myanmar",
                "Burma",
                "Ivory Coast",
                "Côte d'Ivoire"
            };

            var results = new Dictionary<string, (Country MappedTo, bool IsFixed)>();
            
            foreach (var name in problematicNames)
            {
                var mapped = GeoJsonNameMapper.MapGeoJsonNameToCountry(name);
                var isFixed = mapped != Country.Unknown;
                results[name] = (mapped, isFixed);
            }

            return results;
        }

        /// <summary>
        /// Generates a comprehensive report showing which countries are now fixed
        /// and which might still have issues.
        /// </summary>
        public static string GenerateFixReport()
        {
            var results = VerifyProblematicCountries();
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== CONGO & PROBLEMATIC COUNTRIES FIX VERIFICATION ===");
            report.AppendLine();
            
            var fixedResults = results.Where(r => r.Value.IsFixed).ToList();
            var stillBrokenResults = results.Where(r => !r.Value.IsFixed).ToList();
            
            report.AppendLine($"? FIXED COUNTRIES: {fixedResults.Count}/{results.Count}");
            foreach (var kvp in fixedResults.OrderBy(x => x.Key))
            {
                report.AppendLine($"   '{kvp.Key}' -> {kvp.Value.MappedTo}");
            }
            
            report.AppendLine();
            
            if (stillBrokenResults.Any())
            {
                report.AppendLine($"? STILL BROKEN: {stillBrokenResults.Count}");
                foreach (var kvp in stillBrokenResults.OrderBy(x => x.Key))
                {
                    report.AppendLine($"   '{kvp.Key}' -> UNMAPPED");
                }
            }
            else
            {
                report.AppendLine("?? ALL PROBLEMATIC COUNTRIES ARE NOW FIXED!");
            }
            
            report.AppendLine();
            report.AppendLine("=== CONGO SPECIFIC VERIFICATION ===");
            var congoTests = new[]
            {
                ("Congo", "Should map to Republic of Congo"),
                ("Congo, Rep.", "Should map to Republic of Congo"),
                ("Congo, Dem. Rep.", "Should map to Democratic Republic of Congo"),
                ("DRC", "Should map to Democratic Republic of Congo")
            };
            
            foreach (var test in congoTests)
            {
                var mapped = GeoJsonNameMapper.MapGeoJsonNameToCountry(test.Item1);
                var status = mapped != Country.Unknown ? "?" : "?";
                report.AppendLine($"{status} '{test.Item1}' -> {mapped} ({test.Item2})");
            }

            return report.ToString();
        }

        /// <summary>
        /// Verifies that all CountryCode enum values have corresponding Country mappings
        /// for color map generation.
        /// </summary>
        public static List<CountryCode> FindUnmappedCountryCodes()
        {
            var allCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            var unmapped = new List<CountryCode>();
            
            foreach (var code in allCodes)
            {
                var country = code.ToCountry();
                if (country == Country.Unknown)
                {
                    unmapped.Add(code);
                }
            }
            
            return unmapped;
        }
    }
}