// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using WorldMapControls.Extensions;
using WorldMapControls.Models;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Comprehensive validation service to ensure all countries are properly mapped
    /// and can be colored by the color map system.
    /// </summary>
    public static class CountryMappingValidator
    {
        public static CountryMappingValidationResult ValidateAllMappings()
        {
            var result = new CountryMappingValidationResult();

            // Get all enum values
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown).ToHashSet();
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown).ToHashSet();

            // Check CountryCode -> Country mappings
            var mappedCountryCodesFromExtensions = new HashSet<CountryCode>();
            var mappedCountriesFromExtensions = new HashSet<Country>();

            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                if (country != Country.Unknown)
                {
                    mappedCountryCodesFromExtensions.Add(code);
                    mappedCountriesFromExtensions.Add(country);
                }
            }

            // Check Country -> CountryCode mappings
            var mappedCountriesFromReverse = new HashSet<Country>();
            var mappedCountryCodesFromReverse = new HashSet<CountryCode>();

            foreach (var country in allCountries)
            {
                var code = country.ToCountryCode();
                if (code != CountryCode.Unknown)
                {
                    mappedCountriesFromReverse.Add(country);
                    mappedCountryCodesFromReverse.Add(code);
                }
            }

            // Check MapDictionaries coverage
            var countriesInMapDictionaries = MapDictionaries.CountryToName.Keys.Where(c => c != Country.Unknown).ToHashSet();
            var countriesInContinentMap = MapDictionaries.CountryToContinent.Keys.Where(c => c != Country.Unknown).ToHashSet();

            // Find missing mappings
            result.CountryCodesWithoutCountryMapping = allCountryCodes.Except(mappedCountryCodesFromExtensions).ToList();
            result.CountriesWithoutCountryCodeMapping = allCountries.Except(mappedCountriesFromReverse).ToList();
            result.CountriesNotInMapDictionaries = allCountries.Except(countriesInMapDictionaries).ToList();
            result.CountriesNotInContinentMap = allCountries.Except(countriesInContinentMap).ToList();

            // Find bidirectional mapping inconsistencies
            result.BidirectionalMappingIssues = new List<string>();
            foreach (var country in allCountries)
            {
                var code = country.ToCountryCode();
                if (code != CountryCode.Unknown)
                {
                    var backToCountry = code.ToCountry();
                    if (backToCountry != country)
                    {
                        result.BidirectionalMappingIssues.Add(
                            $"{country} -> {code} -> {backToCountry} (inconsistent)");
                    }
                }
            }

            // Check for orphaned entries
            result.OrphanedCountryCodes = allCountryCodes.Where(code => 
                !mappedCountryCodesFromExtensions.Contains(code)).ToList();

            // Summary statistics
            result.TotalCountryCodes = allCountryCodes.Count;
            result.TotalCountries = allCountries.Count;
            result.MappedCountryCodes = mappedCountryCodesFromExtensions.Count;
            result.MappedCountries = mappedCountriesFromExtensions.Count;

            return result;
        }

        /// <summary>
        /// Generates a comprehensive report of all missing countries and mapping issues.
        /// </summary>
        public static string GenerateValidationReport()
        {
            var result = ValidateAllMappings();
            var report = new System.Text.StringBuilder();

            report.AppendLine("=== COUNTRY MAPPING VALIDATION REPORT ===");
            report.AppendLine($"Total CountryCode enum values: {result.TotalCountryCodes}");
            report.AppendLine($"Total Country enum values: {result.TotalCountries}");
            report.AppendLine($"Mapped CountryCodes: {result.MappedCountryCodes}");
            report.AppendLine($"Mapped Countries: {result.MappedCountries}");
            report.AppendLine();

            if (result.CountryCodesWithoutCountryMapping.Any())
            {
                report.AppendLine("? COUNTRY CODES WITHOUT COUNTRY MAPPING:");
                foreach (var code in result.CountryCodesWithoutCountryMapping.OrderBy(c => c.ToString()))
                {
                    var name = code.GetCountryName();
                    report.AppendLine($"   {code} ({name})");
                }
                report.AppendLine();
            }

            if (result.CountriesWithoutCountryCodeMapping.Any())
            {
                report.AppendLine("? COUNTRIES WITHOUT COUNTRY CODE MAPPING:");
                foreach (var country in result.CountriesWithoutCountryCodeMapping.OrderBy(c => c.ToString()))
                {
                    report.AppendLine($"   {country}");
                }
                report.AppendLine();
            }

            if (result.CountriesNotInMapDictionaries.Any())
            {
                report.AppendLine("? COUNTRIES NOT IN MAP DICTIONARIES:");
                foreach (var country in result.CountriesNotInMapDictionaries.OrderBy(c => c.ToString()))
                {
                    report.AppendLine($"   {country}");
                }
                report.AppendLine();
            }

            if (result.CountriesNotInContinentMap.Any())
            {
                report.AppendLine("? COUNTRIES NOT IN CONTINENT MAP:");
                foreach (var country in result.CountriesNotInContinentMap.OrderBy(c => c.ToString()))
                {
                    report.AppendLine($"   {country}");
                }
                report.AppendLine();
            }

            if (result.BidirectionalMappingIssues.Any())
            {
                report.AppendLine("? BIDIRECTIONAL MAPPING ISSUES:");
                foreach (var issue in result.BidirectionalMappingIssues)
                {
                    report.AppendLine($"   {issue}");
                }
                report.AppendLine();
            }

            if (result.OrphanedCountryCodes.Any())
            {
                report.AppendLine("? ORPHANED COUNTRY CODES (no reverse mapping):");
                foreach (var code in result.OrphanedCountryCodes.OrderBy(c => c.ToString()))
                {
                    var name = code.GetCountryName();
                    report.AppendLine($"   {code} ({name})");
                }
                report.AppendLine();
            }

            // Success message
            if (!result.CountryCodesWithoutCountryMapping.Any() && 
                !result.CountriesWithoutCountryCodeMapping.Any() &&
                !result.CountriesNotInMapDictionaries.Any() &&
                !result.CountriesNotInContinentMap.Any() &&
                !result.BidirectionalMappingIssues.Any())
            {
                report.AppendLine("? ALL MAPPINGS ARE COMPLETE AND CONSISTENT!");
            }

            return report.ToString();
        }
    }

    /// <summary>
    /// Result of country mapping validation.
    /// </summary>
    public class CountryMappingValidationResult
    {
        public List<CountryCode> CountryCodesWithoutCountryMapping { get; set; } = new();
        public List<Country> CountriesWithoutCountryCodeMapping { get; set; } = new();
        public List<Country> CountriesNotInMapDictionaries { get; set; } = new();
        public List<Country> CountriesNotInContinentMap { get; set; } = new();
        public List<string> BidirectionalMappingIssues { get; set; } = new();
        public List<CountryCode> OrphanedCountryCodes { get; set; } = new();
        
        public int TotalCountryCodes { get; set; }
        public int TotalCountries { get; set; }
        public int MappedCountryCodes { get; set; }
        public int MappedCountries { get; set; }
    }
}