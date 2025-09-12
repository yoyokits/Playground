// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Centralized dictionaries for geographic lookups.
    /// Guarantees that every Country enum member has a display name and a continent mapping.
    /// </summary>
    public static class MapDictionaries
    {
        #region Fields

        public static readonly IReadOnlyDictionary<Continent, IReadOnlyList<Country>> ContinentToCountries;
        public static readonly IReadOnlyDictionary<Continent, string> ContinentToName;
        public static readonly IReadOnlyDictionary<Country, Continent> CountryToContinent;
        public static readonly IReadOnlyDictionary<Country, string> CountryToName;
        public static readonly IReadOnlyDictionary<string, Country> NormalizedNameToCountry;

        #endregion Fields

        #region Constructors

        static MapDictionaries()
        {
            // Use concrete dictionaries first so we can patch missing items
            var countryNameDict = new Dictionary<Country, string>
            {
                { Country.Unknown, "Unknown" },
                // North America
                { Country.UnitedStates, "United States" }, { Country.Canada, "Canada" }, { Country.Mexico, "Mexico" }, { Country.Greenland, "Greenland" },
                { Country.Guatemala, "Guatemala" }, { Country.Belize, "Belize" }, { Country.ElSalvador, "El Salvador" }, { Country.Honduras, "Honduras" },
                { Country.Nicaragua, "Nicaragua" }, { Country.CostaRica, "Costa Rica" }, { Country.Panama, "Panama" }, { Country.Cuba, "Cuba" },
                { Country.Jamaica, "Jamaica" }, { Country.Haiti, "Haiti" }, { Country.DominicanRepublic, "Dominican Republic" },
                // South America
                { Country.Brazil, "Brazil" }, { Country.Argentina, "Argentina" }, { Country.Chile, "Chile" }, { Country.Colombia, "Colombia" },
                { Country.Venezuela, "Venezuela" }, { Country.Peru, "Peru" }, { Country.Ecuador, "Ecuador" }, { Country.Bolivia, "Bolivia" },
                { Country.Paraguay, "Paraguay" }, { Country.Uruguay, "Uruguay" }, { Country.Guyana, "Guyana" }, { Country.Suriname, "Suriname" },
                // Europe
                { Country.UnitedKingdom, "United Kingdom" }, { Country.France, "France" }, { Country.Germany, "Germany" }, { Country.Italy, "Italy" },
                { Country.Spain, "Spain" }, { Country.Portugal, "Portugal" }, { Country.Netherlands, "Netherlands" }, { Country.Belgium, "Belgium" },
                { Country.Switzerland, "Switzerland" }, { Country.Austria, "Austria" }, { Country.Sweden, "Sweden" }, { Country.Norway, "Norway" },
                { Country.Finland, "Finland" }, { Country.Denmark, "Denmark" }, { Country.Poland, "Poland" }, { Country.Russia, "Russia" },
                { Country.Ukraine, "Ukraine" }, { Country.Turkey, "Turkey" }, { Country.Greece, "Greece" }, { Country.Romania, "Romania" },
                { Country.Bulgaria, "Bulgaria" }, { Country.Hungary, "Hungary" }, { Country.CzechRepublic, "Czech Republic" }, { Country.Slovakia, "Slovakia" },
                { Country.Slovenia, "Slovenia" }, { Country.Croatia, "Croatia" }, { Country.Serbia, "Serbia" }, { Country.Montenegro, "Montenegro" },
                { Country.BosniaAndHerzegovina, "Bosnia and Herzegovina" }, { Country.NorthMacedonia, "North Macedonia" }, { Country.Albania, "Albania" },
                { Country.Estonia, "Estonia" }, { Country.Latvia, "Latvia" }, { Country.Lithuania, "Lithuania" }, { Country.Belarus, "Belarus" },
                { Country.Moldova, "Moldova" }, { Country.Azerbaijan, "Azerbaijan" }, { Country.Iceland, "Iceland" }, { Country.Ireland, "Ireland" },
                { Country.Georgia, "Georgia" }, { Country.Armenia, "Armenia" }, { Country.Kosovo, "Kosovo" },
                // Asia
                { Country.China, "China" }, { Country.Japan, "Japan" }, { Country.SouthKorea, "South Korea" }, { Country.NorthKorea, "North Korea" },
                { Country.India, "India" }, { Country.Indonesia, "Indonesia" }, { Country.Vietnam, "Vietnam" }, { Country.Thailand, "Thailand" },
                { Country.Malaysia, "Malaysia" }, { Country.Singapore, "Singapore" }, { Country.Philippines, "Philippines" }, { Country.Myanmar, "Myanmar" },
                { Country.Cambodia, "Cambodia" }, { Country.Laos, "Laos" }, { Country.Bangladesh, "Bangladesh" }, { Country.Pakistan, "Pakistan" },
                { Country.Afghanistan, "Afghanistan" }, { Country.Nepal, "Nepal" }, { Country.Bhutan, "Bhutan" }, { Country.SriLanka, "Sri Lanka" },
                { Country.Maldives, "Maldives" }, { Country.Mongolia, "Mongolia" }, { Country.Kazakhstan, "Kazakhstan" }, { Country.Uzbekistan, "Uzbekistan" },
                { Country.Turkmenistan, "Turkmenistan" }, { Country.Kyrgyzstan, "Kyrgyzstan" }, { Country.Tajikistan, "Tajikistan" }, { Country.TimorLeste, "Timor-Leste" },
                // Middle East
                { Country.SaudiArabia, "Saudi Arabia" }, { Country.Iran, "Iran" }, { Country.Iraq, "Iraq" }, { Country.Israel, "Israel" },
                { Country.Palestine, "Palestine" }, { Country.Jordan, "Jordan" }, { Country.Lebanon, "Lebanon" }, { Country.Syria, "Syria" },
                { Country.Yemen, "Yemen" }, { Country.Oman, "Oman" }, { Country.UAE, "United Arab Emirates" }, { Country.Qatar, "Qatar" },
                { Country.Bahrain, "Bahrain" }, { Country.Kuwait, "Kuwait" },
                // Africa
                { Country.SouthAfrica, "South Africa" }, { Country.Egypt, "Egypt" }, { Country.Nigeria, "Nigeria" }, { Country.Kenya, "Kenya" },
                { Country.Ethiopia, "Ethiopia" }, { Country.Tanzania, "Tanzania" }, { Country.Uganda, "Uganda" }, { Country.Rwanda, "Rwanda" },
                { Country.Burundi, "Burundi" }, { Country.DemocraticRepublicOfCongo, "Democratic Republic of the Congo" }, { Country.RepublicOfCongo, "Republic of the Congo" },
                { Country.CentralAfricanRepublic, "Central African Republic" }, { Country.Chad, "Chad" }, { Country.Sudan, "Sudan" }, { Country.SouthSudan, "South Sudan" },
                { Country.Libya, "Libya" }, { Country.Tunisia, "Tunisia" }, { Country.Algeria, "Algeria" }, { Country.Morocco, "Morocco" }, { Country.Ghana, "Ghana" },
                { Country.IvoryCoast, "Ivory Coast" }, { Country.BurkinaFaso, "Burkina Faso" }, { Country.Mali, "Mali" }, { Country.Niger, "Niger" },
                { Country.Senegal, "Senegal" }, { Country.Guinea, "Guinea" }, { Country.SierraLeone, "Sierra Leone" }, { Country.Liberia, "Liberia" },
                { Country.Gambia, "Gambia" }, { Country.GuineaBissau, "Guinea-Bissau" }, { Country.Mauritania, "Mauritania" }, { Country.Madagascar, "Madagascar" },
                { Country.Mozambique, "Mozambique" }, { Country.Zimbabwe, "Zimbabwe" }, { Country.Botswana, "Botswana" }, { Country.Namibia, "Namibia" },
                { Country.Zambia, "Zambia" }, { Country.Malawi, "Malawi" }, { Country.Angola, "Angola" }, { Country.Cameroon, "Cameroon" },
                { Country.EquatorialGuinea, "Equatorial Guinea" }, { Country.Gabon, "Gabon" }, { Country.SaoTomeAndPrincipe, "Sao Tome and Principe" }, { Country.Somalia, "Somalia" },
                // Oceania
                { Country.Australia, "Australia" }, { Country.NewZealand, "New Zealand" }, { Country.FijiIslands, "Fiji" }, { Country.PapuaNewGuinea, "Papua New Guinea" },
                { Country.SolomonIslands, "Solomon Islands" }, { Country.Vanuatu, "Vanuatu" }, { Country.NewCaledonia, "New Caledonia" }, { Country.Samoa, "Samoa" },
                { Country.Tonga, "Tonga" }, { Country.Palau, "Palau" }, { Country.MarshallIslands, "Marshall Islands" }, { Country.Micronesia, "Micronesia" },
                { Country.Kiribati, "Kiribati" }, { Country.Nauru, "Nauru" }, { Country.Tuvalu, "Tuvalu" },
                // Additional sovereign & small states
                { Country.Cyprus, "Cyprus" }, { Country.Malta, "Malta" }, { Country.Luxembourg, "Luxembourg" }, { Country.Monaco, "Monaco" }, { Country.Liechtenstein, "Liechtenstein" },
                { Country.Andorra, "Andorra" }, { Country.SanMarino, "San Marino" }, { Country.VaticanCity, "Vatican City" }, { Country.Djibouti, "Djibouti" }, { Country.Eritrea, "Eritrea" },
                { Country.Comoros, "Comoros" }, { Country.Seychelles, "Seychelles" }, { Country.Mauritius, "Mauritius" }, { Country.CaboVerde, "Cabo Verde" }, { Country.Barbados, "Barbados" },
                { Country.Trinidad, "Trinidad and Tobago" }, { Country.Grenada, "Grenada" }, { Country.StLucia, "Saint Lucia" }, { Country.StVincent, "Saint Vincent and the Grenadines" },
                { Country.Dominica, "Dominica" }, { Country.AntiguaBarbuda, "Antigua and Barbuda" }, { Country.StKittsNevis, "Saint Kitts and Nevis" }, { Country.Bahamas, "Bahamas" }, { Country.Brunei, "Brunei" },
                { Country.Lesotho, "Lesotho" }, { Country.Eswatini, "Eswatini" }, { Country.Benin, "Benin" }, { Country.Togo, "Togo" },
                // Territories / dependencies
                { Country.Anguilla, "Anguilla" }, { Country.Antarctica, "Antarctica" }, { Country.AmericanSamoa, "American Samoa" }, { Country.Aruba, "Aruba" }, { Country.AlandIslands, "Åland Islands" },
                { Country.SaintBarthelemy, "Saint Barthélemy" }, { Country.Bermuda, "Bermuda" }, { Country.BonaireSintEustatiusAndSaba, "Bonaire, Sint Eustatius and Saba" }, { Country.BouvetIsland, "Bouvet Island" },
                { Country.CocosIslands, "Cocos Islands" }, { Country.CookIslands, "Cook Islands" }, { Country.Curacao, "Curaçao" }, { Country.ChristmasIsland, "Christmas Island" }, { Country.WesternSahara, "Western Sahara" },
                { Country.FalklandIslands, "Falkland Islands" }, { Country.FaroeIslands, "Faroe Islands" }, { Country.FrenchGuiana, "French Guiana" }, { Country.Guernsey, "Guernsey" }, { Country.Gibraltar, "Gibraltar" },
                { Country.Guadeloupe, "Guadeloupe" }, { Country.SouthGeorgiaAndSouthSandwichIslands, "South Georgia and South Sandwich Islands" }, { Country.Guam, "Guam" }, { Country.HongKong, "Hong Kong" },
                { Country.HeardIslandAndMcDonaldIslands, "Heard Island and McDonald Islands" }, { Country.IsleOfMan, "Isle of Man" }, { Country.BritishIndianOceanTerritory, "British Indian Ocean Territory" },
                { Country.Jersey, "Jersey" }, { Country.CaymanIslands, "Cayman Islands" }, { Country.SaintMartin, "Saint Martin" }, { Country.Macao, "Macao" }, { Country.NorthernMarianaIslands, "Northern Mariana Islands" },
                { Country.Martinique, "Martinique" }, { Country.Montserrat, "Montserrat" }, { Country.NorfolkIsland, "Norfolk Island" }, { Country.Niue, "Niue" }, { Country.FrenchPolynesia, "French Polynesia" },
                { Country.SaintPierreAndMiquelon, "Saint Pierre and Miquelon" }, { Country.Pitcairn, "Pitcairn" }, { Country.PuertoRico, "Puerto Rico" }, { Country.Reunion, "Réunion" },
                { Country.SaintHelenaAscensionAndTristanDaCunha, "Saint Helena, Ascension and Tristan da Cunha" }, { Country.SvalbardAndJanMayen, "Svalbard and Jan Mayen" }, { Country.SintMaarten, "Sint Maarten" },
                { Country.TurksAndCaicosIslands, "Turks and Caicos Islands" }, { Country.FrenchSouthernTerritories, "French Southern Territories" }, { Country.Tokelau, "Tokelau" }, { Country.Taiwan, "Taiwan" },
                { Country.UnitedStatesMinorOutlyingIslands, "United States Minor Outlying Islands" }, { Country.VirginIslandsBritish, "Virgin Islands (British)" }, { Country.VirginIslandsUS, "Virgin Islands (U.S.)" },
                { Country.WallisAndFutuna, "Wallis and Futuna" }, { Country.Mayotte, "Mayotte" }
            };

            var countryContinentDict = new Dictionary<Country, Continent>
            {
                // FIXED: Complete Country -> Continent mapping (This was the main issue!)
                { Country.UnitedStates, Continent.NorthAmerica }, { Country.Canada, Continent.NorthAmerica }, { Country.Mexico, Continent.NorthAmerica }, { Country.Greenland, Continent.NorthAmerica },
                { Country.Guatemala, Continent.NorthAmerica }, { Country.Belize, Continent.NorthAmerica }, { Country.ElSalvador, Continent.NorthAmerica }, { Country.Honduras, Continent.NorthAmerica },
                { Country.Nicaragua, Continent.NorthAmerica }, { Country.CostaRica, Continent.NorthAmerica }, { Country.Panama, Continent.NorthAmerica }, { Country.Cuba, Continent.NorthAmerica },
                { Country.Jamaica, Continent.NorthAmerica }, { Country.Haiti, Continent.NorthAmerica }, { Country.DominicanRepublic, Continent.NorthAmerica },
                { Country.Brazil, Continent.SouthAmerica }, { Country.Argentina, Continent.SouthAmerica }, { Country.Chile, Continent.SouthAmerica }, { Country.Colombia, Continent.SouthAmerica },
                { Country.Venezuela, Continent.SouthAmerica }, { Country.Peru, Continent.SouthAmerica }, { Country.Ecuador, Continent.SouthAmerica }, { Country.Bolivia, Continent.SouthAmerica },
                { Country.Paraguay, Continent.SouthAmerica }, { Country.Uruguay, Continent.SouthAmerica }, { Country.Guyana, Continent.SouthAmerica }, { Country.Suriname, Continent.SouthAmerica },
                { Country.UnitedKingdom, Continent.Europe }, { Country.France, Continent.Europe }, { Country.Germany, Continent.Europe }, { Country.Italy, Continent.Europe }, { Country.Spain, Continent.Europe },
                { Country.Portugal, Continent.Europe }, { Country.Netherlands, Continent.Europe }, { Country.Belgium, Continent.Europe }, { Country.Switzerland, Continent.Europe }, { Country.Austria, Continent.Europe },
                { Country.Sweden, Continent.Europe }, { Country.Norway, Continent.Europe }, { Country.Finland, Continent.Europe }, { Country.Denmark, Continent.Europe }, { Country.Poland, Continent.Europe },
                { Country.Russia, Continent.Europe }, { Country.Ukraine, Continent.Europe }, { Country.Turkey, Continent.Europe }, { Country.Greece, Continent.Europe }, { Country.Romania, Continent.Europe },
                { Country.Bulgaria, Continent.Europe }, { Country.Hungary, Continent.Europe }, { Country.CzechRepublic, Continent.Europe }, { Country.Slovakia, Continent.Europe }, { Country.Slovenia, Continent.Europe },
                { Country.Croatia, Continent.Europe }, { Country.Serbia, Continent.Europe }, { Country.Montenegro, Continent.Europe }, { Country.BosniaAndHerzegovina, Continent.Europe }, { Country.NorthMacedonia, Continent.Europe },
                { Country.Albania, Continent.Europe }, { Country.Estonia, Continent.Europe }, { Country.Latvia, Continent.Europe }, { Country.Lithuania, Continent.Europe }, { Country.Belarus, Continent.Europe },
                { Country.Moldova, Continent.Europe }, { Country.Azerbaijan, Continent.Europe }, { Country.Iceland, Continent.Europe }, { Country.Ireland, Continent.Europe }, { Country.Georgia, Continent.Europe },
                { Country.Armenia, Continent.Asia }, { Country.Kosovo, Continent.Europe },
                { Country.China, Continent.Asia }, { Country.Japan, Continent.Asia }, { Country.SouthKorea, Continent.Asia }, { Country.NorthKorea, Continent.Asia }, { Country.India, Continent.Asia },
                { Country.Indonesia, Continent.Asia }, { Country.Vietnam, Continent.Asia }, { Country.Thailand, Continent.Asia }, { Country.Malaysia, Continent.Asia }, { Country.Singapore, Continent.Asia },
                { Country.Philippines, Continent.Asia }, { Country.Myanmar, Continent.Asia }, { Country.Cambodia, Continent.Asia }, { Country.Laos, Continent.Asia }, { Country.Bangladesh, Continent.Asia },
                { Country.Pakistan, Continent.Asia }, { Country.Afghanistan, Continent.Asia }, { Country.Nepal, Continent.Asia }, { Country.Bhutan, Continent.Asia }, { Country.SriLanka, Continent.Asia },
                { Country.Maldives, Continent.Asia }, { Country.Mongolia, Continent.Asia }, { Country.Kazakhstan, Continent.Asia }, { Country.Uzbekistan, Continent.Asia }, { Country.Turkmenistan, Continent.Asia },
                { Country.Kyrgyzstan, Continent.Asia }, { Country.Tajikistan, Continent.Asia }, { Country.TimorLeste, Continent.Asia },
                { Country.SaudiArabia, Continent.MiddleEast }, { Country.Iran, Continent.MiddleEast }, { Country.Iraq, Continent.MiddleEast }, { Country.Israel, Continent.MiddleEast }, { Country.Palestine, Continent.MiddleEast },
                { Country.Jordan, Continent.MiddleEast }, { Country.Lebanon, Continent.MiddleEast }, { Country.Syria, Continent.MiddleEast }, { Country.Yemen, Continent.MiddleEast }, { Country.Oman, Continent.MiddleEast },
                { Country.UAE, Continent.MiddleEast }, { Country.Qatar, Continent.MiddleEast }, { Country.Bahrain, Continent.MiddleEast }, { Country.Kuwait, Continent.MiddleEast },
                { Country.SouthAfrica, Continent.Africa }, { Country.Egypt, Continent.Africa }, { Country.Nigeria, Continent.Africa }, { Country.Kenya, Continent.Africa }, { Country.Ethiopia, Continent.Africa },
                { Country.Tanzania, Continent.Africa }, { Country.Uganda, Continent.Africa }, { Country.Rwanda, Continent.Africa }, { Country.Burundi, Continent.Africa }, { Country.DemocraticRepublicOfCongo, Continent.Africa },
                { Country.RepublicOfCongo, Continent.Africa }, { Country.CentralAfricanRepublic, Continent.Africa }, { Country.Chad, Continent.Africa }, { Country.Sudan, Continent.Africa }, { Country.SouthSudan, Continent.Africa },
                { Country.Libya, Continent.Africa }, { Country.Tunisia, Continent.Africa }, { Country.Algeria, Continent.Africa }, { Country.Morocco, Continent.Africa }, { Country.Ghana, Continent.Africa },
                { Country.IvoryCoast, Continent.Africa }, { Country.BurkinaFaso, Continent.Africa }, { Country.Mali, Continent.Africa }, { Country.Niger, Continent.Africa }, { Country.Senegal, Continent.Africa },
                { Country.Guinea, Continent.Africa }, { Country.SierraLeone, Continent.Africa }, { Country.Liberia, Continent.Africa }, { Country.Gambia, Continent.Africa }, { Country.GuineaBissau, Continent.Africa },
                { Country.Mauritania, Continent.Africa }, { Country.Madagascar, Continent.Africa }, { Country.Mozambique, Continent.Africa }, { Country.Zimbabwe, Continent.Africa }, { Country.Botswana, Continent.Africa },
                { Country.Namibia, Continent.Africa }, { Country.Zambia, Continent.Africa }, { Country.Malawi, Continent.Africa }, { Country.Angola, Continent.Africa }, { Country.Cameroon, Continent.Africa },
                { Country.EquatorialGuinea, Continent.Africa }, { Country.Gabon, Continent.Africa }, { Country.SaoTomeAndPrincipe, Continent.Africa }, { Country.Somalia, Continent.Africa },
                { Country.Australia, Continent.Oceania }, { Country.NewZealand, Continent.Oceania }, { Country.FijiIslands, Continent.Oceania }, { Country.PapuaNewGuinea, Continent.Oceania }, { Country.SolomonIslands, Continent.Oceania },
                { Country.Vanuatu, Continent.Oceania }, { Country.NewCaledonia, Continent.Oceania }, { Country.Samoa, Continent.Oceania }, { Country.Tonga, Continent.Oceania }, { Country.Palau, Continent.Oceania },
                { Country.MarshallIslands, Continent.Oceania }, { Country.Micronesia, Continent.Oceania }, { Country.Kiribati, Continent.Oceania }, { Country.Nauru, Continent.Oceania }, { Country.Tuvalu, Continent.Oceania },
                { Country.Cyprus, Continent.Europe }, { Country.Malta, Continent.Europe }, { Country.Luxembourg, Continent.Europe }, { Country.Monaco, Continent.Europe }, { Country.Liechtenstein, Continent.Europe },
                { Country.Andorra, Continent.Europe }, { Country.SanMarino, Continent.Europe }, { Country.VaticanCity, Continent.Europe }, { Country.Djibouti, Continent.Africa }, { Country.Eritrea, Continent.Africa },
                { Country.Comoros, Continent.Africa }, { Country.Seychelles, Continent.Africa }, { Country.Mauritius, Continent.Africa }, { Country.CaboVerde, Continent.Africa }, { Country.Barbados, Continent.NorthAmerica },
                { Country.Trinidad, Continent.NorthAmerica }, { Country.Grenada, Continent.NorthAmerica }, { Country.StLucia, Continent.NorthAmerica }, { Country.StVincent, Continent.NorthAmerica }, { Country.Dominica, Continent.NorthAmerica },
                { Country.AntiguaBarbuda, Continent.NorthAmerica }, { Country.StKittsNevis, Continent.NorthAmerica }, { Country.Bahamas, Continent.NorthAmerica }, { Country.Brunei, Continent.Asia }, { Country.Lesotho, Continent.Africa },
                { Country.Eswatini, Continent.Africa }, { Country.Benin, Continent.Africa }, { Country.Togo, Continent.Africa },
                { Country.Anguilla, Continent.NorthAmerica }, { Country.Antarctica, Continent.Antarctica }, { Country.AmericanSamoa, Continent.Oceania }, { Country.Aruba, Continent.NorthAmerica }, { Country.AlandIslands, Continent.Europe },
                { Country.SaintBarthelemy, Continent.NorthAmerica }, { Country.Bermuda, Continent.NorthAmerica }, { Country.BonaireSintEustatiusAndSaba, Continent.NorthAmerica }, { Country.BouvetIsland, Continent.Antarctica }, { Country.CocosIslands, Continent.Asia },
                { Country.CookIslands, Continent.Oceania }, { Country.Curacao, Continent.NorthAmerica }, { Country.ChristmasIsland, Continent.Asia }, { Country.WesternSahara, Continent.Africa }, { Country.FalklandIslands, Continent.SouthAmerica },
                { Country.FaroeIslands, Continent.Europe }, { Country.FrenchGuiana, Continent.SouthAmerica }, { Country.Guernsey, Continent.Europe }, { Country.Gibraltar, Continent.Europe }, { Country.Guadeloupe, Continent.NorthAmerica },
                { Country.SouthGeorgiaAndSouthSandwichIslands, Continent.Antarctica }, { Country.Guam, Continent.Oceania }, { Country.HongKong, Continent.Asia }, { Country.HeardIslandAndMcDonaldIslands, Continent.Antarctica }, { Country.IsleOfMan, Continent.Europe },
                { Country.BritishIndianOceanTerritory, Continent.Asia }, { Country.Jersey, Continent.Europe }, { Country.CaymanIslands, Continent.NorthAmerica }, { Country.SaintMartin, Continent.NorthAmerica }, { Country.Macao, Continent.Asia },
                { Country.NorthernMarianaIslands, Continent.Oceania }, { Country.Martinique, Continent.NorthAmerica }, { Country.Montserrat, Continent.NorthAmerica }, { Country.NorfolkIsland, Continent.Oceania }, { Country.Niue, Continent.Oceania },
                { Country.FrenchPolynesia, Continent.Oceania }, { Country.SaintPierreAndMiquelon, Continent.NorthAmerica }, { Country.Pitcairn, Continent.Oceania }, { Country.PuertoRico, Continent.NorthAmerica }, { Country.Reunion, Continent.Africa },
                { Country.SaintHelenaAscensionAndTristanDaCunha, Continent.Africa }, { Country.SvalbardAndJanMayen, Continent.Europe }, { Country.SintMaarten, Continent.NorthAmerica }, { Country.TurksAndCaicosIslands, Continent.NorthAmerica }, { Country.FrenchSouthernTerritories, Continent.Antarctica },
                { Country.Tokelau, Continent.Oceania }, { Country.Taiwan, Continent.Asia }, { Country.UnitedStatesMinorOutlyingIslands, Continent.Oceania }, { Country.VirginIslandsBritish, Continent.NorthAmerica }, { Country.VirginIslandsUS, Continent.NorthAmerica },
                { Country.WallisAndFutuna, Continent.Oceania }, { Country.Mayotte, Continent.Africa }
            };

            // Ensure completeness for any enum additions not covered above
            foreach (var c in Enum.GetValues<Country>())
            {
                if (!countryNameDict.ContainsKey(c))
                {
                    countryNameDict[c] = GenerateDisplayName(c.ToString());
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[MapDictionaries] Auto-added display name for {c}");
#endif
                }
                if (!countryContinentDict.ContainsKey(c))
                {
                    countryContinentDict[c] = Continent.Unknown;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[MapDictionaries] Auto-assigned Continent.Unknown for {c}");
#endif
                }
            }

            CountryToName = countryNameDict;
            CountryToContinent = countryContinentDict;

            ContinentToName = new Dictionary<Continent, string>
            {
                { Continent.Unknown, "Unknown" }, { Continent.Africa, "Africa" }, { Continent.Antarctica, "Antarctica" },
                { Continent.Asia, "Asia" }, { Continent.Europe, "Europe" }, { Continent.NorthAmerica, "North America" },
                { Continent.SouthAmerica, "South America" }, { Continent.Oceania, "Oceania" }, { Continent.MiddleEast, "Middle East" }
            };

            ContinentToCountries = CountryToContinent
                .Where(kv => kv.Key != Country.Unknown && kv.Value != Continent.Unknown)
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<Country>)g.OrderBy(c => CountryToName[c]).ToList());

            NormalizedNameToCountry = BuildNormalizedNameIndex();
        }

        #endregion Constructors

        #region Methods

        private static string GenerateDisplayName(string enumName)
        {
            // Split PascalCase and handle known abbreviations
            var name = Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");
            name = name.Replace("U S ", "US ")
                       .Replace("U A E", "UAE")
                       .Replace("D R C", "DRC")
                       .Replace("And And", "and");
            return name.Trim();
        }

        private static void AddAlias(IDictionary<string, Country> dict, string alias, Country c)
        {
            if (!dict.ContainsKey(alias))
                dict[alias] = c;
        }

        private static IReadOnlyDictionary<string, Country> BuildNormalizedNameIndex()
        {
            var dict = new Dictionary<string, Country>(StringComparer.OrdinalIgnoreCase);
            foreach (var (country, name) in CountryToName)
            {
                var norm = Normalize(name);
                if (!dict.ContainsKey(norm))
                    dict[norm] = country;
            }
            // Add extensive alternative spellings / aliases for common mismatches
            AddAlias(dict, "unitedstatesofamerica", Country.UnitedStates);
            AddAlias(dict, "usa", Country.UnitedStates);
            AddAlias(dict, "us", Country.UnitedStates);
            AddAlias(dict, "america", Country.UnitedStates);
            AddAlias(dict, "greatbritain", Country.UnitedKingdom);
            AddAlias(dict, "uk", Country.UnitedKingdom);
            AddAlias(dict, "britain", Country.UnitedKingdom);
            // Congo aliases - CRITICAL for JSON mapping
            AddAlias(dict, "democraticrepublicofthecongo", Country.DemocraticRepublicOfCongo);
            AddAlias(dict, "drc", Country.DemocraticRepublicOfCongo);
            AddAlias(dict, "congodemocraticrepublic", Country.DemocraticRepublicOfCongo);
            AddAlias(dict, "congokinshasa", Country.DemocraticRepublicOfCongo);
            AddAlias(dict, "zaire", Country.DemocraticRepublicOfCongo);
            AddAlias(dict, "congodemrep", Country.DemocraticRepublicOfCongo);
            AddAlias(dict, "demrepcongo", Country.DemocraticRepublicOfCongo);
            AddAlias(dict, "congodr", Country.DemocraticRepublicOfCongo);
            
            AddAlias(dict, "republicofthecongo", Country.RepublicOfCongo);
            AddAlias(dict, "congo", Country.RepublicOfCongo); // This is key - short "Congo" maps to Republic of Congo
            AddAlias(dict, "congobrazzaville", Country.RepublicOfCongo);
            AddAlias(dict, "congorep", Country.RepublicOfCongo);
            AddAlias(dict, "congorepublic", Country.RepublicOfCongo);

            // Central African Republic aliases
            AddAlias(dict, "centralafricanrepublic", Country.CentralAfricanRepublic);
            AddAlias(dict, "car", Country.CentralAfricanRepublic);
            AddAlias(dict, "centralafrep", Country.CentralAfricanRepublic);
            AddAlias(dict, "centralafrep", Country.CentralAfricanRepublic);

            // South Korea aliases
            AddAlias(dict, "southkorea", Country.SouthKorea);
            AddAlias(dict, "korea", Country.SouthKorea);
            AddAlias(dict, "republicofkorea", Country.SouthKorea);

            // North Korea aliases
            AddAlias(dict, "northkorea", Country.NorthKorea);
            AddAlias(dict, "dprk", Country.NorthKorea);
            AddAlias(dict, "democraticpeoplesrepublicofkorea", Country.NorthKorea);

            // Saudi Arabia aliases
            AddAlias(dict, "saudiarabia", Country.SaudiArabia);
            AddAlias(dict, "kingdomofsaudiarabia", Country.SaudiArabia);

            // New Zealand aliases
            AddAlias(dict, "newzealand", Country.NewZealand);
            AddAlias(dict, "nz", Country.NewZealand);

            // UAE aliases
            AddAlias(dict, "unitedarabemirates", Country.UAE);
            AddAlias(dict, "uae", Country.UAE);

            // Ivory Coast aliases
            AddAlias(dict, "ivorycoast", Country.IvoryCoast);
            AddAlias(dict, "cotedivoire", Country.IvoryCoast);

            // Sri Lanka aliases
            AddAlias(dict, "srilanka", Country.SriLanka);
            AddAlias(dict, "ceylon", Country.SriLanka);

            // Myanmar aliases
            AddAlias(dict, "myanmar", Country.Myanmar);
            AddAlias(dict, "burma", Country.Myanmar);

            // Czech Republic aliases
            AddAlias(dict, "czechrepublic", Country.CzechRepublic);
            AddAlias(dict, "czechia", Country.CzechRepublic);

            // Sudan aliases
            AddAlias(dict, "sudanrepublicof", Country.Sudan);
            AddAlias(dict, "republicofthesudan", Country.Sudan);
            AddAlias(dict, "republicofsudan", Country.Sudan);

            // South Sudan aliases
            AddAlias(dict, "burkinafaso", Country.BurkinaFaso);
            AddAlias(dict, "guineabissau", Country.GuineaBissau);
            AddAlias(dict, "sierraleone", Country.SierraLeone);
            AddAlias(dict, "equatorialguinea", Country.EquatorialGuinea);
            AddAlias(dict, "southsudan", Country.SouthSudan);
            AddAlias(dict, "republicofsouthsudan", Country.SouthSudan);
            AddAlias(dict, "ssouth", Country.SouthSudan);
            AddAlias(dict, "ssudan", Country.SouthSudan);

            // Timor-Leste aliases
            AddAlias(dict, "timorleste", Country.TimorLeste);
            AddAlias(dict, "easttimor", Country.TimorLeste);
            AddAlias(dict, "democraticrepublicoftimor", Country.TimorLeste);

            // Georgia aliases
            AddAlias(dict, "republicofgeorgia", Country.Georgia);
            
            // Armenia aliases
            AddAlias(dict, "republicofarmenia", Country.Armenia);
            
            // Somalia aliases
            AddAlias(dict, "somalirepublic", Country.Somalia);
            AddAlias(dict, "federalrepublicsomaliaof", Country.Somalia);

            // Kosovo alias
            AddAlias(dict, "kosovo", Country.Kosovo);

            // Additional aliases for missing countries from debug output
            AddAlias(dict, "eswatini", Country.Eswatini);
            AddAlias(dict, "swaziland", Country.Eswatini);
            AddAlias(dict, "lesotho", Country.Lesotho);
            AddAlias(dict, "benin", Country.Benin);
            AddAlias(dict, "togo", Country.Togo);
            AddAlias(dict, "djibouti", Country.Djibouti);
            AddAlias(dict, "eritrea", Country.Eritrea);
            AddAlias(dict, "comoros", Country.Comoros);
            AddAlias(dict, "seychelles", Country.Seychelles);
            AddAlias(dict, "mauritius", Country.Mauritius);
            AddAlias(dict, "brunei", Country.Brunei);
            AddAlias(dict, "bruneidarussalam", Country.Brunei);
            AddAlias(dict, "bahamas", Country.Bahamas);
            AddAlias(dict, "thebahamas", Country.Bahamas);
            AddAlias(dict, "barbados", Country.Barbados);
            AddAlias(dict, "grenada", Country.Grenada);
            AddAlias(dict, "dominica", Country.Dominica);
            
            // European countries
            AddAlias(dict, "northmacedonia", Country.NorthMacedonia);
            AddAlias(dict, "macedonia", Country.NorthMacedonia);
            AddAlias(dict, "fyrom", Country.NorthMacedonia);
            AddAlias(dict, "czechia", Country.CzechRepublic);
            AddAlias(dict, "czechrepublic", Country.CzechRepublic);
            AddAlias(dict, "bosniaandherzegovina", Country.BosniaAndHerzegovina);
            AddAlias(dict, "bosniaherzegovina", Country.BosniaAndHerzegovina);
            AddAlias(dict, "bih", Country.BosniaAndHerzegovina);
            
            // Asian countries  
            AddAlias(dict, "laopdr", Country.Laos);
            AddAlias(dict, "laopeoplesdemocraticrepublic", Country.Laos);
            AddAlias(dict, "timerleste", Country.TimorLeste);
            AddAlias(dict, "easttimor", Country.TimorLeste);
            
            // Small states
            AddAlias(dict, "vatican", Country.VaticanCity);
            AddAlias(dict, "vaticancitystate", Country.VaticanCity);
            AddAlias(dict, "holysee", Country.VaticanCity);
            AddAlias(dict, "sanmarino", Country.SanMarino);
            AddAlias(dict, "republicofsanmarino", Country.SanMarino);

            return dict;
        }

        private static string Normalize(string value) => new string(value.Where(ch => char.IsLetterOrDigit(ch)).ToArray()).ToLowerInvariant();

        #endregion Methods
    }
}