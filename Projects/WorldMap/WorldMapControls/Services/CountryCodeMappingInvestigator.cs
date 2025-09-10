// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldMapControls.Extensions;
using WorldMapControls.Models.Enums;
using WorldMapControls.Models;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Comprehensive investigation tool to verify every CountryCode has proper mappings
    /// </summary>
    public static class CountryCodeMappingInvestigator
    {
        public static string GenerateComprehensiveReport()
        {
            var report = new StringBuilder();
            report.AppendLine("=== COMPREHENSIVE COUNTRYCODE MAPPING INVESTIGATION ===");
            report.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            // Get all CountryCode enum values
            var allCountryCodes = Enum.GetValues<CountryCode>()
                .Where(c => c != CountryCode.Unknown)
                .OrderBy(c => c.ToString())
                .ToArray();

            report.AppendLine($"Total CountryCodes to verify: {allCountryCodes.Length}");
            report.AppendLine();

            // Investigation results
            var unmappedToCountry = new List<CountryCode>();
            var unmappedInCountryToCode = new List<CountryCode>();
            var missingFromCountryName = new List<CountryCode>();
            var missingFromMapDictionaries = new List<CountryCode>();

            report.AppendLine("=== DETAILED ANALYSIS BY COUNTRYCODE ===");

            foreach (var code in allCountryCodes)
            {
                var analysis = AnalyzeCountryCode(code);
                
                if (!analysis.HasCountryMapping)
                    unmappedToCountry.Add(code);
                
                if (!analysis.HasReverseMapping)
                    unmappedInCountryToCode.Add(code);
                    
                if (!analysis.HasNameMapping)
                    missingFromCountryName.Add(code);
                    
                if (!analysis.HasMapDictionaryEntry)
                    missingFromMapDictionaries.Add(code);

                // Log problematic ones
                if (!analysis.IsCompletelyMapped)
                {
                    report.AppendLine($"? {code} ({analysis.CountryName}):");
                    report.AppendLine($"   Country Mapping: {(analysis.HasCountryMapping ? "?" : "?")} -> {analysis.MappedCountry}");
                    report.AppendLine($"   Reverse Mapping: {(analysis.HasReverseMapping ? "?" : "?")}");
                    report.AppendLine($"   Name Mapping: {(analysis.HasNameMapping ? "?" : "?")}");
                    report.AppendLine($"   MapDict Entry: {(analysis.HasMapDictionaryEntry ? "?" : "?")}");
                    report.AppendLine();
                }
            }

            // Summary statistics
            report.AppendLine("=== SUMMARY STATISTICS ===");
            report.AppendLine($"Total CountryCodes: {allCountryCodes.Length}");
            report.AppendLine($"Unmapped to Country: {unmappedToCountry.Count}");
            report.AppendLine($"Missing reverse mapping: {unmappedInCountryToCode.Count}");
            report.AppendLine($"Missing from CountryName dict: {missingFromCountryName.Count}");
            report.AppendLine($"Missing from MapDictionaries: {missingFromMapDictionaries.Count}");
            report.AppendLine();

            // Detailed breakdowns
            if (unmappedToCountry.Any())
            {
                report.AppendLine("=== CODES MISSING COUNTRY MAPPING ===");
                foreach (var code in unmappedToCountry)
                {
                    var name = code.GetCountryName();
                    var suggestedCountry = SuggestCountryEnumName(name);
                    report.AppendLine($"{code} ({name}) -> Suggested: Country.{suggestedCountry}");
                }
                report.AppendLine();
            }

            if (unmappedInCountryToCode.Any())
            {
                report.AppendLine("=== CODES MISSING REVERSE MAPPING ===");
                foreach (var code in unmappedInCountryToCode)
                {
                    report.AppendLine($"{code} -> Need to add reverse mapping");
                }
                report.AppendLine();
            }

            // Generate fix code
            report.AppendLine("=== GENERATED FIX CODE ===");
            report.AppendLine(GenerateFixCode(unmappedToCountry));

            return report.ToString();
        }

        private static CountryCodeAnalysis AnalyzeCountryCode(CountryCode code)
        {
            var analysis = new CountryCodeAnalysis();
            analysis.CountryCode = code;

            // Check name mapping
            analysis.CountryName = code.GetCountryName();
            analysis.HasNameMapping = !string.IsNullOrEmpty(analysis.CountryName) && analysis.CountryName != code.ToString();

            // Check Country mapping
            analysis.MappedCountry = code.ToCountry();
            analysis.HasCountryMapping = analysis.MappedCountry != Country.Unknown;

            // Check reverse mapping (if we have a country, does it map back?)
            if (analysis.HasCountryMapping)
            {
                var reverseCode = analysis.MappedCountry.ToCountryCode();
                analysis.HasReverseMapping = reverseCode == code;
            }

            // Check MapDictionaries
            if (analysis.HasCountryMapping)
            {
                analysis.HasMapDictionaryEntry = MapDictionaries.CountryToName.ContainsKey(analysis.MappedCountry) &&
                                               MapDictionaries.CountryToContinent.ContainsKey(analysis.MappedCountry);
            }

            analysis.IsCompletelyMapped = analysis.HasCountryMapping && 
                                        analysis.HasReverseMapping && 
                                        analysis.HasNameMapping && 
                                        analysis.HasMapDictionaryEntry;

            return analysis;
        }

        private static string SuggestCountryEnumName(string countryName)
        {
            // Convert country name to PascalCase enum name
            var words = countryName.Split(' ', '-', '(', ')', ',', '.', '\'')
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim())
                .Where(w => w.Length > 0)
                .Where(w => !IsCommonWord(w));

            var enumName = string.Join("", words.Select(w => 
                char.ToUpper(w[0]) + w.Substring(1).ToLower()));

            // Handle special characters and cleanup
            enumName = enumName.Replace("'", "")
                              .Replace("â", "a")
                              .Replace("ç", "c")
                              .Replace("é", "e")
                              .Replace("ã", "a")
                              .Replace("õ", "o")
                              .Replace("í", "i")
                              .Replace("ú", "u")
                              .Replace("ñ", "n")
                              .Replace("ü", "u")
                              .Replace("ö", "o")
                              .Replace("à", "a")
                              .Replace("è", "e")
                              .Replace("ò", "o")
                              .Replace("ù", "u");

            return enumName;
        }

        private static bool IsCommonWord(string word)
        {
            var commonWords = new[] { "and", "the", "of", "in", "on", "at", "by", "for", "with", "to" };
            return commonWords.Contains(word.ToLower());
        }

        private static string GenerateFixCode(List<CountryCode> unmappedCodes)
        {
            var code = new StringBuilder();
            
            if (unmappedCodes.Any())
            {
                code.AppendLine("// Add to Country enum:");
                foreach (var countryCode in unmappedCodes)
                {
                    var name = countryCode.GetCountryName();
                    var enumName = SuggestCountryEnumName(name);
                    code.AppendLine($"{enumName}, // {name} ({countryCode})");
                }
                code.AppendLine();

                code.AppendLine("// Add to CountryToCountryCode dictionary:");
                foreach (var countryCode in unmappedCodes)
                {
                    var name = countryCode.GetCountryName();
                    var enumName = SuggestCountryEnumName(name);
                    code.AppendLine($"{{ Country.{enumName}, CountryCode.{countryCode} }},");
                }
                code.AppendLine();

                code.AppendLine("// Add to CountryCodeToCountry dictionary:");
                foreach (var countryCode in unmappedCodes)
                {
                    var name = countryCode.GetCountryName();
                    var enumName = SuggestCountryEnumName(name);
                    code.AppendLine($"{{ CountryCode.{countryCode}, Country.{enumName} }},");
                }
                code.AppendLine();

                code.AppendLine("// Add to MapDictionaries.CountryToName:");
                foreach (var countryCode in unmappedCodes)
                {
                    var name = countryCode.GetCountryName();
                    var enumName = SuggestCountryEnumName(name);
                    code.AppendLine($"{{ Country.{enumName}, \"{name}\" }},");
                }
                code.AppendLine();

                code.AppendLine("// Add to MapDictionaries.CountryToContinent:");
                foreach (var countryCode in unmappedCodes)
                {
                    var name = countryCode.GetCountryName();
                    var enumName = SuggestCountryEnumName(name);
                    var continent = GuessContinent(countryCode, name);
                    code.AppendLine($"{{ Country.{enumName}, Continent.{continent} }}, // {name}");
                }
            }

            return code.ToString();
        }

        private static string GuessContinent(CountryCode code, string name)
        {
            // Simple continent guessing based on country code patterns and names
            var codeStr = code.ToString();
            
            // European country codes and territories
            if (new[] { "AD", "AL", "AM", "AT", "AX", "AZ", "BA", "BE", "BG", "BY", "CH", "CZ", "DE", "DK", 
                       "EE", "ES", "FI", "FO", "FR", "GB", "GE", "GG", "GI", "GR", "HR", "HU", "IE", "IM", 
                       "IS", "IT", "JE", "LI", "LT", "LU", "LV", "MC", "MD", "ME", "MK", "MT", "NL", "NO", 
                       "PL", "PT", "RO", "RS", "RU", "SE", "SI", "SJ", "SK", "SM", "UA", "VA", "XK" }.Contains(codeStr))
                return "Europe";

            // African country codes
            if (new[] { "AO", "BF", "BI", "BJ", "BW", "CD", "CF", "CG", "CI", "CM", "CV", "DJ", "DZ", "EG", 
                       "EH", "ER", "ET", "GA", "GH", "GM", "GN", "GQ", "GW", "KE", "KM", "LR", "LS", "LY", 
                       "MA", "MG", "ML", "MR", "MU", "MW", "MZ", "NA", "NE", "NG", "RE", "RW", "SC", "SD", 
                       "SH", "SL", "SN", "SO", "SS", "ST", "SZ", "TD", "TG", "TN", "TZ", "UG", "YT", "ZA", 
                       "ZM", "ZW" }.Contains(codeStr))
                return "Africa";

            // Asian country codes
            if (new[] { "AE", "AF", "BD", "BH", "BN", "BT", "CC", "CN", "CX", "HK", "ID", "IL", "IN", "IO", 
                       "IQ", "IR", "JO", "JP", "KG", "KH", "KP", "KR", "KW", "KZ", "LA", "LB", "LK", "MM", 
                       "MN", "MO", "MV", "MY", "NP", "OM", "PH", "PK", "PS", "QA", "SA", "SG", "SY", "TH", 
                       "TJ", "TL", "TM", "TW", "UZ", "VN", "YE" }.Contains(codeStr))
                return "Asia";

            // North American country codes
            if (new[] { "AG", "AI", "AW", "BB", "BL", "BM", "BQ", "BS", "BZ", "CA", "CR", "CU", "CW", "DM", 
                       "DO", "GD", "GL", "GP", "GT", "HN", "HT", "JM", "KN", "KY", "LC", "MF", "MQ", "MS", 
                       "MX", "NI", "PA", "PM", "PR", "SV", "SX", "TC", "TT", "US", "VC", "VG", "VI" }.Contains(codeStr))
                return "NorthAmerica";

            // South American country codes
            if (new[] { "AR", "BO", "BR", "CL", "CO", "EC", "FK", "GF", "GS", "GY", "PE", "PY", "SR", "UY", "VE" }.Contains(codeStr))
                return "SouthAmerica";

            // Oceanian country codes
            if (new[] { "AS", "AU", "CK", "FJ", "FM", "GU", "KI", "MH", "MP", "NC", "NF", "NR", "NU", "NZ", 
                       "PF", "PG", "PN", "PW", "SB", "TK", "TO", "TV", "UM", "VU", "WF", "WS" }.Contains(codeStr))
                return "Oceania";

            // Antarctic territories
            if (new[] { "AQ", "BV", "HM", "TF" }.Contains(codeStr))
                return "Antarctica";

            return "Unknown";
        }

        public static void RunDebugDiagnostics()
        {
            var report = GenerateComprehensiveReport();
            System.Diagnostics.Debug.WriteLine(report);
            
            // Also log to console if available
            Console.WriteLine(report);
        }
    }

    public class CountryCodeAnalysis
    {
        public CountryCode CountryCode { get; set; }
        public string CountryName { get; set; }
        public Country MappedCountry { get; set; }
        public bool HasCountryMapping { get; set; }
        public bool HasReverseMapping { get; set; }
        public bool HasNameMapping { get; set; }
        public bool HasMapDictionaryEntry { get; set; }
        public bool IsCompletelyMapped { get; set; }
    }
}