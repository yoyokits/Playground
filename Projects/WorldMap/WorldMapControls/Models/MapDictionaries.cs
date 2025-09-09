// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Centralized dictionaries for geographic lookups.
    /// </summary>
    public static class MapDictionaries
    {
        #region Fields

        // 3. Continent -> Countries (list)
        public static readonly IReadOnlyDictionary<Continent, IReadOnlyList<Country>> ContinentToCountries;

        // 4. Continent -> Continent display name
        public static readonly IReadOnlyDictionary<Continent, string> ContinentToName;

        // 1. Country -> Continent
        public static readonly IReadOnlyDictionary<Country, Continent> CountryToContinent;

        // 2. Country -> Country display name
        public static readonly IReadOnlyDictionary<Country, string> CountryToName;

        // 5. Normalized country name (lower, stripped) -> Country (used for parsing)
        public static readonly IReadOnlyDictionary<string, Country> NormalizedNameToCountry;

        #endregion Fields

        #region Constructors

        static MapDictionaries()
        {
            // Country display names (authoritative source for naming)
            CountryToName = new Dictionary<Country, string>
            {
                { Country.Unknown, "Unknown" },
                { Country.UnitedStates, "United States" },
                { Country.Canada, "Canada" },
                { Country.UnitedKingdom, "United Kingdom" },
                { Country.France, "France" },
                { Country.Germany, "Germany" },
                { Country.Italy, "Italy" },
                { Country.Spain, "Spain" },
                { Country.Portugal, "Portugal" },
                { Country.Netherlands, "Netherlands" },
                { Country.Belgium, "Belgium" },
                { Country.Switzerland, "Switzerland" },
                { Country.Austria, "Austria" },
                { Country.Sweden, "Sweden" },
                { Country.Norway, "Norway" },
                { Country.Finland, "Finland" },
                { Country.Denmark, "Denmark" },
                { Country.Poland, "Poland" },
                { Country.Russia, "Russia" },
                { Country.Ukraine, "Ukraine" },
                { Country.Turkey, "Turkey" },          // Geopolitically bridging Europe / Asia; left unmapped to MiddleEast for flexibility
                { Country.China, "China" },
                { Country.Japan, "Japan" },
                { Country.SouthKorea, "South Korea" },
                { Country.India, "India" },
                { Country.Indonesia, "Indonesia" },
                { Country.Vietnam, "Vietnam" },
                { Country.Thailand, "Thailand" },
                { Country.Australia, "Australia" },
                { Country.NewZealand, "New Zealand" },
                { Country.Brazil, "Brazil" },
                { Country.Argentina, "Argentina" },
                { Country.Chile, "Chile" },
                { Country.Mexico, "Mexico" },
                { Country.SouthAfrica, "South Africa" },
                { Country.Egypt, "Egypt" },
                { Country.Nigeria, "Nigeria" },
                { Country.Kenya, "Kenya" },
                { Country.SaudiArabia, "Saudi Arabia" },
                { Country.Iran, "Iran" }
            };

            // Country -> Continent
            CountryToContinent = new Dictionary<Country, Continent>
            {
                { Country.UnitedStates, Continent.NorthAmerica },
                { Country.Canada, Continent.NorthAmerica },
                { Country.Mexico, Continent.NorthAmerica },

                { Country.Brazil, Continent.SouthAmerica },
                { Country.Argentina, Continent.SouthAmerica },
                { Country.Chile, Continent.SouthAmerica },

                { Country.UnitedKingdom, Continent.Europe },
                { Country.France, Continent.Europe },
                { Country.Germany, Continent.Europe },
                { Country.Italy, Continent.Europe },
                { Country.Spain, Continent.Europe },
                { Country.Portugal, Continent.Europe },
                { Country.Netherlands, Continent.Europe },
                { Country.Belgium, Continent.Europe },
                { Country.Switzerland, Continent.Europe },
                { Country.Austria, Continent.Europe },
                { Country.Sweden, Continent.Europe },
                { Country.Norway, Continent.Europe },
                { Country.Finland, Continent.Europe },
                { Country.Denmark, Continent.Europe },
                { Country.Poland, Continent.Europe },
                { Country.Ukraine, Continent.Europe },
                { Country.Russia, Continent.Europe }, // Could also be partially Asia
                { Country.Turkey, Continent.Europe }, // Simplified; part Asia

                { Country.China, Continent.Asia },
                { Country.Japan, Continent.Asia },
                { Country.SouthKorea, Continent.Asia },
                { Country.India, Continent.Asia },
                { Country.Indonesia, Continent.Asia },
                { Country.Vietnam, Continent.Asia },
                { Country.Thailand, Continent.Asia },
                { Country.Iran, Continent.MiddleEast },
                { Country.SaudiArabia, Continent.MiddleEast },

                { Country.Australia, Continent.Oceania },
                { Country.NewZealand, Continent.Oceania },

                { Country.SouthAfrica, Continent.Africa },
                { Country.Egypt, Continent.Africa },
                { Country.Nigeria, Continent.Africa },
                { Country.Kenya, Continent.Africa },

                { Country.Unknown, Continent.Unknown }
            };

            // Continent display names
            ContinentToName = new Dictionary<Continent, string>
            {
                { Continent.Unknown, "Unknown" },
                { Continent.Africa, "Africa" },
                { Continent.Antarctica, "Antarctica" },
                { Continent.Asia, "Asia" },
                { Continent.Europe, "Europe" },
                { Continent.NorthAmerica, "North America" },
                { Continent.SouthAmerica, "South America" },
                { Continent.Oceania, "Oceania" },
                { Continent.MiddleEast, "Middle East" }
            };

            // Continent -> Countries
            ContinentToCountries = CountryToContinent
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<Country>)g.OrderBy(c => CountryToName[c]).ToList());

            // Normalized name mapping
            NormalizedNameToCountry = BuildNormalizedNameIndex();
        }

        #endregion Constructors

        #region Methods

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

            // Add alternative spellings / aliases
            AddAlias(dict, "unitedstatesofamerica", Country.UnitedStates);
            AddAlias(dict, "usa", Country.UnitedStates);
            AddAlias(dict, "us", Country.UnitedStates);
            AddAlias(dict, "greatbritain", Country.UnitedKingdom);
            AddAlias(dict, "uk", Country.UnitedKingdom);
            AddAlias(dict, "southkorea", Country.SouthKorea);
            AddAlias(dict, "korea", Country.SouthKorea);
            AddAlias(dict, "saudiarabia", Country.SaudiArabia);
            AddAlias(dict, "newzealand", Country.NewZealand);

            return dict;
        }

        private static string Normalize(string value) =>
            new string(value.Where(ch => char.IsLetterOrDigit(ch)).ToArray()).ToLowerInvariant();

        #endregion Methods
    }
}