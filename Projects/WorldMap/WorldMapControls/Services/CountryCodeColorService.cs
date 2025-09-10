// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using WorldMapControls.Extensions;
using WorldMapControls.Models;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Services
{
    public static class CountryCodeColorService
    {
        #region Public API

        /// <summary>
        /// Builds color mappings for all countries, ensuring no country is missed.
        /// Uses a combined approach to guarantee complete coverage.
        /// </summary>
        public static IReadOnlyList<CountryColorMapping> BuildColorMappings(ColorMapType type)
        {
            // HYBRID APPROACH: Get countries from BOTH enum and CountryCode mapping
            // This ensures complete coverage even when CountryCode mappings are incomplete
            var countries = new HashSet<Country>();

            // 1. Add all countries from the Country enum (except Unknown)
            countries.UnionWith(Enum.GetValues<Country>().Where(c => c != Country.Unknown));

            // 2. Add all countries mapped from CountryCode
            var countriesFromCodes = CountryCodeExtensions.GetAllCountryCodes()
                .Where(c => c != CountryCode.Unknown)
                .Select(c => c.ToCountry())
                .Where(c => c != Country.Unknown);
            countries.UnionWith(countriesFromCodes);

            // Convert to sorted array for stable coloring
            var allCountries = countries.OrderBy(c => (int)c).ToArray();

            // Log what we found for debugging
            System.Diagnostics.Debug.WriteLine($"[CountryColorService] Colormap will use {allCountries.Length} countries (including {countriesFromCodes.Count()} from CountryCode)");

            if (allCountries.Length == 0)
                return Array.Empty<CountryColorMapping>();

            // Generate sequential values and map to colors
            var values = Enumerable.Range(0, allCountries.Length)
                .Select(i => (double)i)
                .ToArray();

            var colors = ColorMapCalculator.MapValues(values, type);

            // Create mappings for ALL countries
            var list = new List<CountryColorMapping>(allCountries.Length);
            for (int i = 0; i < allCountries.Length; i++)
            {
                var country = allCountries[i];
                var brush = new SolidColorBrush(colors[i]);
                if (brush.CanFreeze) brush.Freeze();
                list.Add(new CountryColorMapping(country, brush));
            }

            return list;
        }

        // Original simple diagnostic (backward compatible)
        public static CountryCoverageDiagnostics Diagnose(MapData data)
        {
            if (data == null) return new CountryCoverageDiagnostics(
                new Dictionary<Country, string[]>(0),
                Array.Empty<string>());

            var unknown = data.Countries
                .Where(c => c.Country == Country.Unknown)
                .Select(c => c.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToArray();

            var mapped = data.Countries
                .Where(c => c.Country != Country.Unknown)
                .GroupBy(c => c.Country)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct().ToArray());

            return new CountryCoverageDiagnostics(mapped, unknown);
        }

        // Detailed diagnostic including enum/dictionary/code coverage
        public static DetailedCountryCoverageDiagnostics DiagnoseDetailed(MapData data)
        {
            if (data == null)
            {
                return new DetailedCountryCoverageDiagnostics(
                    EnumCountries: Array.Empty<Country>(),
                    DictionaryCountries: Array.Empty<Country>(),
                    MappedCountries: Array.Empty<Country>(),
                    MissingInMap: Array.Empty<Country>(),
                    MissingInDictionary: Array.Empty<Country>(),
                    MissingInDictionaryFromEnum: Array.Empty<Country>(),
                    UnknownFeatureNames: Array.Empty<string>(),
                    FeatureCountryCodeGuesses: new Dictionary<string, CountryCode>(),
                    CountryToNames: new Dictionary<Country, string[]>(),
                    OrphanCountryCodes: Array.Empty<CountryCode>(),
                    AfricanCountries: Array.Empty<Country>() // Added African countries tracking
                );
            }

            var enumCountries = Enum.GetValues<Country>()
                .Where(c => c != Country.Unknown)
                .ToArray();

            var dictCountries = MapDictionaries.CountryToName.Keys
                .Where(c => c != Country.Unknown)
                .ToArray();

            var mappedCountries = data.Countries
                .Where(ci => ci.Country != Country.Unknown)
                .Select(ci => ci.Country)
                .Distinct()
                .ToArray();

            // Find countries missing from the map vs. what's in the enum
            var missingInMap = enumCountries.Except(mappedCountries).OrderBy(c => c).ToArray();
            var missingInDictionary = mappedCountries.Except(dictCountries).OrderBy(c => c).ToArray();
            var missingInDictionaryFromEnum = enumCountries.Except(dictCountries).OrderBy(c => c).ToArray();

            var unknownNames = data.Countries
                .Where(ci => ci.Country == Country.Unknown)
                .Select(ci => ci.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToArray();

            // Build name list per mapped country
            var countryToNames = data.Countries
                .Where(ci => ci.Country != Country.Unknown)
                .GroupBy(ci => ci.Country)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct().OrderBy(n => n).ToArray());

            // Attempt to guess ISO codes for each feature name (even unknown)
            var featureNameToCode = new Dictionary<string, CountryCode>(StringComparer.OrdinalIgnoreCase);
            foreach (var ci in data.Countries)
            {
                if (!featureNameToCode.ContainsKey(ci.Name))
                {
                    var code = CountryCodeExtensions.GetCountryCode(ci.Name);
                    featureNameToCode[ci.Name] = code;
                }
            }

            // Detect orphan codes (present in ISO list but not encountered in map at all)
            var allCodes = CountryCodeExtensions.GetAllCountryCodes().Where(c => c != CountryCode.Unknown).ToArray();
            var encounteredCodes = featureNameToCode.Values.Where(c => c != CountryCode.Unknown).Distinct().ToHashSet();
            var orphanCodes = allCodes.Where(c => !encounteredCodes.Contains(c)).OrderBy(c => c.ToString()).ToArray();

            // Identify African countries (specifically track them since they were mentioned as problematic)
            var africanCountries = mappedCountries
                .Where(c => MapDictionaries.CountryToContinent.TryGetValue(c, out var continent) &&
                            continent == Continent.Africa)
                .OrderBy(c => c)
                .ToArray();

            return new DetailedCountryCoverageDiagnostics(
                EnumCountries: enumCountries,
                DictionaryCountries: dictCountries,
                MappedCountries: mappedCountries,
                MissingInMap: missingInMap,
                MissingInDictionary: missingInDictionary,
                MissingInDictionaryFromEnum: missingInDictionaryFromEnum,
                UnknownFeatureNames: unknownNames,
                FeatureCountryCodeGuesses: featureNameToCode,
                CountryToNames: countryToNames,
                OrphanCountryCodes: orphanCodes,
                AfricanCountries: africanCountries
            );
        }

        /// <summary>
        /// Finds countries in the map that are missing from the current colormap
        /// </summary>
        public static HashSet<Country> FindMissingCountriesInMap(MapData data, IReadOnlyList<CountryColorMapping> mappings)
        {
            if (data == null || mappings == null)
                return new HashSet<Country>();

            // Extract countries present in the mappings
            var mappedCountries = new HashSet<Country>(mappings.Select(m => m.Country));

            // Find countries in the map that are not in the mappings
            var countriesInMap = data.Countries
                .Select(c => c.Country)
                .Where(c => c != Country.Unknown)
                .Distinct();

            var missingCountries = new HashSet<Country>(countriesInMap.Except(mappedCountries));

            // Log any missing countries
            if (missingCountries.Count > 0)
            {
                var missingNames = missingCountries
                    .Select(c => MapDictionaries.CountryToName.TryGetValue(c, out var name) ? $"{c} ({name})" : c.ToString())
                    .ToArray();

                System.Diagnostics.Debug.WriteLine($"[CountryColorService] Found {missingCountries.Count} countries in map missing from colormap: {string.Join(", ", missingNames)}");
            }

            return missingCountries;
        }

        #endregion Public API
    }

    #region Records

    public sealed record CountryCoverageDiagnostics(
        IReadOnlyDictionary<Country, string[]> Mapped,
        IReadOnlyList<string> UnknownNames);

    public sealed record DetailedCountryCoverageDiagnostics(
        IReadOnlyList<Country> EnumCountries,
        IReadOnlyList<Country> DictionaryCountries,
        IReadOnlyList<Country> MappedCountries,
        IReadOnlyList<Country> MissingInMap,
        IReadOnlyList<Country> MissingInDictionary,
        IReadOnlyList<Country> MissingInDictionaryFromEnum,
        IReadOnlyList<string> UnknownFeatureNames,
        IReadOnlyDictionary<string, CountryCode> FeatureCountryCodeGuesses,
        IReadOnlyDictionary<Country, string[]> CountryToNames,
        IReadOnlyList<CountryCode> OrphanCountryCodes,
        IReadOnlyList<Country> AfricanCountries);

    #endregion Records
}