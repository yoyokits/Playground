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
    /// Final verification tool to confirm all CountryCode mappings are complete
    /// </summary>
    public static class FinalMappingVerification
    {
        public static string GenerateFinalReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== FINAL COUNTRYCODE MAPPING VERIFICATION ===");
            report.AppendLine($"Verification Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            // Get all CountryCode enum values
            var allCountryCodes = Enum.GetValues<CountryCode>()
                .Where(c => c != CountryCode.Unknown)
                .OrderBy(c => c.ToString())
                .ToArray();

            report.AppendLine($"Total CountryCodes in enum: {allCountryCodes.Length}");

            // Test each mapping
            var unmapped = 0;
            var mappedCorrectly = 0;
            var bidirectionalErrors = 0;

            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                
                if (country == Country.Unknown)
                {
                    unmapped++;
                    report.AppendLine($"? {code} -> UNMAPPED");
                }
                else
                {
                    mappedCorrectly++;
                    
                    // Test bidirectional mapping
                    var reverseCode = country.ToCountryCode();
                    if (reverseCode != code)
                    {
                        bidirectionalErrors++;
                        report.AppendLine($"?? {code} -> {country} -> {reverseCode} (bidirectional error)");
                    }
                }
            }

            report.AppendLine();
            report.AppendLine("=== FINAL STATISTICS ===");
            report.AppendLine($"? Mapped correctly: {mappedCorrectly}/{allCountryCodes.Length} ({(mappedCorrectly * 100.0 / allCountryCodes.Length):F1}%)");
            report.AppendLine($"? Still unmapped: {unmapped}");
            report.AppendLine($"?? Bidirectional errors: {bidirectionalErrors}");

            if (unmapped == 0 && bidirectionalErrors == 0)
            {
                report.AppendLine();
                report.AppendLine("?? SUCCESS: ALL COUNTRYCODES ARE PROPERLY MAPPED WITH COMPLETE BIDIRECTIONAL SUPPORT!");
                report.AppendLine("? World map should now render correctly with no unmapped countries");
                report.AppendLine("? Color mapping should cover all countries");
                report.AppendLine("? Debug output should show no more unmapped country messages");
            }
            else
            {
                report.AppendLine();
                report.AppendLine("?? WARNING: Some mappings still need attention");
            }

            return report.ToString();
        }

        public static void RunFinalVerification()
        {
            var report = GenerateFinalReport();
            System.Diagnostics.Debug.WriteLine(report);
            Console.WriteLine(report);
        }
    }
}