// Quick console test for country mappings
using System;
using System.Linq;
using WorldMapControls.Extensions;
using WorldMapControls.Models.Enums;
using WorldMapControls.Services;

// Test all CountryCode mappings
Console.WriteLine("=== TESTING ALL COUNTRYCODE MAPPINGS ===");

var allCodes = Enum.GetValues<CountryCode>()
    .Where(c => c != CountryCode.Unknown)
    .OrderBy(c => c.ToString())
    .ToArray();

Console.WriteLine($"Total CountryCodes to test: {allCodes.Length}");

var unmapped = new List<CountryCode>();
var successful = 0;

foreach (var code in allCodes)
{
    var country = code.ToCountry();
    if (country == Country.Unknown)
    {
        unmapped.Add(code);
        Console.WriteLine($"? {code} ({code.GetCountryName()}) -> UNMAPPED");
    }
    else
    {
        successful++;
        // Verify reverse mapping
        var reverseCode = country.ToCountryCode();
        if (reverseCode != code)
        {
            Console.WriteLine($"?? {code} -> {country} -> {reverseCode} (reverse mapping mismatch)");
        }
    }
}

Console.WriteLine();
Console.WriteLine($"? Successfully mapped: {successful}/{allCodes.Length} ({(successful * 100.0 / allCodes.Length):F1}%)");
Console.WriteLine($"? Still unmapped: {unmapped.Count}");

if (unmapped.Any())
{
    Console.WriteLine("\nUNMAPPED CODES:");
    foreach (var code in unmapped)
    {
        Console.WriteLine($"  {code} - {code.GetCountryName()}");
    }
}
else
{
    Console.WriteLine("\n?? ALL COUNTRYCODES ARE NOW PROPERLY MAPPED!");
}

// Run the comprehensive investigator
Console.WriteLine("\n" + CountryCodeMappingInvestigator.GenerateComprehensiveReport());