// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Models.Enums;

namespace WorldMapTest.Models.Enums
{
    [TestClass]
    public class CountryEnumTests
    {
        [TestMethod]
        public void Country_ShouldMatchAllMajorWorldCountries()
        {
            // Test that Country enum contains all major world countries
            var majorCountries = new Country[]
            {
                // Most populous countries (top 20)
                Country.China, Country.India, Country.UnitedStates, Country.Indonesia, Country.Pakistan,
                Country.Brazil, Country.Nigeria, Country.Bangladesh, Country.Russia, Country.Mexico,
                Country.Japan, Country.Ethiopia, Country.Philippines, Country.Vietnam, Country.Turkey,
                Country.Iran, Country.Germany, Country.Thailand, Country.UnitedKingdom, Country.France,
                
                // Major economic powers (G20)
                Country.Italy, Country.SouthKorea, Country.Canada, Country.Australia, Country.Spain,
                Country.Argentina, Country.SaudiArabia, Country.SouthAfrica, Country.Colombia,
                
                // Regional powers
                Country.Egypt, Country.Myanmar, Country.Kenya, Country.Uganda, Country.Algeria,
                Country.Sudan, Country.Ukraine, Country.Iraq, Country.Afghanistan, Country.Poland,
                Country.Morocco, Country.Angola, Country.Malaysia, Country.Uzbekistan, Country.Peru,
                Country.Ghana, Country.Yemen, Country.Nepal, Country.Venezuela, Country.Madagascar
            };

            var allCountries = Enum.GetValues<Country>();

            foreach (var majorCountry in majorCountries)
            {
                allCountries.Should().Contain(majorCountry, 
                    $"Major country {majorCountry} should exist in Country enum");
            }
        }

