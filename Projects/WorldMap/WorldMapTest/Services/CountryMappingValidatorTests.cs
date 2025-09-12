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
    public class CountryMappingValidatorTests
    {
        [TestMethod]
        public void ValidateAllMappings_ShouldReturnValidResults()
        {
            // Act
            var result = CountryMappingValidator.ValidateAllMappings();

            // Assert
            result.Should().NotBeNull("Validation result should not be null");
            result.CountryCodesWithoutCountryMapping.Should().NotBeNull();
            result.CountriesWithoutCountryCodeMapping.Should().NotBeNull();
            result.CountriesNotInMapDictionaries.Should().NotBeNull();
            result.CountriesNotInContinentMap.Should().NotBeNull();
            result.BidirectionalMappingIssues.Should().NotBeNull();
        }

        [TestMethod]
        public void ValidateAllMappings_AllCountryCodesShouldHaveCountryMapping()
        {
            // Act
            var result = CountryMappingValidator.ValidateAllMappings();

            // Assert
            result.CountryCodesWithoutCountryMapping.Should().BeEmpty(
                $"All CountryCode enum values should map to Country enum values. Unmapped: {string.Join(", ", result.CountryCodesWithoutCountryMapping)}");
        }

        [TestMethod]
        public void ValidateAllMappings_AllCountriesShouldHaveCountryCodeMapping()
        {
            // Act  
            var result = CountryMappingValidator.ValidateAllMappings();

            // Assert
            result.CountriesWithoutCountryCodeMapping.Should().BeEmpty(
                $"All Country enum values should map to CountryCode enum values. Unmapped: {string.Join(", ", result.CountriesWithoutCountryCodeMapping)}");
        }

        [TestMethod]
        public void ValidateAllMappings_AllCountriesShouldBeInMapDictionaries()
        {
            // Act
            var result = CountryMappingValidator.ValidateAllMappings();

            // Assert
            result.CountriesNotInMapDictionaries.Should().BeEmpty(
                $"All Country enum values should be in MapDictionaries.CountryToName. Missing: {string.Join(", ", result.CountriesNotInMapDictionaries)}");
        }

        [TestMethod]
        public void ValidateAllMappings_AllCountriesShouldBeInContinentMap()
        {
            // Act
            var result = CountryMappingValidator.ValidateAllMappings();

            // Assert
            result.CountriesNotInContinentMap.Should().BeEmpty(
                $"All Country enum values should be in MapDictionaries.CountryToContinent. Missing: {string.Join(", ", result.CountriesNotInContinentMap)}");
        }

        [TestMethod]
        public void ValidateAllMappings_ShouldNotHaveBidirectionalMappingIssues()
        {
            // Act
            var result = CountryMappingValidator.ValidateAllMappings();

            // Assert
            result.BidirectionalMappingIssues.Should().BeEmpty(
                $"There should be no bidirectional mapping inconsistencies. Issues: {string.Join("; ", result.BidirectionalMappingIssues)}");
        }

        [TestMethod]
        public void ValidateAllMappings_CountryCodeToCountryMappingsShouldBeConsistent()
        {
            // Arrange
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            var inconsistentMappings = new List<string>();

            // Act
            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                if (country != Country.Unknown)
                {
                    var backMappedCode = country.ToCountryCode();
                    if (backMappedCode != code)
                    {
                        inconsistentMappings.Add($"{code} -> {country} -> {backMappedCode}");
                    }
                }
            }

            // Assert
            inconsistentMappings.Should().BeEmpty(
                $"CountryCode -> Country -> CountryCode mappings should be consistent. Inconsistent: {string.Join(", ", inconsistentMappings)}");
        }

        [TestMethod]
        public void ValidateAllMappings_CountryToCountryCodeMappingsShouldBeConsistent()
        {
            // Arrange
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown);
            var inconsistentMappings = new List<string>();

            // Act
            foreach (var country in allCountries)
            {
                var code = country.ToCountryCode();
                if (code != CountryCode.Unknown)
                {
                    var backMappedCountry = code.ToCountry();
                    if (backMappedCountry != country)
                    {
                        inconsistentMappings.Add($"{country} -> {code} -> {backMappedCountry}");
                    }
                }
            }

            // Assert
            inconsistentMappings.Should().BeEmpty(
                $"Country -> CountryCode -> Country mappings should be consistent. Inconsistent: {string.Join(", ", inconsistentMappings)}");
        }

        [TestMethod]
        public void ValidateAllMappings_ShouldValidateSpecificProblematicCountries()
        {
            // Test specific countries that are often problematic
            var problematicCountries = new[]
            {
                (CountryCode.CD, Country.DemocraticRepublicOfCongo, "Democratic Republic of Congo"),
                (CountryCode.CG, Country.RepublicOfCongo, "Republic of Congo"),  
                (CountryCode.CF, Country.CentralAfricanRepublic, "Central African Republic"),
                (CountryCode.SS, Country.SouthSudan, "South Sudan"),
                (CountryCode.SD, Country.Sudan, "Sudan"),
                (CountryCode.KR, Country.SouthKorea, "South Korea"),
                (CountryCode.KP, Country.NorthKorea, "North Korea"),
                (CountryCode.GB, Country.UnitedKingdom, "United Kingdom"),
                (CountryCode.US, Country.UnitedStates, "United States"),
                (CountryCode.AE, Country.UAE, "United Arab Emirates"),
                (CountryCode.MM, Country.Myanmar, "Myanmar"),
                (CountryCode.LK, Country.SriLanka, "Sri Lanka"),
                (CountryCode.TL, Country.TimorLeste, "Timor-Leste"),
                (CountryCode.MK, Country.NorthMacedonia, "North Macedonia"),
                (CountryCode.BA, Country.BosniaAndHerzegovina, "Bosnia and Herzegovina"),
                (CountryCode.VA, Country.VaticanCity, "Vatican City"),
                (CountryCode.PS, Country.Palestine, "Palestine"),
                (CountryCode.TW, Country.Taiwan, "Taiwan")
            };

            foreach (var (code, expectedCountry, description) in problematicCountries)
            {
                // Test CountryCode -> Country mapping
                var mappedCountry = code.ToCountry();
                mappedCountry.Should().Be(expectedCountry, 
                    $"{code} should map to {expectedCountry} ({description})");

                // Test Country -> CountryCode mapping
                var mappedCode = expectedCountry.ToCountryCode();
                mappedCode.Should().Be(code, 
                    $"{expectedCountry} should map back to {code} ({description})");
            }
        }

        [TestMethod]
        public void ValidateAllMappings_ShouldHaveComprehensiveCoverage()
        {
            // Act
            var result = CountryMappingValidator.ValidateAllMappings();

            // Count total mapped entities
            var totalCountryCodes = Enum.GetValues<CountryCode>().Length - 1; // Exclude Unknown
            var totalCountries = Enum.GetValues<Country>().Length - 1; // Exclude Unknown

            var mappedCountryCodes = totalCountryCodes - result.CountryCodesWithoutCountryMapping.Count;
            var mappedCountries = totalCountries - result.CountriesWithoutCountryCodeMapping.Count;

            // Assert good coverage
            var countryCodeCoveragePercent = (double)mappedCountryCodes / totalCountryCodes * 100;
            var countryCoveragePercent = (double)mappedCountries / totalCountries * 100;

            countryCodeCoveragePercent.Should().BeGreaterThan(95, 
                "At least 95% of CountryCode values should be mapped");
            countryCoveragePercent.Should().BeGreaterThan(95, 
                "At least 95% of Country values should be mapped");

            // Log coverage stats for monitoring
            Console.WriteLine($"CountryCode coverage: {mappedCountryCodes}/{totalCountryCodes} ({countryCodeCoveragePercent:F1}%)");
            Console.WriteLine($"Country coverage: {mappedCountries}/{totalCountries} ({countryCoveragePercent:F1}%)");
        }

        [TestMethod]
        public void ValidateAllMappings_ValidationResultShouldBeComplete()
        {
            // Act
            var result = CountryMappingValidator.ValidateAllMappings();

            // Assert all validation aspects are covered
            result.CountryCodesWithoutCountryMapping.Should().NotBeNull();
            result.CountriesWithoutCountryCodeMapping.Should().NotBeNull();
            result.CountriesNotInMapDictionaries.Should().NotBeNull();
            result.CountriesNotInContinentMap.Should().NotBeNull();
            result.BidirectionalMappingIssues.Should().NotBeNull();

            // Test the result can be used for comprehensive reporting
            var totalIssues = result.CountryCodesWithoutCountryMapping.Count +
                            result.CountriesWithoutCountryCodeMapping.Count +
                            result.CountriesNotInMapDictionaries.Count +
                            result.CountriesNotInContinentMap.Count +
                            result.BidirectionalMappingIssues.Count;

            // For a well-configured system, total issues should be 0
            totalIssues.Should().Be(0, 
                "A properly configured mapping system should have no validation issues");

            Console.WriteLine($"Total mapping issues found: {totalIssues}");
            if (totalIssues > 0)
            {
                Console.WriteLine($"CountryCode without Country mapping: {result.CountryCodesWithoutCountryMapping.Count}");
                Console.WriteLine($"Country without CountryCode mapping: {result.CountriesWithoutCountryCodeMapping.Count}");
                Console.WriteLine($"Countries not in MapDictionaries: {result.CountriesNotInMapDictionaries.Count}");
                Console.WriteLine($"Countries not in Continent map: {result.CountriesNotInContinentMap.Count}");
                Console.WriteLine($"Bidirectional mapping issues: {result.BidirectionalMappingIssues.Count}");
            }
        }
    }
}