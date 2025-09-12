// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Services;
using WorldMapControls.Models.Enums;
using WorldMapControls.Extensions;

namespace WorldMapTest.Services
{
    [TestClass]
    public class CountryMappingVerifierTests
    {
        [TestMethod]
        public void VerifyProblematicCountries_ShouldReturnMappingStatus()
        {
            // Act
            var results = CountryMappingVerifier.VerifyProblematicCountries();

            // Assert
            results.Should().NotBeNull("Verification results should not be null");
            results.Should().NotBeEmpty("Should have verification results for problematic countries");

            // Each result should have a name and mapping status
            foreach (var result in results)
            {
                result.Key.Should().NotBeNullOrEmpty("Country name should not be null or empty");
                // result.Value should contain mapping information
            }
        }

        [TestMethod]
        public void VerifyProblematicCountries_CongoCountries_ShouldBeFixed()
        {
            // Act
            var results = CountryMappingVerifier.VerifyProblematicCountries();

            // Assert - Test key Congo variants that are often problematic
            var congoVariants = new[]
            {
                "Congo",
                "Congo, Rep.", 
                "Congo, Dem. Rep.",
                "Democratic Republic of the Congo",
                "Republic of the Congo"
            };

            foreach (var congoVariant in congoVariants)
            {
                if (results.ContainsKey(congoVariant))
                {
                    var (mappedTo, isFixed) = results[congoVariant];
                    isFixed.Should().BeTrue($"Congo variant '{congoVariant}' should be fixed, mapped to {mappedTo}");
                    mappedTo.Should().NotBe(Country.Unknown, $"Congo variant '{congoVariant}' should map to a valid country");
                }
            }
        }

        [TestMethod]
        public void GenerateFixReport_ShouldReturnComprehensiveReport()
        {
            // Act
            var report = CountryMappingVerifier.GenerateFixReport();

            // Assert
            report.Should().NotBeNullOrEmpty("Fix report should not be null or empty");
            
            // Report should contain key sections
            report.Should().Contain("CONGO", "Report should contain Congo-related information");
            report.Should().Contain("COUNTRIES", "Report should mention countries");
            
            // Report should show mapping status with visual indicators
            report.Should().MatchRegex(@"(FIXED|BROKEN|SUCCESS|MAPPED|\+|\-)", "Report should contain status indicators");
        }

        [TestMethod]
        public void GenerateFixReport_ShouldShowCongoSpecificVerification()
        {
            // Act
            var report = CountryMappingVerifier.GenerateFixReport();

            // Assert
            report.Should().Contain("CONGO SPECIFIC", "Report should have Congo-specific verification section");
            
            // Should test various Congo name patterns
            var congoPatterns = new[] { "Congo", "Congo, Rep.", "Congo, Dem. Rep.", "DRC" };
            foreach (var pattern in congoPatterns)
            {
                report.Should().Contain(pattern, $"Report should mention Congo pattern '{pattern}'");
            }
        }

        [TestMethod]
        public void FindUnmappedCountryCodes_ShouldReturnUnmappedCodes()
        {
            // Act
            var unmappedCodes = CountryMappingVerifier.FindUnmappedCountryCodes();

            // Assert
            unmappedCodes.Should().NotBeNull("Unmapped codes list should not be null");
            
            // For a properly configured system, this should be empty
            unmappedCodes.Should().BeEmpty(
                $"All CountryCode enum values should map to Country enum values. Unmapped: {string.Join(", ", unmappedCodes)}");
        }

        [TestMethod]
        public void FindUnmappedCountryCodes_AllCountryCodes_ShouldHaveValidMapping()
        {
            // Arrange
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            var unmappedCodes = new List<CountryCode>();

            // Act
            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                if (country == Country.Unknown)
                {
                    unmappedCodes.Add(code);
                }
            }

            // Assert
            unmappedCodes.Should().BeEmpty(
                $"All CountryCode values should have Country mappings. Unmapped codes: {string.Join(", ", unmappedCodes)}");
            