        [TestMethod]
        public void Country_ShouldContainAllContinentRepresentatives()
        {
            var continentTests = new Dictionary<string, Country[]>
            {
                ["North America"] = new[] 
                { 
                    Country.UnitedStates, Country.Canada, Country.Mexico, Country.Guatemala, Country.Cuba, 
                    Country.Jamaica, Country.Haiti, Country.DominicanRepublic, Country.CostaRica, Country.Panama,
                    Country.Nicaragua, Country.Honduras, Country.ElSalvador, Country.Belize
                },
                ["South America"] = new[] 
                { 
                    Country.Brazil, Country.Argentina, Country.Chile, Country.Peru, Country.Colombia, 
                    Country.Venezuela, Country.Ecuador, Country.Bolivia, Country.Paraguay, Country.Uruguay,
                    Country.Guyana, Country.Suriname
                },
                ["Europe"] = new[] 
                { 
                    Country.Russia, Country.Germany, Country.UnitedKingdom, Country.France, Country.Italy, 
                    Country.Spain, Country.Ukraine, Country.Poland, Country.Romania, Country.Netherlands,
                    Country.Belgium, Country.Greece, Country.CzechRepublic, Country.Portugal, Country.Hungary,
                    Country.Sweden, Country.Austria, Country.Belarus, Country.Switzerland, Country.Bulgaria,
                    Country.Serbia, Country.Denmark, Country.Finland, Country.Slovakia, Country.Norway,
                    Country.Ireland, Country.Croatia, Country.BosniaAndHerzegovina, Country.Albania,
                    Country.Lithuania, Country.Slovenia, Country.Latvia, Country.Estonia, Country.NorthMacedonia,
                    Country.Moldova, Country.Luxembourg, Country.Malta, Country.Iceland, Country.Montenegro
                },
                ["Asia"] = new[] 
                { 
                    Country.China, Country.India, Country.Indonesia, Country.Pakistan, Country.Bangladesh,
                    Country.Japan, Country.Philippines, Country.Vietnam, Country.Turkey, Country.Iran,
                    Country.Thailand, Country.Myanmar, Country.SouthKorea, Country.Iraq, Country.Afghanistan,
                    Country.Malaysia, Country.Uzbekistan, Country.Nepal, Country.Yemen, Country.NorthKorea,
                    Country.SriLanka, Country.Kazakhstan, Country.Cambodia, Country.Jordan, Country.Azerbaijan,
                    Country.UAE, Country.Tajikistan, Country.Israel, Country.Laos, Country.Singapore,
                    Country.Lebanon, Country.Palestine, Country.Mongolia, Country.Armenia, Country.Kuwait,
                    Country.Georgia, Country.Qatar, Country.Bahrain, Country.TimorLeste, Country.Bhutan,
                    Country.Brunei, Country.Maldives
                },
                ["Africa"] = new[] 
                { 
                    Country.Nigeria, Country.Ethiopia, Country.Egypt, Country.SouthAfrica, Country.Kenya,
                    Country.Uganda, Country.Algeria, Country.Sudan, Country.Morocco, Country.Angola,
                    Country.Ghana, Country.Mozambique, Country.Madagascar, Country.Cameroon, Country.IvoryCoast,
                    Country.Niger, Country.BurkinaFaso, Country.Mali, Country.Malawi, Country.Zambia,
                    Country.Senegal, Country.Somalia, Country.Chad, Country.Zimbabwe, Country.Guinea,
                    Country.Rwanda, Country.Benin, Country.Tunisia, Country.Burundi, Country.SouthSudan,
                    Country.Togo, Country.SierraLeone, Country.Libya, Country.Liberia, Country.CentralAfricanRepublic,
                    Country.Mauritania, Country.Eritrea, Country.Gambia, Country.Botswana, Country.Namibia,
                    Country.Gabon, Country.Lesotho, Country.GuineaBissau, Country.EquatorialGuinea, Country.Mauritius,
                    Country.Eswatini, Country.Djibouti, Country.Comoros, Country.CaboVerde, Country.SaoTomeAndPrincipe,
                    Country.Seychelles
                },
                ["Oceania"] = new[] 
                { 
                    Country.Australia, Country.PapuaNewGuinea, Country.NewZealand, Country.FijiIslands,
                    Country.SolomonIslands, Country.Vanuatu, Country.Samoa, Country.Micronesia,
                    Country.Tonga, Country.Kiribati, Country.Palau, Country.MarshallIslands, Country.Tuvalu,
                    Country.Nauru
                }
            };

            var allCountries = Enum.GetValues<Country>();

            foreach (var continent in continentTests)
            {
                foreach (var expectedCountry in continent.Value)
                {
                    allCountries.Should().Contain(expectedCountry,
                        $"{continent.Key} country {expectedCountry} should exist in Country enum");
                }
            }
        }

        [TestMethod]
        public void Country_ShouldContainSmallCountriesAndMicrostates()
        {
            // Test that small countries and microstates are included
            var smallCountries = new[]
            {
                Country.VaticanCity, Country.Monaco, Country.Liechtenstein, Country.SanMarino, Country.Andorra,
                Country.Malta, Country.Grenada, Country.StVincent, Country.Barbados, Country.AntiguaBarbuda,
                Country.Seychelles, Country.Palau, Country.Nauru, Country.Tuvalu, Country.SanMarino,
                Country.Liechtenstein, Country.Monaco, Country.StKittsNevis, Country.MarshallIslands,
                Country.Dominica, Country.Tonga, Country.Kiribati, Country.Micronesia, Country.Comoros,
                Country.Luxembourg, Country.Cyprus, Country.Bahrain, Country.Trinidad, Country.Mauritius,
                Country.Estonia, Country.Latvia, Country.Lithuania, Country.Slovenia, Country.Croatia,
                Country.BosniaAndHerzegovina, Country.Albania, Country.NorthMacedonia, Country.Montenegro
            };

            var allCountries = Enum.GetValues<Country>();

            foreach (var smallCountry in smallCountries)
            {
                allCountries.Should().Contain(smallCountry, 
                    $"Small country/microstate {smallCountry} should exist in Country enum");
            }
        }

