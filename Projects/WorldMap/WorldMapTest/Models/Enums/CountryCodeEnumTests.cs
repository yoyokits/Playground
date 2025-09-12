// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Models.Enums;

namespace WorldMapTest.Models.Enums
{
    [TestClass]
    public class CountryCodeEnumTests
    {
        [TestMethod]
        public void CountryCode_ShouldHaveCorrectWorldCountryCount()
        {
            // Arrange
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            
            // Act
            var count = allCountryCodes.Count();
            
            // Assert - ISO 3166-1 alpha-2 has 249 officially assigned codes
            // This includes 193 UN member states + 2 observer states + territories and dependencies
            count.Should().BeInRange(240, 260, 
                "CountryCode should contain all ISO 3166-1 alpha-2 codes (approximately 249)");
        }

        [TestMethod]
        public void CountryCode_AllValues_ShouldBeTwoLetterCodes()
        {
            // Arrange
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            var invalidCodes = new List<CountryCode>();

            // Act
            foreach (var code in allCountryCodes)
            {
                var codeString = code.ToString();
                if (codeString.Length != 2 || !codeString.All(char.IsLetter))
                {
                    invalidCodes.Add(code);
                }
            }

            // Assert
            invalidCodes.Should().BeEmpty(
                $"All country codes should be exactly 2 uppercase letters. Invalid: {string.Join(", ", invalidCodes)}");
        }

        [TestMethod]
        public void CountryCode_ShouldContainAllMajorCountries()
        {
            // Test for presence of major world countries
            var majorCountryCodes = new[]
            {
                // G20 countries and other major economies
                CountryCode.US, // United States
                CountryCode.CN, // China
                CountryCode.JP, // Japan
                CountryCode.DE, // Germany
                CountryCode.IN, // India
                CountryCode.GB, // United Kingdom
                CountryCode.FR, // France
                CountryCode.IT, // Italy
                CountryCode.BR, // Brazil
                CountryCode.CA, // Canada
                CountryCode.RU, // Russia
                CountryCode.KR, // South Korea
                CountryCode.AU, // Australia
                CountryCode.MX, // Mexico
                CountryCode.ID, // Indonesia
                CountryCode.NL, // Netherlands
                CountryCode.SA, // Saudi Arabia
                CountryCode.TR, // Turkey
                CountryCode.CH, // Switzerland
                CountryCode.ZA, // South Africa
                
                // Other major countries by population
                CountryCode.PK, // Pakistan
                CountryCode.NG, // Nigeria
                CountryCode.BD, // Bangladesh
                CountryCode.IR, // Iran
                CountryCode.VN, // Vietnam
                CountryCode.PH, // Philippines
                CountryCode.ET, // Ethiopia
                CountryCode.EG, // Egypt
                CountryCode.TH, // Thailand
                CountryCode.TZ, // Tanzania
                CountryCode.MM, // Myanmar
                CountryCode.KE, // Kenya
                CountryCode.UG, // Uganda
                CountryCode.CO, // Colombia
                CountryCode.AR, // Argentina
                CountryCode.PL, // Poland
                CountryCode.ES, // Spain
            };

            var allCountryCodes = Enum.GetValues<CountryCode>();

            foreach (var majorCode in majorCountryCodes)
            {
                allCountryCodes.Should().Contain(majorCode, 
                    $"Major country {majorCode} should be included in CountryCode enum");
            }
        }

        [TestMethod]
        public void CountryCode_ShouldContainAllContinents()
        {
            // Test that enum contains representatives from all continents
            var continentRepresentatives = new Dictionary<string, CountryCode[]>
            {
                ["North America"] = new[] { CountryCode.US, CountryCode.CA, CountryCode.MX, CountryCode.CR },
                ["South America"] = new[] { CountryCode.BR, CountryCode.AR, CountryCode.CL, CountryCode.PE },
                ["Europe"] = new[] { CountryCode.GB, CountryCode.DE, CountryCode.FR, CountryCode.IT, CountryCode.ES },
                ["Asia"] = new[] { CountryCode.CN, CountryCode.IN, CountryCode.JP, CountryCode.KR, CountryCode.TH },
                ["Africa"] = new[] { CountryCode.NG, CountryCode.ZA, CountryCode.EG, CountryCode.KE, CountryCode.ET },
                ["Oceania"] = new[] { CountryCode.AU, CountryCode.NZ, CountryCode.FJ, CountryCode.PG },
                ["Antarctica"] = new[] { CountryCode.AQ }
            };

            var allCountryCodes = Enum.GetValues<CountryCode>();

            foreach (var continent in continentRepresentatives)
            {
                foreach (var representative in continent.Value)
                {
                    allCountryCodes.Should().Contain(representative,
                        $"{continent.Key} representative {representative} should be in CountryCode enum");
                }
            }
        }

