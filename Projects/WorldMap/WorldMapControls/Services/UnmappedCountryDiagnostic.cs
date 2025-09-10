// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Linq;
using WorldMapControls.Extensions;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Quick diagnostic tool to identify unmapped countries from debug output.
    /// </summary>
    public static class UnmappedCountryDiagnostic
    {
        public static void PrintAllUnmappedCountries()
        {
            Console.WriteLine("=== CHECKING ALL COUNTRY CODE TO COUNTRY MAPPINGS ===");
            
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown).OrderBy(c => c.ToString());
            var unmappedCodes = new System.Collections.Generic.List<CountryCode>();
            
            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                if (country == Country.Unknown)
                {
                    unmappedCodes.Add(code);
                    Console.WriteLine($"? {code} ({code.GetCountryName()}) -> NOT MAPPED TO Country ENUM");
                }
            }
            
            Console.WriteLine($"\n=== SUMMARY ===");
            Console.WriteLine($"Total CountryCodes: {allCountryCodes.Count()}");
            Console.WriteLine($"Unmapped CountryCodes: {unmappedCodes.Count}");
            
            if (unmappedCodes.Count > 0)
            {
                Console.WriteLine($"\n=== COUNTRIES THAT NEED TO BE ADDED ===");
                foreach (var code in unmappedCodes)
                {
                    var name = code.GetCountryName();
                    var enumName = ConvertToEnumName(name);
                    Console.WriteLine($"// Add to Country enum: {enumName}, // {name} ({code})");
                    Console.WriteLine($"// Add to CountryToCountryCode: {{ Country.{enumName}, CountryCode.{code} }},");
                    Console.WriteLine($"// Add to CountryCodeToCountry: {{ CountryCode.{code}, Country.{enumName} }},");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("?? ALL COUNTRY CODES ARE MAPPED!");
            }
        }
        
        private static string ConvertToEnumName(string countryName)
        {
            // Convert country name to PascalCase enum name
            var words = countryName.Split(' ', '-', '(', ')', ',', '.')
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim())
                .Where(w => w.Length > 0);
            
            var enumName = string.Join("", words.Select(w => 
                char.ToUpper(w[0]) + w.Substring(1).ToLower()));
                
            // Handle special cases
            enumName = enumName.Replace("'", "")
                              .Replace("â", "a")
                              .Replace("ç", "c")
                              .Replace("é", "e")
                              .Replace("ã", "a")
                              .Replace("õ", "o")
                              .Replace("í", "i")
                              .Replace("ú", "u");
            
            return enumName;
        }
    }
}