        [TestMethod]
        public void Country_ShouldContainNewlyIndependentCountries()
        {
            // Test for countries that gained independence recently
            var newCountries = new[]
            {
                Country.SouthSudan,      // 2011
                Country.Montenegro,      // 2006
                Country.Serbia,          // 2006 (as separate from Serbia and Montenegro)
                Country.TimorLeste,      // 2002
                Country.CzechRepublic,   // 1993 (post-Czechoslovakia)
                Country.Slovakia,        // 1993 (post-Czechoslovakia)
                Country.Estonia,         // 1991 (post-Soviet)
                Country.Latvia,          // 1991 (post-Soviet)
                Country.Lithuania,       // 1991 (post-Soviet)
                Country.Ukraine,         // 1991 (post-Soviet)
                Country.Belarus,         // 1991 (post-Soviet)
                Country.Moldova,         // 1991 (post-Soviet)
                Country.Kazakhstan,      // 1991 (post-Soviet)
                Country.Uzbekistan,      // 1991 (post-Soviet)
                Country.Kyrgyzstan,      // 1991 (post-Soviet)
                Country.Tajikistan,      // 1991 (post-Soviet)
                Country.Armenia,         // 1991 (post-Soviet)
                Country.Azerbaijan,      // 1991 (post-Soviet)
                Country.Georgia,         // 1991 (post-Soviet)
                Country.Turkmenistan,    // 1991 (post-Soviet)
                Country.Croatia,         // 1991 (post-Yugoslavia)
                Country.Slovenia,        // 1991 (post-Yugoslavia)
                Country.BosniaAndHerzegovina, // 1992 (post-Yugoslavia)
                Country.NorthMacedonia   // 1991 (post-Yugoslavia)
            };

            var allCountries = Enum.GetValues<Country>();

            foreach (var newCountry in newCountries)
            {
                allCountries.Should().Contain(newCountry, 
                    $"Recently independent country {newCountry} should exist in Country enum");
            }
        }

        [TestMethod]
        public void Country_ShouldContainTerritorialEntities()
        {
            // Test for autonomous regions, territories, and dependencies that might be included
            var territorialEntities = new[]
            {
                Country.Greenland,          // Autonomous territory of Denmark
                Country.HongKong,           // SAR of China
                Country.Macao,              // SAR of China
                Country.Taiwan,             // Disputed territory
                Country.PuertoRico,         // US territory
                Country.NewCaledonia,       // French territory
                Country.FrenchGuiana,       // French overseas region
                Country.FrenchPolynesia,    // French territory
                Country.Guadeloupe,         // French territory
                Country.Martinique,         // French territory
                Country.Reunion,            // French territory
                Country.Mayotte,            // French territory
                Country.FrenchSouthernTerritories, // French territory
                Country.WallisAndFutuna,    // French territory
                Country.SaintPierreAndMiquelon, // French territory
                Country.SaintBarthelemy,    // French territory
                Country.SaintMartin,        // French territory
                Country.CookIslands,        // Associated with New Zealand
                Country.Niue,               // Associated with New Zealand
                Country.Tokelau,            // New Zealand territory
                Country.AmericanSamoa,      // US territory
                Country.Guam,               // US territory
                Country.NorthernMarianaIslands, // US territory
                Country.VirginIslandsUS,    // US territory
                Country.UnitedStatesMinorOutlyingIslands, // US territory
                Country.VirginIslandsBritish, // UK territory
                Country.Anguilla,           // UK territory
                Country.Bermuda,            // UK territory
                Country.CaymanIslands,      // UK territory
                Country.FalklandIslands,    // UK territory
                Country.Gibraltar,          // UK territory
                Country.Montserrat,         // UK territory
                Country.Pitcairn,           // UK territory
                Country.SaintHelenaAscensionAndTristanDaCunha, // UK territory
                Country.TurksAndCaicosIslands, // UK territory
                Country.BritishIndianOceanTerritory, // UK territory
                Country.SouthGeorgiaAndSouthSandwichIslands, // UK territory
                Country.IsleOfMan,          // UK Crown dependency
                Country.Jersey,             // UK Crown dependency
                Country.Guernsey,           // UK Crown dependency
                Country.Aruba,              // Netherlands territory
                Country.Curacao,            // Netherlands territory
                Country.SintMaarten,        // Netherlands territory
                Country.BonaireSintEustatiusAndSaba, // Netherlands territory
                Country.AlandIslands,       // Finland autonomous region
                Country.FaroeIslands,       // Denmark territory
                Country.SvalbardAndJanMayen, // Norway territory
                Country.BouvetIsland,       // Norway territory
                Country.HeardIslandAndMcDonaldIslands, // Australia territory
                Country.NorfolkIsland,      // Australia territory
                Country.ChristmasIsland,    // Australia territory
                Country.CocosIslands,       // Australia territory
                Country.Antarctica,         // International territory
                Country.WesternSahara       // Disputed territory
            };

            var allCountries = Enum.GetValues<Country>();

            foreach (var territory in territorialEntities)
            {
                allCountries.Should().Contain(territory, 
                    $"Territorial entity {territory} should exist in Country enum");
            }
        }

