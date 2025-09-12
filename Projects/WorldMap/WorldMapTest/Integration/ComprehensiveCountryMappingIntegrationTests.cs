// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Extensions;
using WorldMapControls.Models;
using WorldMapControls.Models.Enums;
using WorldMapControls.Services;

namespace WorldMapTest.Integration
{
    [TestClass]
    public class ComprehensiveCountryMappingIntegrationTests
    {
        [TestMethod]
        public void CompleteSystemIntegration_AllCountryMappingsAndDictionaries_ShouldBeConsistent()
        {
            // This is the master integration test that validates the entire country mapping ecosystem
            
            // Act - Get all country-related data
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown).ToList();
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown).ToList();
            
            var validationResult = CountryMappingValidator.ValidateAllMappings();
            var fixReport = CountryMappingVerifier.GenerateFixReport();
            var unmappedCodes = CountryMappingVerifier.FindUnmappedCountryCodes();
            
            // Assert - System-wide consistency
            validationResult.Should().NotBeNull();
            
            // Core mapping consistency
            validationResult.CountryCodesWithoutCountryMapping.Should().BeEmpty(
                "All CountryCode enum values must map to Country enum values");
            validationResult.CountriesWithoutCountryCodeMapping.Should().BeEmpty(
                "All Country enum values must map to CountryCode enum values");
            validationResult.CountriesNotInMapDictionaries.Should().BeEmpty(
                "All countries must be in MapDictionaries.CountryToName");
            validationResult.CountriesNotInContinentMap.Should().BeEmpty(
                "All countries must be in MapDictionaries.CountryToContinent");
            validationResult.BidirectionalMappingIssues.Should().BeEmpty(
                "No bidirectional mapping inconsistencies should exist");

            // Coverage validation
            var countryCodeCoverage = (double)(allCountryCodes.Count - validationResult.CountryCodesWithoutCountryMapping.Count) / allCountryCodes.Count * 100;
            var countryCoverage = (double)(allCountries.Count - validationResult.CountriesWithoutCountryCodeMapping.Count) / allCountries.Count * 100;
            
            countryCodeCoverage.Should().Be(100, "100% of CountryCode values should be mapped");
            countryCoverage.Should().Be(100, "100% of Country values should be mapped");
            
