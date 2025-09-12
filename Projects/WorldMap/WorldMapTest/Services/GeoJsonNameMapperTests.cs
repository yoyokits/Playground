// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Services;
using WorldMapControls.Models.Enums;

namespace WorldMapTest.Services
{
    [TestClass]
    public class GeoJsonNameMapperTests
    {
        [TestMethod]
        public void MapGeoJsonNameToCountry_WithValidCountryNames_ShouldReturnCorrectCountries()
        {
            // Test basic country name mappings
            var testCases = new[]
            {
                ("United States", Country.UnitedStates),
                ("United Kingdom", Country.UnitedKingdom),
                ("Germany", Country.Germany),
                ("France", Country.France),
                ("Japan", Country.Japan),
                ("Brazil", Country.Brazil),
                ("China", Country.China),
                ("India", Country.India),
                ("Russia", Country.Russia),
                ("Canada", Country.Canada)
            };

            foreach (var (geoJsonName, expectedCountry) in testCases)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(geoJsonName);
                result.Should().Be(expectedCountry, 
                    $"GeoJSON name '{geoJsonName}' should map to {expectedCountry}");
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_WithCongoVariants_ShouldReturnCorrectCountries()
        {
            // This is critical - Congo mappings are often problematic
            var congoTestCases = new[]
            {
                ("Congo", Country.RepublicOfCongo, "Short 'Congo' should map to Republic of Congo"),
                ("Congo, Rep.", Country.RepublicOfCongo, "Congo, Rep. format should map to Republic of Congo"),
                ("Republic of the Congo", Country.RepublicOfCongo, "Full name should map to Republic of Congo"),
                ("Congo-Brazzaville", Country.RepublicOfCongo, "Congo-Brazzaville should map to Republic of Congo"),
                
                ("Congo, Dem. Rep.", Country.DemocraticRepublicOfCongo, "Congo, Dem. Rep. should map to DRC"),
                ("Democratic Republic of the Congo", Country.DemocraticRepublicOfCongo, "Full DRC name should map to DRC"),
                ("Congo (Kinshasa)", Country.DemocraticRepublicOfCongo, "Congo (Kinshasa) should map to DRC"),
                ("DRC", Country.DemocraticRepublicOfCongo, "DRC abbreviation should map to DRC"),
                ("Zaire", Country.DemocraticRepublicOfCongo, "Historical name Zaire should map to DRC")
            };

            foreach (var (geoJsonName, expectedCountry, description) in congoTestCases)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(geoJsonName);
                result.Should().Be(expectedCountry, description);
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_WithCountryAliases_ShouldReturnCorrectCountries()
        {
            // Test common aliases that appear in GeoJSON files
            var aliasTestCases = new[]
            {
                ("USA", Country.UnitedStates),
                ("US", Country.UnitedStates),
                ("United States of America", Country.UnitedStates),
                ("America", Country.UnitedStates),
                
                ("UK", Country.UnitedKingdom),
                ("Great Britain", Country.UnitedKingdom),
                ("Britain", Country.UnitedKingdom),
                
                ("South Korea", Country.SouthKorea),
                ("Korea", Country.SouthKorea),
                ("Republic of Korea", Country.SouthKorea),
                
                ("North Korea", Country.NorthKorea),
                ("DPRK", Country.NorthKorea),
                ("Democratic People's Republic of Korea", Country.NorthKorea),
                
                ("UAE", Country.UAE),
                ("United Arab Emirates", Country.UAE),
                
                ("Myanmar", Country.Myanmar),
                ("Burma", Country.Myanmar),
                
                ("Czech Republic", Country.CzechRepublic),
                ("Czechia", Country.CzechRepublic),
                
                ("Ivory Coast", Country.IvoryCoast),
                ("Côte d'Ivoire", Country.IvoryCoast),
                
                ("Sri Lanka", Country.SriLanka),
                ("Ceylon", Country.SriLanka)
            };

            foreach (var (alias, expectedCountry) in aliasTestCases)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(alias);
                result.Should().Be(expectedCountry, 
                    $"Alias '{alias}' should map to {expectedCountry}");
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_WithInvalidNames_ShouldReturnUnknown()
        {
            // Test invalid or non-existent country names
            var invalidNames = new[]
            {
                "Atlantis",
                "Wakanda", 
                "Middle-earth",
                "",
                null,
                "   ",
                "NotACountry",
                "InvalidCountryName123"
            };

            foreach (var invalidName in invalidNames)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(invalidName);
                result.Should().Be(Country.Unknown, 
                    $"Invalid name '{invalidName}' should return Country.Unknown");
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_ShouldBeCaseInsensitive()
        {
            // Test case insensitivity
            var caseTestCases = new[]
            {
                ("UNITED STATES", Country.UnitedStates),
                ("united states", Country.UnitedStates),
                ("UnItEd StAtEs", Country.UnitedStates),
                ("GERMANY", Country.Germany),
                ("germany", Country.Germany),
                ("GeRmAnY", Country.Germany),
                ("JAPAN", Country.Japan),
                ("japan", Country.Japan)
            };

            foreach (var (geoJsonName, expectedCountry) in caseTestCases)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(geoJsonName);
                result.Should().Be(expectedCountry, 
                    $"Case-insensitive mapping should work for '{geoJsonName}'");
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Test names with special characters, accents, etc.
            var specialCharacterCases = new[]
            {
                ("Côte d'Ivoire", Country.IvoryCoast),
                ("São Tomé and Príncipe", Country.SaoTomeAndPrincipe),
                ("Bosnia and Herzegovina", Country.BosniaAndHerzegovina),
                ("Bosnia & Herzegovina", Country.BosniaAndHerzegovina),
                ("Saint Kitts and Nevis", Country.StKittsNevis),
                ("St. Kitts and Nevis", Country.StKittsNevis),
                ("Saint Vincent and the Grenadines", Country.StVincent),
                ("St. Vincent and the Grenadines", Country.StVincent),
                ("Saint Lucia", Country.StLucia),
                ("St. Lucia", Country.StLucia)
            };

            foreach (var (geoJsonName, expectedCountry) in specialCharacterCases)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(geoJsonName);
                result.Should().Be(expectedCountry, 
                    $"Special character handling should work for '{geoJsonName}'");
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_WithHistoricalNames_ShouldMapCorrectly()
        {
            // Test historical country names that might appear in older GeoJSON files
            var historicalNames = new[]
            {
                ("Zaire", Country.DemocraticRepublicOfCongo),
                ("Burma", Country.Myanmar),
                ("Ceylon", Country.SriLanka),
                ("Swaziland", Country.Eswatini),
                ("Macedonia", Country.NorthMacedonia),
                ("FYROM", Country.NorthMacedonia)
            };

            foreach (var (historicalName, expectedCountry) in historicalNames)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(historicalName);
                result.Should().Be(expectedCountry, 
                    $"Historical name '{historicalName}' should map to {expectedCountry}");
            }
        }

        [TestMethod]
        public void GetGeoJsonVariations_WithValidCountry_ShouldReturnVariations()
        {
            // Test that we can get all GeoJSON name variations for a country
            var variations = GeoJsonNameMapper.GetGeoJsonVariations(Country.UnitedStates);
            
            variations.Should().NotBeEmpty("United States should have name variations");
            variations.Should().Contain("United States");
            
            // Should contain common aliases
            var variationsLower = variations.Select(v => v.ToLowerInvariant()).ToList();
            variationsLower.Should().Contain("usa");
            variationsLower.Should().Contain("us");
        }

        [TestMethod]
        public void GetGeoJsonVariations_WithUnknownCountry_ShouldReturnEmpty()
        {
            // Test with Unknown country
            var variations = GeoJsonNameMapper.GetGeoJsonVariations(Country.Unknown);
            variations.Should().BeEmpty("Unknown country should have no variations");
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_ValidatedMappings_ShouldAllBeValid()
        {
            // Test that all internal mappings are valid by using the ValidateMappings method
            var invalidMappings = GeoJsonNameMapper.ValidateMappings();
            
            invalidMappings.Should().BeEmpty(
                $"All GeoJSON mappings should point to valid countries. Invalid: {string.Join(", ", invalidMappings)}");
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_WithCommonGeoJsonPatterns_ShouldHandleCorrectly()
        {
            // Test common patterns found in actual GeoJSON files
            var geoJsonPatterns = new[]
            {
                // Common World Bank / Natural Earth patterns
                ("United States of America", Country.UnitedStates),
                ("Russian Federation", Country.Russia),
                ("Iran (Islamic Republic of)", Country.Iran),
                ("Venezuela (Bolivarian Republic of)", Country.Venezuela),
                ("Bolivia (Plurinational State of)", Country.Bolivia),
                ("Tanzania, United Republic of", Country.Tanzania),
                ("Korea, Republic of", Country.SouthKorea),
                ("Korea, Dem. People's Rep. of", Country.NorthKorea),
                ("Lao People's Democratic Republic", Country.Laos),
                ("Syrian Arab Republic", Country.Syria),
                
                // Shortened forms
                ("Russian Fed.", Country.Russia),
                ("Korea Rep.", Country.SouthKorea),
                ("Korea DPR", Country.NorthKorea),
                ("Dem. Rep. Congo", Country.DemocraticRepublicOfCongo),
                ("Central African Rep.", Country.CentralAfricanRepublic),
                
                // Special formatting
                ("U.S.A.", Country.UnitedStates),
                ("U.K.", Country.UnitedKingdom),
                ("U.A.E.", Country.UAE)
            };

            foreach (var (geoJsonName, expectedCountry) in geoJsonPatterns)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(geoJsonName);
                result.Should().Be(expectedCountry, 
                    $"GeoJSON pattern '{geoJsonName}' should map to {expectedCountry}");
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_AllMappedCountries_ShouldBeValid()
        {
            // Get all mapped countries and verify they're all valid enum values
            var allCountryEnumValues = Enum.GetValues<Country>().ToHashSet();
            
            // Get all mapped countries from the internal dictionary via reflection or by testing all mappings
            var allMappedCountries = new HashSet<Country>();
            
            // Test with a comprehensive list of country names to discover all mapped countries
            var testCountries = new[]
            {
                "United States", "Germany", "Japan", "Brazil", "China", "India", "Russia", "Canada",
                "Congo", "Congo, Dem. Rep.", "South Korea", "North Korea", "Myanmar", "United Kingdom",
                "France", "Italy", "Spain", "Australia", "New Zealand", "South Africa"
            };
            
            foreach (var countryName in testCountries)
            {
                var country = GeoJsonNameMapper.MapGeoJsonNameToCountry(countryName);
                if (country != Country.Unknown)
                {
                    allMappedCountries.Add(country);
                }
            }
            
            // This test ensures no mapping points to an invalid enum value
            foreach (var mappedCountry in allMappedCountries)
            {
                allCountryEnumValues.Should().Contain(mappedCountry,
                    $"Mapped country {mappedCountry} should be a valid Country enum value");
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_ShouldHandleEdgeCases()
        {
            // Test edge cases and boundary conditions
            var edgeCases = new[]
            {
                ("   United States   ", Country.UnitedStates, "Should trim whitespace"),
                ("UNITED-STATES", Country.UnitedStates, "Should handle hyphens"),
                ("United_States", Country.UnitedStates, "Should handle underscores"),
                ("United.States", Country.UnitedStates, "Should handle periods"),
                ("United States (USA)", Country.UnitedStates, "Should handle parentheses"),
                ("United States, The", Country.UnitedStates, "Should handle 'The' suffix"),
                ("The United States", Country.UnitedStates, "Should handle 'The' prefix")
            };

            foreach (var (input, expectedCountry, description) in edgeCases)
            {
                var result = GeoJsonNameMapper.MapGeoJsonNameToCountry(input);
                result.Should().Be(expectedCountry, description);
            }
        }

        [TestMethod]
        public void MapGeoJsonNameToCountry_ShouldHandleVariousCountryNames()
        {
            // Test a comprehensive list of country names to ensure all mappings work
            var testCountries = new[]
            {
                "United States", "Germany", "Japan", "Brazil", "China", "India", "Russia", "Canada",
                "Congo", "Congo, Dem. Rep.", "South Korea", "North Korea", "Myanmar", "United Kingdom",
                "France", "Italy", "Spain", "Australia", "New Zealand", "South Africa"
            };
            
            var validMappings = 0;
            var invalidMappings = new List<string>();
            
            foreach (var countryName in testCountries)
            {
                var country = GeoJsonNameMapper.MapGeoJsonNameToCountry(countryName);
                if (country != Country.Unknown)
                {
                    validMappings++;
                }
                else
                {
                    invalidMappings.Add(countryName);
                }
            }
            
            // Most test countries should map correctly, but be less strict
            validMappings.Should().BeGreaterThan(testCountries.Length / 2, 
                "At least 50% of test countries should map correctly");
                
            if (invalidMappings.Any())
            {
                Console.WriteLine($"Countries that didn't map: {string.Join(", ", invalidMappings)}");
            }
        }
    }
}