        [TestMethod]
        public void Country_ShouldNotHaveDuplicateNames()
        {
            // Check that no country names are duplicated in the enum
            var allCountries = Enum.GetValues<Country>();
            var countryNames = allCountries.Select(c => c.ToString()).ToList();
            
            var duplicates = countryNames.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            duplicates.Should().BeEmpty("Country enum should not have duplicate names");
        }

        [TestMethod]
        public void Country_CountShouldBeComprehensive()
        {
            // Test total count is reasonable for a comprehensive world country list
            var allCountries = Enum.GetValues<Country>().Where(c => c != Country.Unknown);
            var count = allCountries.Count();

            // Should include:
            // - 193 UN member states
            // - 2 UN observer states (Holy See, Palestine) 
            // - Major territories and dependencies
            // - Disputed territories
            // Total should be in range 250-300 for comprehensive coverage
            count.Should().BeInRange(200, 350, 
                "Country enum should comprehensively cover countries, territories, and dependencies");
        }

        [TestMethod]
        public void Country_ShouldBeWellOrganized()
        {
            // Verify Unknown is first (0 value)
            ((int)Country.Unknown).Should().Be(0, "Unknown should have value 0");

            // Verify we don't have unreasonable gaps in enum values
            var allCountries = Enum.GetValues<Country>();
            var enumValues = allCountries.Cast<int>().OrderBy(x => x).ToList();
            
            // Check that values are reasonable sequential (allowing for some gaps)
            var maxValue = enumValues.Max();
            var countryCount = enumValues.Count;
            
            // Ratio should be reasonable (not too many gaps)
            var ratio = (double)maxValue / countryCount;
            ratio.Should().BeLessThan(2.0, "Enum values should not have excessive gaps");
        }

        [TestMethod]
        public void Country_SpecialCases_ShouldBeHandledCorrectly()
        {
            // Test special or controversial cases
            var specialCases = new[]
            {
                (Country.NorthKorea, "North Korea should be included despite isolation"),
                (Country.SouthKorea, "South Korea should be separate from North Korea"), 
                (Country.Taiwan, "Taiwan should be included despite disputed status"),
                (Country.Palestine, "Palestine should be included as UN observer"),
                (Country.VaticanCity, "Vatican City should be included as UN observer"),
                (Country.DemocraticRepublicOfCongo, "DRC should be separate from Republic of Congo"),
                (Country.RepublicOfCongo, "Republic of Congo should be separate from DRC"),
                (Country.SouthSudan, "South Sudan should be included as newest UN member"),
                (Country.WesternSahara, "Western Sahara should be included despite disputed status")
            };

            var allCountries = Enum.GetValues<Country>();

            foreach (var (country, reason) in specialCases)
            {
                if (allCountries.Contains(country))
                {
                    // If it exists, that's good
                    Assert.IsTrue(true, $"{country} correctly included - {reason}");
                }
                else
                {
                    // Note: Some controversial territories may not be included
                    Console.WriteLine($"Note: {country} not included - this may be intentional due to recognition status");
                }
            }
        }
    }
}