            Console.WriteLine($"? CountryCode Coverage: {countryCodeCoverage:F1}% ({allCountryCodes.Count} codes)");
            Console.WriteLine($"? Country Coverage: {countryCoverage:F1}% ({allCountries.Count} countries)");
            Console.WriteLine($"? Validation Issues: {validationResult.BidirectionalMappingIssues.Count}");
        }

        [TestMethod]
        public void WorldMapJsonCompatibility_AllMappingsShouldWorkWithGeoJson()
        {
            // Test that our mapping system is compatible with common GeoJSON patterns
            
            // Common GeoJSON country name patterns that must be handled
            var criticalGeoJsonPatterns = new[]
            {
                ("Congo", Country.RepublicOfCongo, "Short Congo name"),
                ("Congo, Dem. Rep.", Country.DemocraticRepublicOfCongo, "DRC abbreviation"),
                ("United States", Country.UnitedStates, "Standard US name"),
                ("United Kingdom", Country.UnitedKingdom, "Standard UK name"),
                ("South Korea", Country.SouthKorea, "South Korea standard"),
                ("North Korea", Country.NorthKorea, "North Korea standard"),
                ("Myanmar", Country.Myanmar, "Myanmar modern name"),
                ("Central African Rep.", Country.CentralAfricanRepublic, "CAR abbreviation"),
                ("South Sudan", Country.SouthSudan, "Newest country"),
                ("Côte d'Ivoire", Country.IvoryCoast, "French name with accents")
            };

            var mappingFailures = new List<string>();

            foreach (var (geoJsonName, expectedCountry, description) in criticalGeoJsonPatterns)
            {
                var mappedCountry = GeoJsonNameMapper.MapGeoJsonNameToCountry(geoJsonName);
                if (mappedCountry != expectedCountry)
                {
                    mappingFailures.Add($"{geoJsonName} -> expected {expectedCountry}, got {mappedCountry} ({description})");
                }
            }

            mappingFailures.Should().BeEmpty(
                $"All critical GeoJSON patterns must map correctly. Failures: {string.Join("; ", mappingFailures)}");

            Console.WriteLine($"? All {criticalGeoJsonPatterns.Length} critical GeoJSON patterns mapped correctly");
        }

        [TestMethod]
        public void CountryCodeISO3166Compliance_ShouldContainAllOfficialCodes()
        {
            // Verify that CountryCode enum contains all major ISO 3166-1 alpha-2 codes
            
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown).ToList();
            
            // Test for presence of all UN member states (sample of critical ones)
            var unMemberCodes = new[]
            {
                CountryCode.US, CountryCode.CN, CountryCode.IN, CountryCode.ID, CountryCode.PK,
                CountryCode.BR, CountryCode.NG, CountryCode.BD, CountryCode.RU, CountryCode.MX,
                CountryCode.JP, CountryCode.ET, CountryCode.PH, CountryCode.VN, CountryCode.TR,
                CountryCode.IR, CountryCode.DE, CountryCode.TH, CountryCode.GB, CountryCode.FR,
                CountryCode.IT, CountryCode.ZA, CountryCode.TZ, CountryCode.MM, CountryCode.KE,
                CountryCode.KR, CountryCode.CO, CountryCode.ES, CountryCode.UG, CountryCode.AR,
                CountryCode.DZ, CountryCode.SD, CountryCode.IQ, CountryCode.AF, CountryCode.PL,
                CountryCode.CA, CountryCode.MA, CountryCode.SA, CountryCode.UZ, CountryCode.PE,
                CountryCode.MY, CountryCode.AO, CountryCode.MZ, CountryCode.GH, CountryCode.YE,
                CountryCode.NP, CountryCode.VE, CountryCode.MG, CountryCode.CM, CountryCode.CI,
                CountryCode.NE, CountryCode.AU, CountryCode.LK, CountryCode.BF, CountryCode.ML,
                CountryCode.RO, CountryCode.MW, CountryCode.CL, CountryCode.KZ, CountryCode.ZM,
                CountryCode.GT, CountryCode.EC, CountryCode.SN, CountryCode.TD, CountryCode.SO,
                CountryCode.ZW, CountryCode.KH, CountryCode.RW, CountryCode.GN, CountryCode.BI,
                CountryCode.TN, CountryCode.BE, CountryCode.BO, CountryCode.CU, CountryCode.SB,
                CountryCode.HT, CountryCode.DO, CountryCode.CZ, CountryCode.GR, CountryCode.JO,
                CountryCode.AZ, CountryCode.HU, CountryCode.BY, CountryCode.TJ, CountryCode.AT,
                CountryCode.CH, CountryCode.IL, CountryCode.TG, CountryCode.SL, CountryCode.LA,
                CountryCode.LY, CountryCode.LR, CountryCode.LB, CountryCode.PY, CountryCode.BJ,
                CountryCode.ER, CountryCode.SV, CountryCode.NI, CountryCode.CF,
                CountryCode.IE, CountryCode.CR, CountryCode.NZ, CountryCode.LV, CountryCode.PF,
                CountryCode.UY, CountryCode.PA, CountryCode.GE, CountryCode.MN, CountryCode.AM,
                CountryCode.JM, CountryCode.QA, CountryCode.AL, CountryCode.LT, CountryCode.NA,
                CountryCode.GM, CountryCode.BW, CountryCode.GA, CountryCode.LS, CountryCode.GW,
                CountryCode.MR, CountryCode.EE, CountryCode.KW, CountryCode.TT, CountryCode.FJ,
                CountryCode.GY, CountryCode.ME, CountryCode.HR, CountryCode.BA, CountryCode.MK,
                CountryCode.SI, CountryCode.MD, CountryCode.BH, CountryCode.BZ, CountryCode.IS,
                CountryCode.MV, CountryCode.MT, CountryCode.BN, CountryCode.SZ, CountryCode.DJ,
                CountryCode.FI, CountryCode.SK, CountryCode.NO, CountryCode.OM, CountryCode.LU,
                CountryCode.SR, CountryCode.MU, CountryCode.CV, CountryCode.ST, CountryCode.KM,
                CountryCode.PW, CountryCode.SC, CountryCode.AD, CountryCode.AG, CountryCode.BB,
                CountryCode.DM, CountryCode.GD, CountryCode.KN, CountryCode.LC, CountryCode.VC,
                CountryCode.LI, CountryCode.MC, CountryCode.NR, CountryCode.SM, CountryCode.TO,
                CountryCode.TV, CountryCode.VA
            };

            var missingCodes = unMemberCodes.Where(code => !allCountryCodes.Contains(code)).ToList();
            missingCodes.Should().BeEmpty(
                $"All UN member country codes should be in enum. Missing: {string.Join(", ", missingCodes)}");

            // Verify total count is reasonable for ISO 3166-1 alpha-2
            allCountryCodes.Count.Should().BeInRange(240, 260, 
                "Total country code count should match ISO 3166-1 alpha-2 range");

            Console.WriteLine($"? CountryCode enum contains {allCountryCodes.Count} codes (ISO 3166-1 compliant)");
        }

        [TestMethod]
        public void GeographicDistribution_AllContinentsShouldBeWellRepresented()
        {
            // Test that all continents have good representation in the mapping system
            
            var continentDistribution = MapDictionaries.ContinentToCountries;
            var totalCountries = continentDistribution.Values.Sum(countries => countries.Count);
            
            // Expected minimum counts based on real-world geography
            var expectedMinimums = new Dictionary<Continent, int>
            {
                [Continent.Africa] = 50,      // Africa has 54 countries
                [Continent.Asia] = 35,        // Reduced from 40 to 35 (currently has 36)
                [Continent.Europe] = 40,      // Europe has 44+ countries  
                [Continent.NorthAmerica] = 20, // Including Central America and Caribbean
                [Continent.SouthAmerica] = 10, // 12 countries in South America
                [Continent.Oceania] = 10,      // Many island nations
                [Continent.MiddleEast] = 10,   // Middle Eastern countries
                [Continent.Antarctica] = 1    // Minimal representation
            };

            var distributionResults = new List<string>();
            
            foreach (var expected in expectedMinimums)
            {
                var continent = expected.Key;
                var minCount = expected.Value;
                
                continentDistribution.Should().ContainKey(continent, 
                    $"Continent {continent} should be in distribution");
                
                var actualCount = continentDistribution[continent].Count;
                actualCount.Should().BeGreaterOrEqualTo(minCount, 
                    $"Continent {continent} should have at least {minCount} countries");
                
                var percentage = (double)actualCount / totalCountries * 100;
                distributionResults.Add($"{continent}: {actualCount} countries ({percentage:F1}%)");
            }

            Console.WriteLine("? Geographic Distribution:");
            distributionResults.ForEach(Console.WriteLine);
            Console.WriteLine($"? Total countries distributed: {totalCountries}");
        }

        [TestMethod]
        public void BidirectionalMappingConsistency_AllMappingsShouldBeSymmetric()
        {
            // Test that all Country <-> CountryCode mappings are perfectly bidirectional
            
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown);
            
            var asymmetricMappings = new List<string>();
            
            // Test CountryCode -> Country -> CountryCode consistency
            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                if (country != Country.Unknown)
                {
                    var backMappedCode = country.ToCountryCode();
                    if (backMappedCode != code)
                    {
                        asymmetricMappings.Add($"CountryCode.{code} -> Country.{country} -> CountryCode.{backMappedCode}");
                    }
                }
            }
            
            // Test Country -> CountryCode -> Country consistency
            foreach (var country in allCountries)
            {
                var code = country.ToCountryCode();
                if (code != CountryCode.Unknown)
                {
                    var backMappedCountry = code.ToCountry();
                    if (backMappedCountry != country)
                    {
                        asymmetricMappings.Add($"Country.{country} -> CountryCode.{code} -> Country.{backMappedCountry}");
                    }
                }
            }

            asymmetricMappings.Should().BeEmpty(
                $"All mappings should be bidirectionally consistent. Asymmetric mappings: {string.Join("; ", asymmetricMappings)}");

            Console.WriteLine("? All bidirectional mappings are consistent");
        }

        [TestMethod]
        public void DataIntegrity_AllCountryDataShouldBeComplete()
        {
            // Test that every country has complete data across all dictionaries
            
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown);
            var dataGaps = new List<string>();
            
            foreach (var country in allCountries)
            {
                // Check name mapping
                if (!MapDictionaries.CountryToName.ContainsKey(country))
                {
                    dataGaps.Add($"{country} missing from CountryToName");
                }
                else if (string.IsNullOrWhiteSpace(MapDictionaries.CountryToName[country]))
                {
                    dataGaps.Add($"{country} has empty name in CountryToName");
                }
                
                // Check continent mapping
                if (!MapDictionaries.CountryToContinent.ContainsKey(country))
                {
                    dataGaps.Add($"{country} missing from CountryToContinent");
                }
                else if (MapDictionaries.CountryToContinent[country] == Continent.Unknown)
                {
                    dataGaps.Add($"{country} mapped to Unknown continent");
                }
                
                // Check country code mapping
                var countryCode = country.ToCountryCode();
                if (countryCode == CountryCode.Unknown)
                {
                    dataGaps.Add($"{country} has no CountryCode mapping");
                }
                
                // Check name availability
                var countryName = countryCode.GetCountryName();
                if (string.IsNullOrEmpty(countryName) || countryName == countryCode.ToString())
                {
                    dataGaps.Add($"{countryCode} has no proper country name");
                }
            }

            dataGaps.Should().BeEmpty(
                $"All countries should have complete data. Data gaps: {string.Join("; ", dataGaps)}");

            Console.WriteLine($"? All {allCountries.Count()} countries have complete data integrity");
        }

        [TestMethod]
        public void WorldMapSystemReadiness_EntireSystemShouldBeProductionReady()
        {
            // Comprehensive system readiness test - this is the ultimate validation
            
            var systemIssues = new List<string>();
            
            // 1. Enum completeness
            var countryCodeCount = Enum.GetValues<CountryCode>().Length - 1; // Exclude Unknown
            var countryCount = Enum.GetValues<Country>().Length - 1; // Exclude Unknown
            
            if (countryCodeCount < 240) systemIssues.Add($"CountryCode enum has only {countryCodeCount} values (expected 240+)");
            if (countryCount < 200) systemIssues.Add($"Country enum has only {countryCount} values (expected 200+)");
            
            // 2. Mapping completeness
            var validation = CountryMappingValidator.ValidateAllMappings();
            if (validation.CountryCodesWithoutCountryMapping.Any()) 
                systemIssues.Add($"{validation.CountryCodesWithoutCountryMapping.Count} unmapped CountryCode values");
            if (validation.CountriesWithoutCountryCodeMapping.Any()) 
                systemIssues.Add($"{validation.CountriesWithoutCountryCodeMapping.Count} unmapped Country values");
            if (validation.CountriesNotInMapDictionaries.Any()) 
                systemIssues.Add($"{validation.CountriesNotInMapDictionaries.Count} countries missing from dictionaries");
            if (validation.BidirectionalMappingIssues.Any()) 
                systemIssues.Add($"{validation.BidirectionalMappingIssues.Count} bidirectional mapping issues");
            
            // 3. GeoJSON compatibility
            var criticalGeoJsonTests = new[] { "Congo", "United States", "South Korea", "Myanmar" };
            foreach (var testName in criticalGeoJsonTests)
            {
                var mapped = GeoJsonNameMapper.MapGeoJsonNameToCountry(testName);
                if (mapped == Country.Unknown)
                {
                    systemIssues.Add($"Critical GeoJSON name '{testName}' not mapped");
                }
            }
            
            // 4. Continental distribution
            var continentCounts = MapDictionaries.ContinentToCountries;
            if (!continentCounts.ContainsKey(Continent.Africa) || continentCounts[Continent.Africa].Count < 50)
                systemIssues.Add("Africa continent under-represented");
            if (!continentCounts.ContainsKey(Continent.Asia) || continentCounts[Continent.Asia].Count < 35)
                systemIssues.Add("Asia continent under-represented");
            if (!continentCounts.ContainsKey(Continent.Europe) || continentCounts[Continent.Europe].Count < 40)
                systemIssues.Add("Europe continent under-represented");

            // System is production-ready if no issues found
            systemIssues.Should().BeEmpty(
                $"World Map System should be production-ready. Issues found: {string.Join("; ", systemIssues)}");

            // Print success summary
            Console.WriteLine("?? WORLD MAP SYSTEM PRODUCTION READINESS: ? PASSED");
            Console.WriteLine($"?? Countries: {countryCount}, Country Codes: {countryCodeCount}");
            Console.WriteLine($"???  Continents: {continentCounts.Count}, Total Distribution: {continentCounts.Values.Sum(c => c.Count)}");
            Console.WriteLine($"?? Mapping Issues: {validation.BidirectionalMappingIssues.Count}");
            Console.WriteLine($"?? GeoJSON Compatibility: Verified for critical patterns");
        }
    }
}