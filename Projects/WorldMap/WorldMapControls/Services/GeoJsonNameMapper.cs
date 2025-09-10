// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Specialized mapper for handling shortened country names commonly found in GeoJSON files.
    /// This addresses the issue where GeoJSON uses abbreviated names like "Congo" instead of full names.
    /// </summary>
    public static class GeoJsonNameMapper
    {
        /// <summary>
        /// Maps commonly shortened GeoJSON country names to their full Country enum values.
        /// This is the primary fix for countries not being colored due to name mismatches.
        /// </summary>
        private static readonly Dictionary<string, Country> JsonNameToCountry = new(StringComparer.OrdinalIgnoreCase)
        {
            // CONGO VARIATIONS - Critical fixes
            ["Congo"] = Country.RepublicOfCongo,
            ["Congo, Rep."] = Country.RepublicOfCongo,
            ["Congo Republic"] = Country.RepublicOfCongo,
            ["Congo-Brazzaville"] = Country.RepublicOfCongo,
            ["Republic of the Congo"] = Country.RepublicOfCongo,
            
            ["Congo, Dem. Rep."] = Country.DemocraticRepublicOfCongo,
            ["Dem. Rep. Congo"] = Country.DemocraticRepublicOfCongo, // Added this specific variation
            ["Democratic Rep. Congo"] = Country.DemocraticRepublicOfCongo,
            ["Congo DR"] = Country.DemocraticRepublicOfCongo,
            ["Congo DRC"] = Country.DemocraticRepublicOfCongo,
            ["Congo-Kinshasa"] = Country.DemocraticRepublicOfCongo,
            ["Democratic Republic of the Congo"] = Country.DemocraticRepublicOfCongo,
            ["Zaire"] = Country.DemocraticRepublicOfCongo,
            ["DRC"] = Country.DemocraticRepublicOfCongo,

            // CENTRAL AFRICAN REPUBLIC VARIATIONS
            ["Central African Rep."] = Country.CentralAfricanRepublic,
            ["Central African Republic"] = Country.CentralAfricanRepublic,
            ["CAR"] = Country.CentralAfricanRepublic,

            // SOUTH SUDAN VARIATIONS
            ["S. Sudan"] = Country.SouthSudan,
            ["South Sudan"] = Country.SouthSudan,
            ["S Sudan"] = Country.SouthSudan,
            ["Republic of South Sudan"] = Country.SouthSudan,

            // SUDAN VARIATIONS
            ["Sudan"] = Country.Sudan,
            ["Republic of Sudan"] = Country.Sudan,
            ["Republic of the Sudan"] = Country.Sudan,

            // GEORGIA AND ARMENIA
            ["Georgia"] = Country.Georgia,
            ["Republic of Georgia"] = Country.Georgia,
            ["Armenia"] = Country.Armenia,
            ["Republic of Armenia"] = Country.Armenia,

            // TURKMENISTAN
            ["Turkmenistan"] = Country.Turkmenistan,
            ["Turkmen"] = Country.Turkmenistan,

            // SOMALIA
            ["Somalia"] = Country.Somalia,
            ["Somali Republic"] = Country.Somalia,
            ["Federal Republic of Somalia"] = Country.Somalia,

            // ICELAND AND IRELAND
            ["Iceland"] = Country.Iceland,
            ["Republic of Iceland"] = Country.Iceland,
            ["Ireland"] = Country.Ireland,
            ["Republic of Ireland"] = Country.Ireland,
            ["Éire"] = Country.Ireland,

            // TIMOR-LESTE
            ["Timor-Leste"] = Country.TimorLeste,
            ["East Timor"] = Country.TimorLeste,
            ["Democratic Republic of Timor-Leste"] = Country.TimorLeste,

            // AFRICAN COUNTRIES WITH COMMON VARIATIONS
            ["Ivory Coast"] = Country.IvoryCoast,
            ["Côte d'Ivoire"] = Country.IvoryCoast,
            ["Cote d'Ivoire"] = Country.IvoryCoast,
            ["Burkina Faso"] = Country.BurkinaFaso,
            ["Guinea-Bissau"] = Country.GuineaBissau,
            ["Guinea Bissau"] = Country.GuineaBissau,
            ["Sierra Leone"] = Country.SierraLeone,
            ["Equatorial Guinea"] = Country.EquatorialGuinea,
            ["Sao Tome and Principe"] = Country.SaoTomeAndPrincipe,
            ["São Tomé and Príncipe"] = Country.SaoTomeAndPrincipe,
            ["Cape Verde"] = Country.CaboVerde,
            ["Cabo Verde"] = Country.CaboVerde,

            // MIDDLE EASTERN COUNTRIES
            ["United Arab Emirates"] = Country.UAE,
            ["UAE"] = Country.UAE,
            ["Saudi Arabia"] = Country.SaudiArabia,
            ["Kingdom of Saudi Arabia"] = Country.SaudiArabia,

            // ASIAN COUNTRIES
            ["Myanmar"] = Country.Myanmar,
            ["Burma"] = Country.Myanmar,
            ["Sri Lanka"] = Country.SriLanka,
            ["Ceylon"] = Country.SriLanka,
            ["North Korea"] = Country.NorthKorea,
            ["DPRK"] = Country.NorthKorea,
            ["Democratic People's Republic of Korea"] = Country.NorthKorea,
            ["South Korea"] = Country.SouthKorea,
            ["Republic of Korea"] = Country.SouthKorea,
            ["Korea"] = Country.SouthKorea, // Default Korea to South Korea

            // EUROPEAN COUNTRIES
            ["Czech Republic"] = Country.CzechRepublic,
            ["Czechia"] = Country.CzechRepublic,
            ["Bosnia and Herzegovina"] = Country.BosniaAndHerzegovina,
            ["Bosnia & Herzegovina"] = Country.BosniaAndHerzegovina,
            ["BiH"] = Country.BosniaAndHerzegovina,
            ["North Macedonia"] = Country.NorthMacedonia,
            ["Macedonia"] = Country.NorthMacedonia,
            ["FYROM"] = Country.NorthMacedonia,

            // SMALL ISLAND STATES AND TERRITORIES
            ["Trinidad and Tobago"] = Country.Trinidad,
            ["Trinidad & Tobago"] = Country.Trinidad,
            ["Saint Lucia"] = Country.StLucia,
            ["St. Lucia"] = Country.StLucia,
            ["Saint Vincent and the Grenadines"] = Country.StVincent,
            ["St. Vincent and the Grenadines"] = Country.StVincent,
            ["Saint Kitts and Nevis"] = Country.StKittsNevis,
            ["St. Kitts and Nevis"] = Country.StKittsNevis,
            ["Antigua and Barbuda"] = Country.AntiguaBarbuda,

            // EUROPEAN MICROSTATES
            ["San Marino"] = Country.SanMarino,
            ["Vatican City"] = Country.VaticanCity,
            ["Holy See"] = Country.VaticanCity,

            // AMERICAS
            ["United States"] = Country.UnitedStates,
            ["United States of America"] = Country.UnitedStates,
            ["USA"] = Country.UnitedStates,
            ["US"] = Country.UnitedStates,
            ["America"] = Country.UnitedStates,
            ["United Kingdom"] = Country.UnitedKingdom,
            ["UK"] = Country.UnitedKingdom,
            ["Great Britain"] = Country.UnitedKingdom,
            ["Britain"] = Country.UnitedKingdom,

            // OCEANIA
            ["New Zealand"] = Country.NewZealand,
            ["NZ"] = Country.NewZealand,
            ["Papua New Guinea"] = Country.PapuaNewGuinea,
            ["PNG"] = Country.PapuaNewGuinea,
            ["Solomon Islands"] = Country.SolomonIslands,
            ["Marshall Islands"] = Country.MarshallIslands,
            ["Fiji"] = Country.FijiIslands,

            // ADDITIONAL MISSING COUNTRIES FROM DEBUG OUTPUT
            ["Lesotho"] = Country.Lesotho,
            ["Eswatini"] = Country.Eswatini,
            ["Swaziland"] = Country.Eswatini, // Alternative name
            ["Benin"] = Country.Benin,
            ["Togo"] = Country.Togo,
            
            // European microstates and small countries
            ["Cyprus"] = Country.Cyprus,
            ["Malta"] = Country.Malta,
            ["Luxembourg"] = Country.Luxembourg,
            ["Monaco"] = Country.Monaco,
            ["Liechtenstein"] = Country.Liechtenstein,
            ["Andorra"] = Country.Andorra,
            ["San Marino"] = Country.SanMarino,
            ["Vatican City"] = Country.VaticanCity,
            ["Holy See"] = Country.VaticanCity,
            
            // African countries
            ["Djibouti"] = Country.Djibouti,
            ["Eritrea"] = Country.Eritrea,
            ["Comoros"] = Country.Comoros,
            ["Seychelles"] = Country.Seychelles,
            ["Mauritius"] = Country.Mauritius,
            ["Cabo Verde"] = Country.CaboVerde,
            ["Cape Verde"] = Country.CaboVerde,
            
            // Caribbean countries
            ["Barbados"] = Country.Barbados,
            ["Trinidad and Tobago"] = Country.Trinidad,
            ["Grenada"] = Country.Grenada,
            ["Saint Lucia"] = Country.StLucia,
            ["St. Lucia"] = Country.StLucia,
            ["Saint Vincent and the Grenadines"] = Country.StVincent,
            ["St. Vincent and the Grenadines"] = Country.StVincent,
            ["Dominica"] = Country.Dominica,
            ["Antigua and Barbuda"] = Country.AntiguaBarbuda,
            ["Saint Kitts and Nevis"] = Country.StKittsNevis,
            ["St. Kitts and Nevis"] = Country.StKittsNevis,
            ["Bahamas"] = Country.Bahamas,
            ["The Bahamas"] = Country.Bahamas,
            
            // Asian countries
            ["Brunei"] = Country.Brunei,
            ["Brunei Darussalam"] = Country.Brunei,
            
            // Additional countries that commonly appear unmapped in debug output
            ["Lao PDR"] = Country.Laos,
            ["Lao People's Democratic Republic"] = Country.Laos,
            ["Laos"] = Country.Laos,
            
            // Bosnia variants
            ["Bosnia and Herzegovina"] = Country.BosniaAndHerzegovina,
            ["Bosnia & Herzegovina"] = Country.BosniaAndHerzegovina,
            ["BiH"] = Country.BosniaAndHerzegovina,
            
            // Macedonia variants
            ["Macedonia"] = Country.NorthMacedonia,
            ["North Macedonia"] = Country.NorthMacedonia,
            ["FYROM"] = Country.NorthMacedonia,
            ["The former Yugoslav Republic of Macedonia"] = Country.NorthMacedonia,
            
            // Czech variants
            ["Czech Republic"] = Country.CzechRepublic,
            ["Czechia"] = Country.CzechRepublic,
            
            // Vatican variants
            ["Vatican"] = Country.VaticanCity,
            ["Vatican City State"] = Country.VaticanCity,
            ["Holy See (Vatican City State)"] = Country.VaticanCity,
            
            // Small island states that often appear unmapped
            ["São Tomé and Príncipe"] = Country.SaoTomeAndPrincipe,
            ["Sao Tome & Principe"] = Country.SaoTomeAndPrincipe,
            
            // Former country names
            ["Swaziland"] = Country.Eswatini,
            ["Kingdom of Swaziland"] = Country.Eswatini,
            
            // Additional African country variants
            ["Central African Republic"] = Country.CentralAfricanRepublic,
            ["CAR"] = Country.CentralAfricanRepublic,
            
            // European country variants
            ["Republic of Belarus"] = Country.Belarus,
            ["Republic of Moldova"] = Country.Moldova,
            ["Republic of Estonia"] = Country.Estonia,
            ["Republic of Latvia"] = Country.Latvia,
            ["Republic of Lithuania"] = Country.Lithuania
        };

        /// <summary>
        /// Attempts to map a GeoJSON country name to a Country enum value.
        /// This method handles the shortened names commonly found in GeoJSON files.
        /// </summary>
        /// <param name="geoJsonName">The country name as it appears in the GeoJSON file</param>
        /// <returns>The corresponding Country enum value, or Country.Unknown if no match found</returns>
        public static Country MapGeoJsonNameToCountry(string geoJsonName)
        {
            if (string.IsNullOrWhiteSpace(geoJsonName))
                return Country.Unknown;

            // Direct lookup first
            if (JsonNameToCountry.TryGetValue(geoJsonName, out var country))
                return country;

            // Fallback to normalized lookup (remove spaces, special chars, lowercase)
            var normalized = NormalizeName(geoJsonName);
            var match = JsonNameToCountry.FirstOrDefault(kvp => 
                NormalizeName(kvp.Key) == normalized);
            
            return match.Key != null ? match.Value : Country.Unknown;
        }

        /// <summary>
        /// Gets all known GeoJSON name variations for a given country.
        /// Useful for testing and validation.
        /// </summary>
        /// <param name="country">The country to get variations for</param>
        /// <returns>List of GeoJSON names that map to this country</returns>
        public static List<string> GetGeoJsonVariations(Country country)
        {
            return JsonNameToCountry
                .Where(kvp => kvp.Value == country)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Normalizes a country name for comparison by removing spaces, 
        /// special characters, and converting to lowercase.
        /// </summary>
        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return new string(name
                .Where(c => char.IsLetterOrDigit(c))
                .ToArray())
                .ToLowerInvariant();
        }

        /// <summary>
        /// Validates that all countries in the mapping have corresponding Country enum values.
        /// Returns any countries that are mapped to invalid enum values.
        /// </summary>
        public static List<string> ValidateMappings()
        {
            var invalid = new List<string>();
            foreach (var kvp in JsonNameToCountry)
            {
                if (kvp.Value == Country.Unknown)
                    invalid.Add(kvp.Key);
            }
            return invalid;
        }
    }
}