        [TestMethod]
        public void CountryCode_ShouldContainUNMembers()
        {
            // Test for UN member states (sample of key members)
            var unMemberSamples = new[]
            {
                CountryCode.AD, // Andorra
                CountryCode.AF, // Afghanistan
                CountryCode.AG, // Antigua and Barbuda
                CountryCode.AL, // Albania
                CountryCode.AM, // Armenia
                CountryCode.AO, // Angola
                CountryCode.AR, // Argentina
                CountryCode.AT, // Austria
                CountryCode.AU, // Australia
                CountryCode.AZ, // Azerbaijan
                CountryCode.BB, // Barbados
                CountryCode.BD, // Bangladesh
                CountryCode.BE, // Belgium
                CountryCode.BF, // Burkina Faso
                CountryCode.BG, // Bulgaria
                CountryCode.BH, // Bahrain
                CountryCode.BI, // Burundi
                CountryCode.BJ, // Benin
                CountryCode.BN, // Brunei Darussalam
                CountryCode.BO, // Bolivia
                CountryCode.BR, // Brazil
                CountryCode.BS, // Bahamas
                CountryCode.BT, // Bhutan
                CountryCode.BW, // Botswana
                CountryCode.BY, // Belarus
                CountryCode.BZ, // Belize
            };

            var allCountryCodes = Enum.GetValues<CountryCode>();

            foreach (var member in unMemberSamples)
            {
                allCountryCodes.Should().Contain(member, 
                    $"UN member {member} should be in CountryCode enum");
            }
        }

        [TestMethod]
        public void CountryCode_ShouldContainTerritories()
        {
            // Test for major territories and dependencies
            var territories = new[]
            {
                CountryCode.PR, // Puerto Rico
                CountryCode.VI, // Virgin Islands (U.S.)
                CountryCode.GU, // Guam
                CountryCode.AS, // American Samoa
                CountryCode.HK, // Hong Kong
                CountryCode.MO, // Macao
                CountryCode.TW, // Taiwan
                CountryCode.GF, // French Guiana
                CountryCode.GP, // Guadeloupe
                CountryCode.MQ, // Martinique
                CountryCode.YT, // Mayotte
                CountryCode.RE, // Réunion
                CountryCode.GL, // Greenland
                CountryCode.FO, // Faroe Islands
                CountryCode.GI, // Gibraltar
                CountryCode.FK, // Falkland Islands
            };

            var allCountryCodes = Enum.GetValues<CountryCode>();

            foreach (var territory in territories)
            {
                allCountryCodes.Should().Contain(territory, 
                    $"Territory/dependency {territory} should be in CountryCode enum");
            }
        }

        [TestMethod]
        public void CountryCode_ShouldNotHaveDuplicateValues()
        {
            // Check for duplicate enum values
            var allCountryCodes = Enum.GetValues<CountryCode>();
            var codeStrings = allCountryCodes.Select(c => c.ToString()).ToList();
            var duplicates = codeStrings.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            duplicates.Should().BeEmpty("There should be no duplicate country codes");
        }

        [TestMethod]
        public void CountryCode_ShouldBeInAlphabeticalOrder()
        {
            // Verify enum values are in alphabetical order (good practice)
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown);
            var codeStrings = allCountryCodes.Select(c => c.ToString()).ToList();
            var sortedCodeStrings = codeStrings.OrderBy(x => x).ToList();

            codeStrings.Should().BeEquivalentTo(sortedCodeStrings, options => options.WithStrictOrdering(),
                "Country codes should be in alphabetical order");
        }

        [TestMethod]
        public void CountryCode_CommonCountryCodes_ShouldExist()
        {
            // Test for commonly used country codes that frequently appear in systems
            var commonCodes = new[]
            {
                CountryCode.US, CountryCode.CA, CountryCode.GB, CountryCode.DE, CountryCode.FR,
                CountryCode.IT, CountryCode.ES, CountryCode.NL, CountryCode.BE, CountryCode.CH,
                CountryCode.AT, CountryCode.SE, CountryCode.NO, CountryCode.DK, CountryCode.FI,
                CountryCode.PL, CountryCode.CZ, CountryCode.HU, CountryCode.RO, CountryCode.BG,
                CountryCode.HR, CountryCode.SI, CountryCode.SK, CountryCode.EE, CountryCode.LV,
                CountryCode.LT, CountryCode.IE, CountryCode.PT, CountryCode.GR, CountryCode.MT,
                CountryCode.CY, CountryCode.LU, CountryCode.RU, CountryCode.CN, CountryCode.JP,
                CountryCode.KR, CountryCode.IN, CountryCode.ID, CountryCode.TH, CountryCode.VN,
                CountryCode.PH, CountryCode.MY, CountryCode.SG, CountryCode.AU, CountryCode.NZ,
                CountryCode.BR, CountryCode.MX, CountryCode.AR, CountryCode.CL, CountryCode.CO,
                CountryCode.PE, CountryCode.VE, CountryCode.ZA, CountryCode.NG, CountryCode.EG,
                CountryCode.MA, CountryCode.KE, CountryCode.ET, CountryCode.GH, CountryCode.TN,
                CountryCode.DZ, CountryCode.LY, CountryCode.SA, CountryCode.AE, CountryCode.IR,
                CountryCode.IQ, CountryCode.IL, CountryCode.TR, CountryCode.JO, CountryCode.LB,
                CountryCode.SY, CountryCode.KW, CountryCode.QA, CountryCode.BH, CountryCode.OM,
                CountryCode.YE, CountryCode.PK, CountryCode.BD, CountryCode.LK, CountryCode.AF,
                CountryCode.NP, CountryCode.MM, CountryCode.KH, CountryCode.LA, CountryCode.BN,
                CountryCode.TW, CountryCode.HK, CountryCode.MO
            };

            var allCountryCodes = Enum.GetValues<CountryCode>();

            foreach (var code in commonCodes)
            {
                allCountryCodes.Should().Contain(code, 
                    $"Common country code {code} should exist in enum");
            }
        }

