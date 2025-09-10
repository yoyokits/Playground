// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using WorldMapControls.Extensions;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Comprehensive test suite to validate all country mappings and identify any issues.
    /// </summary>
    public static class CountryMappingTestSuite
    {
        /// <summary>
        /// Tests all possible country mappings to ensure complete coverage.
        /// </summary>
        public static CountryMappingTestResult RunCompleteTest()
        {
            var result = new CountryMappingTestResult();
            
            // Test 1: Verify all CountryCode enums have reverse mappings
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                if (country == Country.Unknown)
                {
                    result.UnmappedCountryCodes.Add(code);
                }
                else
                {
                    result.MappedCountryCodes.Add(code);
                }
            }
            
            // Test 2: Verify all Country enums have forward mappings
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown);
            foreach (var country in allCountries)
            {
                var code = country.ToCountryCode();
                if (code == CountryCode.Unknown)
                {
                    result.UnmappedCountries.Add(country);
                }
                else
                {
                    result.MappedCountries.Add(country);
                }
            }
            
            // Test 3: Test bidirectional consistency
            foreach (var country in allCountries)
            {
                var code = country.ToCountryCode();
                if (code != CountryCode.Unknown)
                {
                    var backToCountry = code.ToCountry();
                    if (backToCountry != country)
                    {
                        result.BidirectionalErrors.Add($"{country} -> {code} -> {backToCountry}");
                    }
                }
            }
            
            // Test 4: Test common problematic country names
            var problematicNames = new[]
            {
                "Dem. Rep. Congo", "Congo, Dem. Rep.", "Democratic Republic of the Congo",
                "Congo", "Congo, Rep.", "Republic of the Congo",
                "Central African Rep.", "Central African Republic", "CAR",
                "S. Sudan", "South Sudan", "Republic of South Sudan",
                "Eswatini", "Swaziland", "Kingdom of Swaziland",
                "North Macedonia", "Macedonia", "FYROM",
                "Czech Republic", "Czechia",
                "Bosnia and Herzegovina", "Bosnia & Herzegovina", "BiH",
                "Vatican City", "Holy See", "Vatican",
                "Brunei", "Brunei Darussalam",
                "Laos", "Lao PDR", "Lao People's Democratic Republic"
            };
            
            foreach (var name in problematicNames)
            {
                var mapped = GeoJsonNameMapper.MapGeoJsonNameToCountry(name);
                if (mapped == Country.Unknown)
                {
                    result.UnmappedProblematicNames.Add(name);
                }
                else
                {
                    result.MappedProblematicNames.Add((name, mapped));
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Generates a comprehensive report of all mapping test results.
        /// </summary>
        public static string GenerateTestReport()
        {
            var result = RunCompleteTest();
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== COMPREHENSIVE COUNTRY MAPPING TEST REPORT ===");
            report.AppendLine($"Test Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            // Summary
            report.AppendLine("SUMMARY:");
            report.AppendLine($"? Mapped CountryCodes: {result.MappedCountryCodes.Count}");
            report.AppendLine($"? Unmapped CountryCodes: {result.UnmappedCountryCodes.Count}");
            report.AppendLine($"? Mapped Countries: {result.MappedCountries.Count}");
            report.AppendLine($"? Unmapped Countries: {result.UnmappedCountries.Count}");
            report.AppendLine($"? Bidirectional Errors: {result.BidirectionalErrors.Count}");
            report.AppendLine($"? Mapped Problematic Names: {result.MappedProblematicNames.Count}");
            report.AppendLine($"? Unmapped Problematic Names: {result.UnmappedProblematicNames.Count}");
            report.AppendLine();
            
            // Detailed results
            if (result.UnmappedCountryCodes.Any())
            {
                report.AppendLine("UNMAPPED COUNTRY CODES (need Country enum entries):");
                foreach (var code in result.UnmappedCountryCodes.OrderBy(c => c.ToString()))
                {
                    var name = code.GetCountryName();
                    report.AppendLine($"   {code} ({name}) - ADD TO Country ENUM");
                }
                report.AppendLine();
            }
            
            if (result.UnmappedCountries.Any())
            {
                report.AppendLine("UNMAPPED COUNTRIES (need CountryCode mappings):");
                foreach (var country in result.UnmappedCountries.OrderBy(c => c.ToString()))
                {
                    report.AppendLine($"   {country} - ADD TO CountryToCountryCode DICTIONARY");
                }
                report.AppendLine();
            }
            
            if (result.BidirectionalErrors.Any())
            {
                report.AppendLine("BIDIRECTIONAL MAPPING ERRORS:");
                foreach (var error in result.BidirectionalErrors)
                {
                    report.AppendLine($"   {error}");
                }
                report.AppendLine();
            }
            
            if (result.UnmappedProblematicNames.Any())
            {
                report.AppendLine("UNMAPPED PROBLEMATIC NAMES:");
                foreach (var name in result.UnmappedProblematicNames)
                {
                    report.AppendLine($"   '{name}' - ADD TO GeoJsonNameMapper");
                }
                report.AppendLine();
            }
            
            if (result.MappedProblematicNames.Any())
            {
                report.AppendLine("? SUCCESSFULLY MAPPED PROBLEMATIC NAMES:");
                foreach (var (name, country) in result.MappedProblematicNames)
                {
                    report.AppendLine($"   '{name}' -> {country}");
                }
                report.AppendLine();
            }
            
            // Final assessment
            var totalIssues = result.UnmappedCountryCodes.Count + result.UnmappedCountries.Count + 
                            result.BidirectionalErrors.Count + result.UnmappedProblematicNames.Count;
            
            if (totalIssues == 0)
            {
                report.AppendLine("?? ALL TESTS PASSED - COMPLETE MAPPING COVERAGE ACHIEVED!");
            }
            else
            {
                report.AppendLine($"?? {totalIssues} ISSUES FOUND - NEEDS ATTENTION");
            }
            
            return report.ToString();
        }
    }
    
    public class CountryMappingTestResult
    {
        public List<CountryCode> MappedCountryCodes { get; set; } = new();
        public List<CountryCode> UnmappedCountryCodes { get; set; } = new();
        public List<Country> MappedCountries { get; set; } = new();
        public List<Country> UnmappedCountries { get; set; } = new();
        public List<string> BidirectionalErrors { get; set; } = new();
        public List<(string Name, Country Country)> MappedProblematicNames { get; set; } = new();
        public List<string> UnmappedProblematicNames { get; set; } = new();
    }
}