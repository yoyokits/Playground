// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Extensions;
using WorldMapControls.Models.Enums;

namespace WorldMapTest.Extensions
{
    [TestClass]
    public class CountryCodeExtensionsTests
    {
        [TestMethod]
        public void GetAllCountryCodes_ShouldExcludeUnknown()
        {
            // Act
            var countryCodes = CountryCodeExtensions.GetAllCountryCodes();

            // Assert
            countryCodes.Should().NotContain(CountryCode.Unknown);
            var codeStrings = countryCodes.Select(c => c.ToString()).ToArray();
            codeStrings.Should().BeInAscendingOrder();
            countryCodes.Length.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void GetAllCountryCodes_CountShouldMatchActualWorldCountryCount()
        {
            // According to UN, there are 195 countries (193 UN members + 2 observer states)
            // However, CountryCode enum includes territories and dependencies, so count should be higher
            // Act
            var countryCodes = CountryCodeExtensions.GetAllCountryCodes();
            
            // Assert - Should have all ISO 3166-1 alpha-2 codes (around 249)
            countryCodes.Length.Should().BeInRange(240, 260, "ISO 3166-1 alpha-2 includes countries and territories");
        }

        [TestMethod]
        public void GetCountryCode_WithValidCountryName_ShouldReturnCorrectCode()
        {
            // Arrange & Act & Assert
            CountryCodeExtensions.GetCountryCode("United States").Should().Be(CountryCode.US);
            CountryCodeExtensions.GetCountryCode("Germany").Should().Be(CountryCode.DE);
            CountryCodeExtensions.GetCountryCode("Japan").Should().Be(CountryCode.JP);
            CountryCodeExtensions.GetCountryCode("Brazil").Should().Be(CountryCode.BR);
        }

        [TestMethod]
        public void GetCountryCode_WithInvalidCountryName_ShouldReturnUnknown()
        {
            // Act & Assert
            CountryCodeExtensions.GetCountryCode("Atlantis").Should().Be(CountryCode.Unknown);
            CountryCodeExtensions.GetCountryCode("").Should().Be(CountryCode.Unknown);
            CountryCodeExtensions.GetCountryCode(null!).Should().Be(CountryCode.Unknown);
        }

        [TestMethod]
        public void GetCountryCode_ShouldBeCaseInsensitive()
        {
            // Act & Assert
            CountryCodeExtensions.GetCountryCode("UNITED STATES").Should().Be(CountryCode.US);
            CountryCodeExtensions.GetCountryCode("united states").Should().Be(CountryCode.US);
            CountryCodeExtensions.GetCountryCode("UnItEd StAtEs").Should().Be(CountryCode.US);
        }

        [TestMethod]
        public void GetCountryName_WithValidCountryCode_ShouldReturnCorrectName()
        {
            // Act & Assert
            CountryCode.US.GetCountryName().Should().Be("United States");
            CountryCode.DE.GetCountryName().Should().Be("Germany");
            CountryCode.JP.GetCountryName().Should().Be("Japan");
            CountryCode.BR.GetCountryName().Should().Be("Brazil");
        }

        [TestMethod]
        public void GetCountryName_WithUnknownCountryCode_ShouldReturnCodeAsString()
        {
            // Act & Assert
            CountryCode.Unknown.GetCountryName().Should().Be("Unknown");
        }

        [TestMethod]
        public void ToCode_WithValidCountryCode_ShouldReturnTwoLetterCode()
        {
            // Act & Assert
            CountryCode.US.ToCode().Should().Be("US");
            CountryCode.DE.ToCode().Should().Be("DE");
            CountryCode.JP.ToCode().Should().Be("JP");
            CountryCode.BR.ToCode().Should().Be("BR");
        }

        [TestMethod]
        public void ToCode_WithUnknownCountryCode_ShouldReturnEmptyString()
        {
            // Act & Assert
            CountryCode.Unknown.ToCode().Should().Be("");
        }

        [TestMethod]
        public void ToCountry_WithValidCountryCode_ShouldReturnCorrectCountry()
        {
            // Act & Assert
            CountryCode.US.ToCountry().Should().Be(Country.UnitedStates);
            CountryCode.DE.ToCountry().Should().Be(Country.Germany);
            CountryCode.JP.ToCountry().Should().Be(Country.Japan);
            CountryCode.BR.ToCountry().Should().Be(Country.Brazil);
            CountryCode.GB.ToCountry().Should().Be(Country.UnitedKingdom);
        }

        [TestMethod]
        public void ToCountryCode_WithValidCountry_ShouldReturnCorrectCode()
        {
            // Act & Assert
            Country.UnitedStates.ToCountryCode().Should().Be(CountryCode.US);
            Country.Germany.ToCountryCode().Should().Be(CountryCode.DE);
            Country.Japan.ToCountryCode().Should().Be(CountryCode.JP);
            Country.Brazil.ToCountryCode().Should().Be(CountryCode.BR);
            Country.UnitedKingdom.ToCountryCode().Should().Be(CountryCode.GB);
        }

        [TestMethod]
        public void BidirectionalMapping_AllCountryCodes_ShouldHaveConsistentMapping()
        {
            // Arrange
            var allCodes = CountryCodeExtensions.GetAllCountryCodes();
            var inconsistentMappings = new List<(CountryCode Code, Country MappedCountry, CountryCode BackMappedCode)>();

            // Act
            foreach (var code in allCodes)
            {
                var country = code.ToCountry();
                if (country != Country.Unknown)
                {
                    var backMappedCode = country.ToCountryCode();
                    if (backMappedCode != code)
                    {
                        inconsistentMappings.Add((code, country, backMappedCode));
                    }
                }
            }

            // Assert
            inconsistentMappings.Should().BeEmpty(
                $"All country codes should have bidirectional consistent mapping. " +
                $"Inconsistent mappings: {string.Join(", ", inconsistentMappings.Select(x => $"{x.Code} -> {x.MappedCountry} -> {x.BackMappedCode}"))}");
        }

        [TestMethod]
        public void AllCountryCodes_ShouldHaveCountryMapping()
        {
            // Arrange
            var allCodes = CountryCodeExtensions.GetAllCountryCodes();
            var unmappedCodes = new List<CountryCode>();

            // Act
            foreach (var code in allCodes)
            {
                var country = code.ToCountry();
                if (country == Country.Unknown)
                {
                    unmappedCodes.Add(code);
                }
            }

            // Assert
            unmappedCodes.Should().BeEmpty(
                $"All country codes should map to a Country enum value. " +
                $"Unmapped codes: {string.Join(", ", unmappedCodes)}");
        }

        [TestMethod]
        public void CountryCodeEnum_ShouldContainAllISOCountryCodes()
        {
            // Arrange - Major world countries that must be included
            var majorCountries = new[]
            {
                CountryCode.US, CountryCode.CA, CountryCode.MX, // North America
                CountryCode.BR, CountryCode.AR, CountryCode.CL, // South America  
                CountryCode.GB, CountryCode.FR, CountryCode.DE, CountryCode.IT, CountryCode.ES, CountryCode.RU, // Europe
                CountryCode.CN, CountryCode.JP, CountryCode.IN, CountryCode.KR, CountryCode.ID, // Asia
                CountryCode.SA, CountryCode.IR, CountryCode.TR, CountryCode.IL, // Middle East
                CountryCode.EG, CountryCode.NG, CountryCode.ZA, CountryCode.KE, CountryCode.ET, // Africa
                CountryCode.AU, CountryCode.NZ, CountryCode.FJ // Oceania
            };

            var allCodes = CountryCodeExtensions.GetAllCountryCodes();

            // Act & Assert
            foreach (var majorCountry in majorCountries)
            {
                allCodes.Should().Contain(majorCountry, $"Major country {majorCountry} should be in CountryCode enum");
            }
        }

        [TestMethod]
        public void CountryNameMapping_AllCountryCodes_ShouldHaveNameMapping()
        {
            // Arrange
            var allCodes = CountryCodeExtensions.GetAllCountryCodes();
            var codesWithoutNames = new List<CountryCode>();

            // Act
            foreach (var code in allCodes)
            {
                var name = code.GetCountryName();
                if (string.IsNullOrEmpty(name) || name == code.ToString())
                {
                    codesWithoutNames.Add(code);
                }
            }

            // Assert
            codesWithoutNames.Should().BeEmpty(
                $"All country codes should have proper name mapping. " +
                $"Codes without names: {string.Join(", ", codesWithoutNames)}");
        }

        [TestMethod]
        public void CongoMappings_ShouldBeCorrectlyMapped()
        {
            // The two Congo countries are often confused - verify correct mapping
            
            // Act & Assert
            CountryCode.CD.ToCountry().Should().Be(Country.DemocraticRepublicOfCongo, 
                "CD should map to Democratic Republic of Congo");
            CountryCode.CG.ToCountry().Should().Be(Country.RepublicOfCongo, 
                "CG should map to Republic of Congo");

            Country.DemocraticRepublicOfCongo.ToCountryCode().Should().Be(CountryCode.CD,
                "Democratic Republic of Congo should map back to CD");
            Country.RepublicOfCongo.ToCountryCode().Should().Be(CountryCode.CG,
                "Republic of Congo should map back to CG");
        }

        [TestMethod]
        public void KoreaMappings_ShouldBeCorrectlyMapped()
        {
            // Verify North and South Korea are correctly mapped
            
            // Act & Assert
            CountryCode.KR.ToCountry().Should().Be(Country.SouthKorea, "KR should map to South Korea");
            CountryCode.KP.ToCountry().Should().Be(Country.NorthKorea, "KP should map to North Korea");

            Country.SouthKorea.ToCountryCode().Should().Be(CountryCode.KR, "South Korea should map to KR");
            Country.NorthKorea.ToCountryCode().Should().Be(CountryCode.KP, "North Korea should map to KP");
        }

        [TestMethod]
        public void CommonAliasCountries_ShouldHaveCorrectMapping()
        {
            // Test commonly aliased countries
            var testCases = new[]
            {
                (CountryCode.US, Country.UnitedStates, "United States"),
                (CountryCode.GB, Country.UnitedKingdom, "United Kingdom"),
                (CountryCode.AE, Country.UAE, "United Arab Emirates"),
                (CountryCode.CZ, Country.CzechRepublic, "Czech Republic"),
                (CountryCode.MM, Country.Myanmar, "Myanmar"),
                (CountryCode.LK, Country.SriLanka, "Sri Lanka"),
                (CountryCode.CI, Country.IvoryCoast, "Côte d'Ivoire")
            };

            foreach (var (code, expectedCountry, expectedName) in testCases)
            {
                code.ToCountry().Should().Be(expectedCountry, $"{code} should map to {expectedCountry}");
                expectedCountry.ToCountryCode().Should().Be(code, $"{expectedCountry} should map to {code}");
                code.GetCountryName().Should().Be(expectedName, $"{code} should have name {expectedName}");
            }
        }
    }
}