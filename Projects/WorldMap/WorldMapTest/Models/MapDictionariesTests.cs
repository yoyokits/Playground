// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Models;
using WorldMapControls.Models.Enums;

namespace WorldMapTest.Models
{
    [TestClass]
    public class MapDictionariesTests
    {
        [TestMethod]
        public void CountryToName_AllCountriesExceptUnknown_ShouldHaveNames()
        {
            // Arrange
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown);
            var missingNames = new List<Country>();

            // Act
            foreach (var country in allCountries)
            {
                if (!MapDictionaries.CountryToName.ContainsKey(country))
                {
                    missingNames.Add(country);
                }
            }

            // Assert
            missingNames.Should().BeEmpty(
                $"All countries should have name mappings. Missing: {string.Join(", ", missingNames)}");
        }

        [TestMethod]
        public void CountryToName_ShouldNotContainNullOrEmptyNames()
        {
            // Act
            var invalidNames = MapDictionaries.CountryToName
                .Where(kvp => string.IsNullOrWhiteSpace(kvp.Value))
                .ToList();

            // Assert
            invalidNames.Should().BeEmpty("All country names should be non-null and non-empty");
        }

        [TestMethod]
        public void CountryToContinent_AllCountriesExceptUnknown_ShouldHaveContinentMapping()
        {
            // Arrange
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown);
            var missingContinents = new List<Country>();

            // Act
            foreach (var country in allCountries)
            {
                if (!MapDictionaries.CountryToContinent.ContainsKey(country))
                {
                    missingContinents.Add(country);
                }
            }