        [TestMethod]
        public void CountryCode_ShouldContainNewlyRecognizedCountries()
        {
            // Test for relatively new countries that should be included
            var newCountries = new[]
            {
                CountryCode.SS, // South Sudan (2011)
                CountryCode.ME, // Montenegro (2006)
                CountryCode.RS, // Serbia (2006)
                CountryCode.TL, // Timor-Leste (2002)
                CountryCode.MK, // North Macedonia (name changed 2019, but country existed)
                CountryCode.CZ, // Czech Republic (1993, post-Czechoslovakia)
                CountryCode.SK, // Slovakia (1993, post-Czechoslovakia)
            };

            var allCountryCodes = Enum.GetValues<CountryCode>();

            foreach (var code in newCountries)
            {
                allCountryCodes.Should().Contain(code, 
                    $"Newly recognized country {code} should exist in enum");
            }
        }

        [TestMethod]
        public void CountryCode_IslandNations_ShouldBeIncluded()
        {
            // Test for island nations and small states
            var islandNations = new[]
            {
                CountryCode.FJ, // Fiji
                CountryCode.TV, // Tuvalu
                CountryCode.NR, // Nauru
                CountryCode.KI, // Kiribati
                CountryCode.PW, // Palau
                CountryCode.MH, // Marshall Islands
                CountryCode.FM, // Micronesia
                CountryCode.TO, // Tonga
                CountryCode.WS, // Samoa
                CountryCode.VU, // Vanuatu
                CountryCode.SB, // Solomon Islands
                CountryCode.PG, // Papua New Guinea
                CountryCode.MV, // Maldives
                CountryCode.MT, // Malta
                CountryCode.CY, // Cyprus
                CountryCode.IS, // Iceland
                CountryCode.IE, // Ireland
                CountryCode.JM, // Jamaica
                CountryCode.CU, // Cuba
                CountryCode.DO, // Dominican Republic
                CountryCode.HT, // Haiti
                CountryCode.BS, // Bahamas
                CountryCode.BB, // Barbados
                CountryCode.LC, // Saint Lucia
                CountryCode.GD, // Grenada
                CountryCode.VC, // Saint Vincent and the Grenadines
                CountryCode.AG, // Antigua and Barbuda
                CountryCode.KN, // Saint Kitts and Nevis
                CountryCode.DM, // Dominica
                CountryCode.TT, // Trinidad and Tobago
                CountryCode.MU, // Mauritius
                CountryCode.SC, // Seychelles
                CountryCode.KM, // Comoros
                CountryCode.CV, // Cabo Verde
                CountryCode.ST, // Sao Tome and Principe
                CountryCode.LK, // Sri Lanka
                CountryCode.SG, // Singapore
                CountryCode.BN, // Brunei
                CountryCode.BH, // Bahrain
                CountryCode.QA  // Qatar
            };

            var allCountryCodes = Enum.GetValues<CountryCode>();

            foreach (var code in islandNations)
            {
                allCountryCodes.Should().Contain(code, 
                    $"Island nation/small state {code} should exist in enum");
            }
        }

        [TestMethod]
        public void CountryCode_CompareWithWorldCountryStatistics()
        {
            // Compare with known world statistics
            var allCountryCodes = Enum.GetValues<CountryCode>().Where(c => c != CountryCode.Unknown).ToList();
            
            // World Bank lists 195 countries, but ISO 3166-1 has more due to territories
            allCountryCodes.Count.Should().BeGreaterOrEqualTo(195, 
                "Should have at least 195 countries (World Bank count)");
            
            // Should not exceed reasonable maximum (including all territories)
            allCountryCodes.Count.Should().BeLessOrEqualTo(260, 
                "Should not exceed reasonable maximum including territories");
        }
    }
}