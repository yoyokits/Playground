// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Extensions
{
    /// <summary>
    /// Provides comprehensive country name variations and parsing capabilities.
    /// This addresses the issue where country names in data sources don't exactly match enum values.
    /// </summary>
    public static class CountryNameVariationsExtensions
    {
        #region Fields

        /// <summary>
        /// Comprehensive mapping from various country name formats to CountryCode enum values.
        /// This dictionary handles multiple name variations for each country, including:
        /// - Official names, common names, historical names
        /// - UN format names with parentheses
        /// - Shortened forms and abbreviations
        /// - Alternative spellings and transliterations
        /// </summary>
        private static readonly Dictionary<string, CountryCode> CountryNameToCodeMapping = 
            BuildComprehensiveCountryNameDictionary();

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the CountryCode for a given country name, handling various name formats and variations.
        /// This is the primary method for parsing country names from any source.
        /// </summary>
        /// <param name="countryName">The country name to parse</param>
        /// <returns>The corresponding CountryCode, or CountryCode.Unknown if not found</returns>
        public static CountryCode ParseCountryName(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                return CountryCode.Unknown;

            // Try direct lookup first
            if (CountryNameToCodeMapping.TryGetValue(countryName, out var directMatch))
                return directMatch;

            // Try normalized lookup (remove special characters, case insensitive)
            var normalized = NormalizeCountryName(countryName);
            var match = CountryNameToCodeMapping.FirstOrDefault(kvp => 
                NormalizeCountryName(kvp.Key) == normalized);
            
            return match.Key != null ? match.Value : CountryCode.Unknown;
        }

        /// <summary>
        /// Gets all known name variations for a given CountryCode.
        /// Useful for testing and validation purposes.
        /// </summary>
        /// <param name="countryCode">The country code to get variations for</param>
        /// <returns>List of all known name variations for the country</returns>
        public static List<string> GetNameVariations(CountryCode countryCode)
        {
            return CountryNameToCodeMapping
                .Where(kvp => kvp.Value == countryCode)
                .Select(kvp => kvp.Key)
                .OrderBy(name => name)
                .ToList();
        }

        /// <summary>
        /// Gets the complete country name to country code mapping dictionary.
        /// </summary>
        /// <returns>Read-only dictionary of all country name variations to country codes</returns>
        public static IReadOnlyDictionary<string, CountryCode> GetAllNameMappings()
        {
            return CountryNameToCodeMapping;
        }

        /// <summary>
        /// Validates that a country name can be parsed and provides suggestions if not.
        /// </summary>
        /// <param name="countryName">The country name to validate</param>
        /// <returns>Validation result with success status and suggestions</returns>
        public static CountryNameValidationResult ValidateCountryName(string countryName)
        {
            var result = new CountryNameValidationResult
            {
                InputName = countryName,
                IsValid = false,
                ParsedCountryCode = CountryCode.Unknown,
                Suggestions = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(countryName))
            {
                result.ErrorMessage = "Country name cannot be empty";
                return result;
            }

            var parsedCode = ParseCountryName(countryName);
            if (parsedCode != CountryCode.Unknown)
            {
                result.IsValid = true;
                result.ParsedCountryCode = parsedCode;
                result.ParsedCountry = parsedCode.ToCountry();
                return result;
            }

            // Provide suggestions for similar names
            result.ErrorMessage = $"Country name '{countryName}' could not be parsed";
            result.Suggestions = FindSimilarCountryNames(countryName);
            
            return result;
        }

        /// <summary>
        /// Attempts to parse multiple country names from a single string (e.g., "United States, Canada, Mexico").
        /// </summary>
        /// <param name="multipleCountryNames">String containing multiple country names separated by delimiters</param>
        /// <param name="delimiters">Characters to use as delimiters (default: comma, semicolon, pipe)</param>
        /// <returns>List of parsed country codes</returns>
        public static List<CountryCode> ParseMultipleCountryNames(
            string multipleCountryNames, 
            char[]? delimiters = null)
        {
            delimiters ??= new[] { ',', ';', '|' };

            if (string.IsNullOrWhiteSpace(multipleCountryNames))
                return new List<CountryCode>();

            return multipleCountryNames
                .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => ParseCountryName(name.Trim()))
                .Where(code => code != CountryCode.Unknown)
                .Distinct()
                .ToList();
        }

        #endregion

        #region Private Methods

        private static Dictionary<string, CountryCode> BuildComprehensiveCountryNameDictionary()
        {
            var dictionary = new Dictionary<string, CountryCode>(StringComparer.OrdinalIgnoreCase);

            // Start with all standard country code names
            var allCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            foreach (var code in allCodes)
            {
                var standardName = code.GetCountryName();
                if (!string.IsNullOrWhiteSpace(standardName))
                {
                    dictionary[standardName] = code;
                }
            }

            // Add comprehensive variations for each country
            AddCountryVariations(dictionary);

            return dictionary;
        }

        private static void AddCountryVariations(Dictionary<string, CountryCode> dictionary)
        {
            // Helper method to safely add variations
            void AddVariation(string name, CountryCode code)
            {
                if (!string.IsNullOrWhiteSpace(name) && !dictionary.ContainsKey(name))
                {
                    dictionary[name] = code;
                }
            }

            // DOMINICAN REPUBLIC - Critical fix for the user's issue
            AddVariation("Dominican Republic", CountryCode.DO);
            AddVariation("Dominican Rep.", CountryCode.DO);
            AddVariation("Dominicana", CountryCode.DO);
            AddVariation("Rep. Dominicana", CountryCode.DO);
            AddVariation("República Dominicana", CountryCode.DO);

            // CONGO VARIATIONS - Critical disambiguation
            AddVariation("Congo", CountryCode.CG); // Default to Republic of Congo
            AddVariation("Congo, Rep.", CountryCode.CG);
            AddVariation("Congo Republic", CountryCode.CG);
            AddVariation("Congo-Brazzaville", CountryCode.CG);
            AddVariation("Republic of the Congo", CountryCode.CG);
            AddVariation("Republic of Congo", CountryCode.CG);
            AddVariation("Congo (Brazzaville)", CountryCode.CG);
            
            AddVariation("Congo, Dem. Rep.", CountryCode.CD);
            AddVariation("Dem. Rep. Congo", CountryCode.CD);
            AddVariation("Democratic Rep. Congo", CountryCode.CD);
            AddVariation("Congo DR", CountryCode.CD);
            AddVariation("Congo DRC", CountryCode.CD);
            AddVariation("Congo-Kinshasa", CountryCode.CD);
            AddVariation("Democratic Republic of the Congo", CountryCode.CD);
            AddVariation("Democratic Republic of Congo", CountryCode.CD);
            AddVariation("Congo (Kinshasa)", CountryCode.CD);
            AddVariation("Zaire", CountryCode.CD);
            AddVariation("DRC", CountryCode.CD);

            // RUSSIA VARIATIONS
            AddVariation("Russia", CountryCode.RU);
            AddVariation("Russian Federation", CountryCode.RU);
            AddVariation("Russian Fed.", CountryCode.RU);
            AddVariation("Russia Fed.", CountryCode.RU);
            AddVariation("Rossiya", CountryCode.RU);
            AddVariation("??????", CountryCode.RU);

            // KOREA VARIATIONS
            AddVariation("Korea", CountryCode.KR); // Default to South Korea
            AddVariation("South Korea", CountryCode.KR);
            AddVariation("Korea, Republic of", CountryCode.KR);
            AddVariation("Republic of Korea", CountryCode.KR);
            AddVariation("Korea Rep.", CountryCode.KR);
            AddVariation("ROK", CountryCode.KR);
            AddVariation("S. Korea", CountryCode.KR);
            
            AddVariation("North Korea", CountryCode.KP);
            AddVariation("Korea, Dem. People's Rep. of", CountryCode.KP);
            AddVariation("Democratic People's Republic of Korea", CountryCode.KP);
            AddVariation("Korea DPR", CountryCode.KP);
            AddVariation("DPRK", CountryCode.KP);
            AddVariation("N. Korea", CountryCode.KP);

            // OFFICIAL UN NAMES (with parentheses)
            AddVariation("Iran (Islamic Republic of)", CountryCode.IR);
            AddVariation("Iran, Islamic Republic of", CountryCode.IR);
            AddVariation("Venezuela (Bolivarian Republic of)", CountryCode.VE);
            AddVariation("Venezuela, Bolivarian Republic of", CountryCode.VE);
            AddVariation("Tanzania, United Republic of", CountryCode.TZ);
            AddVariation("United Republic of Tanzania", CountryCode.TZ);
            AddVariation("Bolivia (Plurinational State of)", CountryCode.BO);
            AddVariation("Bolivia, Plurinational State of", CountryCode.BO);
            AddVariation("Syrian Arab Republic", CountryCode.SY);
            AddVariation("Lao People's Democratic Republic", CountryCode.LA);
            AddVariation("Lao PDR", CountryCode.LA);

            // UNITED STATES VARIATIONS
            AddVariation("United States", CountryCode.US);
            AddVariation("United States of America", CountryCode.US);
            AddVariation("USA", CountryCode.US);
            AddVariation("US", CountryCode.US);
            AddVariation("America", CountryCode.US);
            AddVariation("U.S.A.", CountryCode.US);
            AddVariation("U.S.", CountryCode.US);
            AddVariation("Estados Unidos", CountryCode.US);

            // UNITED KINGDOM VARIATIONS
            AddVariation("United Kingdom", CountryCode.GB);
            AddVariation("UK", CountryCode.GB);
            AddVariation("Great Britain", CountryCode.GB);
            AddVariation("Britain", CountryCode.GB);
            AddVariation("U.K.", CountryCode.GB);
            AddVariation("England", CountryCode.GB); // Often used incorrectly
            AddVariation("Scotland", CountryCode.GB);
            AddVariation("Wales", CountryCode.GB);
            AddVariation("Northern Ireland", CountryCode.GB);

            // EUROPEAN COUNTRIES
            AddVariation("Czech Republic", CountryCode.CZ);
            AddVariation("Czechia", CountryCode.CZ);
            AddVariation("Czech Rep.", CountryCode.CZ);
            
            AddVariation("Bosnia and Herzegovina", CountryCode.BA);
            AddVariation("Bosnia & Herzegovina", CountryCode.BA);
            AddVariation("Bosnia-Herzegovina", CountryCode.BA);
            AddVariation("BiH", CountryCode.BA);
            
            AddVariation("North Macedonia", CountryCode.MK);
            AddVariation("Macedonia", CountryCode.MK);
            AddVariation("FYROM", CountryCode.MK);
            AddVariation("Former Yugoslav Republic of Macedonia", CountryCode.MK);
            AddVariation("The former Yugoslav Republic of Macedonia", CountryCode.MK);

            // ASIAN COUNTRIES
            AddVariation("Myanmar", CountryCode.MM);
            AddVariation("Burma", CountryCode.MM);
            AddVariation("Myanmar (Burma)", CountryCode.MM);
            
            AddVariation("Sri Lanka", CountryCode.LK);
            AddVariation("Ceylon", CountryCode.LK);
            
            AddVariation("United Arab Emirates", CountryCode.AE);
            AddVariation("UAE", CountryCode.AE);
            AddVariation("U.A.E.", CountryCode.AE);
            
            AddVariation("Saudi Arabia", CountryCode.SA);
            AddVariation("Kingdom of Saudi Arabia", CountryCode.SA);
            AddVariation("KSA", CountryCode.SA);

            // AFRICAN COUNTRIES
            AddVariation("Central African Republic", CountryCode.CF);
            AddVariation("Central African Rep.", CountryCode.CF);
            AddVariation("CAR", CountryCode.CF);
            AddVariation("Centrafrique", CountryCode.CF);
            
            AddVariation("South Sudan", CountryCode.SS);
            AddVariation("S. Sudan", CountryCode.SS);
            AddVariation("S Sudan", CountryCode.SS);
            AddVariation("Republic of South Sudan", CountryCode.SS);
            
            AddVariation("Ivory Coast", CountryCode.CI);
            AddVariation("Côte d'Ivoire", CountryCode.CI);
            AddVariation("Cote d'Ivoire", CountryCode.CI);

            // OCEANIA COUNTRIES
            AddVariation("New Zealand", CountryCode.NZ);
            AddVariation("NZ", CountryCode.NZ);
            
            AddVariation("Papua New Guinea", CountryCode.PG);
            AddVariation("PNG", CountryCode.PG);

            // SMALL ISLAND STATES
            AddVariation("Saint Lucia", CountryCode.LC);
            AddVariation("St. Lucia", CountryCode.LC);
            AddVariation("St Lucia", CountryCode.LC);
            
            AddVariation("Saint Vincent and the Grenadines", CountryCode.VC);
            AddVariation("St. Vincent and the Grenadines", CountryCode.VC);
            AddVariation("St Vincent and the Grenadines", CountryCode.VC);
            AddVariation("Saint Vincent & the Grenadines", CountryCode.VC);
            AddVariation("St. Vincent & the Grenadines", CountryCode.VC);
            
            AddVariation("Saint Kitts and Nevis", CountryCode.KN);
            AddVariation("St. Kitts and Nevis", CountryCode.KN);
            AddVariation("St Kitts and Nevis", CountryCode.KN);
            AddVariation("Saint Kitts & Nevis", CountryCode.KN);
            AddVariation("St. Kitts & Nevis", CountryCode.KN);
            
            AddVariation("Trinidad and Tobago", CountryCode.TT);
            AddVariation("Trinidad & Tobago", CountryCode.TT);
            
            AddVariation("Antigua and Barbuda", CountryCode.AG);
            AddVariation("Antigua & Barbuda", CountryCode.AG);

            // VATICAN VARIATIONS
            AddVariation("Vatican City", CountryCode.VA);
            AddVariation("Vatican", CountryCode.VA);
            AddVariation("Vatican City State", CountryCode.VA);
            AddVariation("Holy See", CountryCode.VA);
            AddVariation("Holy See (Vatican City State)", CountryCode.VA);
            AddVariation("Holy See (Vatican City)", CountryCode.VA);

            // HISTORICAL NAMES
            AddVariation("Swaziland", CountryCode.SZ); // Now Eswatini
            AddVariation("Burma", CountryCode.MM); // Now Myanmar
            AddVariation("Ceylon", CountryCode.LK); // Now Sri Lanka
            AddVariation("Zaire", CountryCode.CD); // Now DRC
            AddVariation("East Timor", CountryCode.TL); // Also Timor-Leste
            AddVariation("Timor-Leste", CountryCode.TL);

            // SPECIAL CASES AND TERRITORIES
            AddVariation("Taiwan", CountryCode.TW);
            AddVariation("Republic of China", CountryCode.TW);
            AddVariation("Chinese Taipei", CountryCode.TW);
            
            AddVariation("Hong Kong", CountryCode.HK);
            AddVariation("Hong Kong SAR", CountryCode.HK);
            AddVariation("Hong Kong, China", CountryCode.HK);
            
            AddVariation("Macao", CountryCode.MO);
            AddVariation("Macau", CountryCode.MO);
            AddVariation("Macao SAR", CountryCode.MO);
            AddVariation("Macau SAR", CountryCode.MO);

            // COMMON ABBREVIATIONS AND ALTERNATIVE SPELLINGS
            foreach (var code in Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown))
            {
                // Add the two-letter code itself as a variation
                AddVariation(code.ToString(), code);
            }

            // KOSOVO VARIATIONS
            AddVariation("Kosovo", CountryCode.XK);
            AddVariation("Republic of Kosovo", CountryCode.XK);
            AddVariation("Kosova", CountryCode.XK);
            AddVariation("Kosovë", CountryCode.XK);
        }

        private static string NormalizeCountryName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return new string(name
                .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                .ToArray())
                .Replace(" ", "")
                .ToLowerInvariant();
        }

        private static List<string> FindSimilarCountryNames(string inputName)
        {
            if (string.IsNullOrWhiteSpace(inputName))
                return new List<string>();

            var inputNormalized = NormalizeCountryName(inputName);
            var suggestions = new List<string>();

            foreach (var countryName in CountryNameToCodeMapping.Keys)
            {
                var normalizedCountryName = NormalizeCountryName(countryName);
                
                // Simple similarity check - contains or is contained by
                if (inputNormalized.Length >= 3 && normalizedCountryName.Length >= 3)
                {
                    if (normalizedCountryName.Contains(inputNormalized) || 
                        inputNormalized.Contains(normalizedCountryName))
                    {
                        suggestions.Add(countryName);
                    }
                }
            }

            return suggestions.Take(5).ToList(); // Limit to top 5 suggestions
        }

        #endregion

        #region Helper Classes

        public class CountryNameValidationResult
        {
            public string InputName { get; set; } = string.Empty;
            public bool IsValid { get; set; }
            public CountryCode ParsedCountryCode { get; set; } = CountryCode.Unknown;
            public Country ParsedCountry { get; set; } = Country.Unknown;
            public string ErrorMessage { get; set; } = string.Empty;
            public List<string> Suggestions { get; set; } = new();
        }

        #endregion

        #region Country Enum Extensions

        /// <summary>
        /// Gets all known GeoJSON name variations for a given Country enum value.
        /// This is useful for mapping Country enum values back to their various name representations.
        /// </summary>
        /// <param name="country">The country to get GeoJSON name variations for</param>
        /// <returns>List of all known GeoJSON name variations for the country</returns>
        public static List<string> GetGeoJsonNameVariations(this Country country)
        {
            var countryCode = country.ToCountryCode();
            if (countryCode == CountryCode.Unknown)
                return new List<string>();
                
            return GetNameVariations(countryCode);
        }

        /// <summary>
        /// Gets all known name variations for a given Country enum value.
        /// </summary>
        /// <param name="country">The country to get variations for</param>
        /// <returns>List of all known name variations for the country</returns>
        public static List<string> GetNameVariations(this Country country)
        {
            var countryCode = country.ToCountryCode();
            return GetNameVariations(countryCode);
        }

        #endregion
    }
}