            // Assert
            missingContinents.Should().BeEmpty(
                $"All countries should have continent mappings. Missing: {string.Join(", ", missingContinents)}");
        }

        [TestMethod]
        public void CountryToContinent_ShouldNotMapToUnknownContinent()
        {
            // Act
            var unknownContinentMappings = MapDictionaries.CountryToContinent
                .Where(kvp => kvp.Value == Continent.Unknown && kvp.Key != Country.Unknown)
                .ToList();

            // Assert
            unknownContinentMappings.Should().BeEmpty(
                "No valid country should be mapped to Unknown continent");
        }

        [TestMethod]
        public void ContinentToName_AllContinents_ShouldHaveNames()
        {
            // Arrange
            var allContinents = Enum.GetValues<Continent>();
            var missingNames = new List<Continent>();

            // Act
            foreach (var continent in allContinents)
            {
                if (!MapDictionaries.ContinentToName.ContainsKey(continent))
                {
                    missingNames.Add(continent);
                }
            }

            // Assert
            missingNames.Should().BeEmpty(
                $"All continents should have name mappings. Missing: {string.Join(", ", missingNames)}");
        }

        [TestMethod]
        public void ContinentToCountries_AllContinentsExceptUnknown_ShouldHaveCountries()
        {
            // Arrange
            var continentsExceptUnknown = Enum.GetValues<Continent>().Where(c => c != Continent.Unknown);
            var continentsWithoutCountries = new List<Continent>();

            // Act
            foreach (var continent in continentsExceptUnknown)
            {
                if (!MapDictionaries.ContinentToCountries.ContainsKey(continent) ||
                    !MapDictionaries.ContinentToCountries[continent].Any())
                {
                    continentsWithoutCountries.Add(continent);
                }
            }

            // Assert
            continentsWithoutCountries.Should().BeEmpty(
                $"All continents should have at least one country. Empty: {string.Join(", ", continentsWithoutCountries)}");
        }

        [TestMethod]
        public void ContinentToCountries_CountriesInLists_ShouldBeOrderedByName()
        {
            // Act & Assert
            foreach (var kvp in MapDictionaries.ContinentToCountries)
            {
                if (kvp.Value.Count > 1)
                {
                    var countryNames = kvp.Value.Select(c => MapDictionaries.CountryToName[c]).ToList();
                    var sortedNames = countryNames.OrderBy(n => n).ToList();
                    
                    countryNames.Should().BeEquivalentTo(sortedNames, 
                        $"Countries in {kvp.Key} should be ordered alphabetically by name");
                }
            }
        }

        [TestMethod]
        public void NormalizedNameToCountry_ShouldContainBasicCountryNames()
        {
            // Arrange - Test basic country name normalization
            var testCases = new[]
            {
                ("unitedstates", Country.UnitedStates),
                ("germany", Country.Germany),
                ("japan", Country.Japan),
                ("brazil", Country.Brazil),
                ("unitedkingdom", Country.UnitedKingdom)
            };

            // Act & Assert
            foreach (var (normalizedName, expectedCountry) in testCases)
            {
                MapDictionaries.NormalizedNameToCountry.Should().ContainKey(normalizedName);
                MapDictionaries.NormalizedNameToCountry[normalizedName].Should().Be(expectedCountry);
            }
        }

        [TestMethod]
        public void NormalizedNameToCountry_ShouldContainCommonAliases()
        {
            // Arrange - Test common aliases
            var aliasTests = new[]
            {
                ("usa", Country.UnitedStates),
                ("us", Country.UnitedStates),
                ("uk", Country.UnitedKingdom),
                ("greatbritain", Country.UnitedKingdom),
                ("uae", Country.UAE),
                ("drc", Country.DemocraticRepublicOfCongo),
                ("congo", Country.RepublicOfCongo) // Short "Congo" should map to Republic of Congo
            };

            // Act & Assert
            foreach (var (alias, expectedCountry) in aliasTests)
            {
                MapDictionaries.NormalizedNameToCountry.Should().ContainKey(alias, 
                    $"Alias '{alias}' should be mapped");
                MapDictionaries.NormalizedNameToCountry[alias].Should().Be(expectedCountry,
                    $"Alias '{alias}' should map to {expectedCountry}");
            }
        }

        [TestMethod]
        public void NormalizedNameToCountry_CongoAliases_ShouldBeMappedCorrectly()
        {
            // This is critical for GeoJSON mapping - Congo variants should map correctly
            var congoMappings = new[]
            {
                ("congo", Country.RepublicOfCongo),
                ("republicofthecongo", Country.RepublicOfCongo),
                ("congobrazzaville", Country.RepublicOfCongo),
                ("democraticrepublicofthecongo", Country.DemocraticRepublicOfCongo),
                ("drc", Country.DemocraticRepublicOfCongo),
                ("congokinshasa", Country.DemocraticRepublicOfCongo),
                ("zaire", Country.DemocraticRepublicOfCongo)
            };

            foreach (var (alias, expectedCountry) in congoMappings)
            {
                MapDictionaries.NormalizedNameToCountry.Should().ContainKey(alias,
                    $"Congo alias '{alias}' should be mapped");
                MapDictionaries.NormalizedNameToCountry[alias].Should().Be(expectedCountry,
                    $"Congo alias '{alias}' should map to {expectedCountry}");
            }
        }

        [TestMethod]
        public void AllCountriesInMapDictionaries_ShouldMatchCountryEnum()
        {
            // Arrange
            var countriesInEnum = Enum.GetValues<Country>().ToHashSet();
            var countriesInNameDict = MapDictionaries.CountryToName.Keys.ToHashSet();
            var countriesInContinentDict = MapDictionaries.CountryToContinent.Keys.ToHashSet();

            // Act & Assert
            countriesInNameDict.Should().BeEquivalentTo(countriesInEnum,
                "CountryToName should contain exactly the same countries as Country enum");
            
            countriesInContinentDict.Should().BeEquivalentTo(countriesInEnum,
                "CountryToContinent should contain exactly the same countries as Country enum");
        }

        [TestMethod]
        public void ContinentToCountries_AllCountries_ShouldBeIncluded()
        {
            // Arrange
            var allCountriesFromEnum = Enum.GetValues<Country>().Where(c => c != Country.Unknown).ToHashSet();
            var allCountriesFromContinentLists = MapDictionaries.ContinentToCountries.Values
                .SelectMany(countries => countries)
                .ToHashSet();

            // Act & Assert - Check that all enum countries are in continent lists
            allCountriesFromEnum.Should().BeSubsetOf(allCountriesFromContinentLists,
                "All countries from enum should be included in continent country lists");
                
            // Check that we don't have too many extra countries in continent lists
            var extraCountries = allCountriesFromContinentLists.Except(allCountriesFromEnum).ToList();
            extraCountries.Should().BeEmpty(
                $"Continent lists should not contain countries not in enum. Extra: {string.Join(", ", extraCountries)}");
        }

        [TestMethod]
        public void ContinentDistribution_ShouldBeReasonable()
        {
            // Test that continents have reasonable country distributions
            var continentCounts = MapDictionaries.ContinentToCountries
                .Where(kvp => kvp.Key != Continent.Unknown)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            // Assert reasonable minimums (these are approximate based on actual geography)
            continentCounts[Continent.Africa].Should().BeGreaterThan(50, "Africa has many countries");
            continentCounts[Continent.Europe].Should().BeGreaterThan(40, "Europe has many countries");
            continentCounts[Continent.Asia].Should().BeGreaterThan(35, "Asia has many countries");
            continentCounts[Continent.NorthAmerica].Should().BeGreaterThan(20, "North America includes Central America and Caribbean");
            continentCounts[Continent.SouthAmerica].Should().BeGreaterThan(10, "South America has 12+ countries");
            continentCounts[Continent.Oceania].Should().BeGreaterThan(10, "Oceania has many island nations");
        }

        [TestMethod]
        public void MajorCountriesMapping_ShouldBeCorrect()
        {
            // Test major countries are correctly mapped
            var majorCountryMappings = new[]
            {
                (Country.UnitedStates, "United States", Continent.NorthAmerica),
                (Country.China, "China", Continent.Asia),
                (Country.India, "India", Continent.Asia),
                (Country.Indonesia, "Indonesia", Continent.Asia),
                (Country.Pakistan, "Pakistan", Continent.Asia),
                (Country.Brazil, "Brazil", Continent.SouthAmerica),
                (Country.Nigeria, "Nigeria", Continent.Africa),
                (Country.Bangladesh, "Bangladesh", Continent.Asia),
                (Country.Russia, "Russia", Continent.Europe),
                (Country.Mexico, "Mexico", Continent.NorthAmerica),
                (Country.Japan, "Japan", Continent.Asia),
                (Country.Ethiopia, "Ethiopia", Continent.Africa),
                (Country.Philippines, "Philippines", Continent.Asia),
                (Country.Vietnam, "Vietnam", Continent.Asia),
                (Country.Turkey, "Turkey", Continent.Europe),
                (Country.Iran, "Iran", Continent.MiddleEast),
                (Country.Germany, "Germany", Continent.Europe),
                (Country.Thailand, "Thailand", Continent.Asia),
                (Country.UnitedKingdom, "United Kingdom", Continent.Europe),
                (Country.France, "France", Continent.Europe),
                (Country.Italy, "Italy", Continent.Europe),
                (Country.Tanzania, "Tanzania", Continent.Africa),
                (Country.SouthAfrica, "South Africa", Continent.Africa),
                (Country.Myanmar, "Myanmar", Continent.Asia),
                (Country.Kenya, "Kenya", Continent.Africa),
                (Country.SouthKorea, "South Korea", Continent.Asia),
                (Country.Colombia, "Colombia", Continent.SouthAmerica),
                (Country.Spain, "Spain", Continent.Europe),
                (Country.Uganda, "Uganda", Continent.Africa),
                (Country.Argentina, "Argentina", Continent.SouthAmerica)
            };

            foreach (var (country, expectedName, expectedContinent) in majorCountryMappings)
            {
                MapDictionaries.CountryToName[country].Should().Be(expectedName,
                    $"{country} should have name {expectedName}");
                MapDictionaries.CountryToContinent[country].Should().Be(expectedContinent,
                    $"{country} should be in {expectedContinent}");
            }
        }
    }
}