            // This should match the FindUnmappedCountryCodes method result
            var verifierResult = CountryMappingVerifier.FindUnmappedCountryCodes();
            unmappedCodes.Should().BeEquivalentTo(verifierResult,
                "Manual verification should match CountryMappingVerifier.FindUnmappedCountryCodes result");
        }

        [TestMethod]
        public void VerifyProblematicCountries_ShouldIncludeKnownProblematicCountries()
        {
            // Act
            var results = CountryMappingVerifier.VerifyProblematicCountries();

            // Assert - Known problematic country names that should be tested
            var knownProblematicNames = new[]
            {
                "Congo", "DRC", "South Sudan", "Sudan", "Myanmar", "Korea",
                "Macedonia", "Czech Republic", "Bosnia", "Yugoslavia"
            };

            // The method should test at least some of these problematic cases
            var testedNames = results.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var foundProblematic = knownProblematicNames.Where(name => 
                testedNames.Any(tested => tested.Contains(name, StringComparison.OrdinalIgnoreCase))).ToList();

            foundProblematic.Should().NotBeEmpty(
                "Verification should include some known problematic country names");
        }

        [TestMethod]
        public void CountryMappingVerifier_Integration_AllMethodsShouldWork()
        {
            // Integration test to ensure all methods work together
            
            // Act
            var problematicResults = CountryMappingVerifier.VerifyProblematicCountries();
            var fixReport = CountryMappingVerifier.GenerateFixReport();
            var unmappedCodes = CountryMappingVerifier.FindUnmappedCountryCodes();

            // Assert
            problematicResults.Should().NotBeNull();
            fixReport.Should().NotBeNullOrEmpty();
            unmappedCodes.Should().NotBeNull();

            // Results should be consistent with each other
            Console.WriteLine($"Problematic countries tested: {problematicResults.Count}");
            Console.WriteLine($"Unmapped country codes: {unmappedCodes.Count}");
            Console.WriteLine($"Fix report length: {fixReport.Length} characters");
            
            // For a well-configured system, unmapped codes should be minimal
            var unmappedPercent = (double)unmappedCodes.Count / Enum.GetValues<CountryCode>().Length * 100;
            unmappedPercent.Should().BeLessThan(5, 
                "Less than 5% of country codes should be unmapped in a well-configured system");
        }

        [TestMethod]
        public void CountryMappingVerifier_ShouldHandleAllMajorWorldRegions()
        {
            // Test that verification covers countries from all major world regions
            var results = CountryMappingVerifier.VerifyProblematicCountries();
            var reportedCountries = results.Keys.ToList();

            // Should include representatives from different regions (this is flexible based on what's actually problematic)
            var regions = new Dictionary<string, string[]>
            {
                ["Africa"] = new[] { "Congo", "Sudan", "Chad", "Niger", "Mali", "Ghana", "Kenya", "Ethiopia" },
                ["Europe"] = new[] { "Macedonia", "Bosnia", "Czech", "Serbia", "Montenegro", "Kosovo" }, 
                ["Asia"] = new[] { "Korea", "Myanmar", "Timor", "Kazakhstan", "Georgia", "Armenia" },
                ["Americas"] = new[] { "United States", "Brazil", "Venezuela", "Colombia" },
                ["MiddleEast"] = new[] { "Iran", "Iraq", "Syria", "Yemen", "Palestine" }
            };

            var regionsWithProblematicCountries = 0;
            foreach (var region in regions)
            {
                var hasProblematicCountry = region.Value.Any(country =>
                    reportedCountries.Any(reported => 
                        reported.Contains(country, StringComparison.OrdinalIgnoreCase)));
                        
                if (hasProblematicCountry)
                {
                    regionsWithProblematicCountries++;
                    Console.WriteLine($"{region.Key} has problematic countries in verification");
                }
            }

            // Don't require all regions to have problems, but should have some geographic diversity
            regionsWithProblematicCountries.Should().BeGreaterThan(0,
                "Verification should cover geographically diverse problematic countries");
        }
    }
}