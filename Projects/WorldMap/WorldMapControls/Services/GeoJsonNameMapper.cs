// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.Generic;
using System.Linq;
using WorldMapControls.Extensions;
using WorldMapControls.Models;
using WorldMapControls.Models.Enums;

namespace WorldMapControls.Services
{
    /// <summary>
    /// Specialized mapper for handling shortened country names commonly found in GeoJSON files.
    /// This addresses the issue where GeoJSON uses abbreviated or alternate names that do not match the internal enums directly.
    /// </summary>
    public static class GeoJsonNameMapper
    {
        /// <summary>
        /// Maps commonly shortened GeoJSON country names to their full Country enum values.
        /// </summary>
        private static readonly Dictionary<string, Country> JsonNameToCountry = new(StringComparer.OrdinalIgnoreCase)
        {
            // BASIC STANDARD COUNTRY NAMES - Essential for mapping
            ["Germany"] = Country.Germany,
            ["France"] = Country.France,
            ["Italy"] = Country.Italy,
            ["Spain"] = Country.Spain,
            ["Japan"] = Country.Japan,
            ["Brazil"] = Country.Brazil,
            ["China"] = Country.China,
            ["India"] = Country.India,
            ["Russia"] = Country.Russia,
            ["Russian Federation"] = Country.Russia,
            ["Russian Fed."] = Country.Russia,
            ["Canada"] = Country.Canada,
            ["Australia"] = Country.Australia,
            ["Mexico"] = Country.Mexico,
            ["Argentina"] = Country.Argentina,
            ["Chile"] = Country.Chile,
            ["Colombia"] = Country.Colombia,
            ["Peru"] = Country.Peru,
            ["Venezuela"] = Country.Venezuela,
            ["Poland"] = Country.Poland,
            ["Ukraine"] = Country.Ukraine,
            ["Turkey"] = Country.Turkey,
            ["Greece"] = Country.Greece,
            ["Portugal"] = Country.Portugal,
            ["Belgium"] = Country.Belgium,
            ["Netherlands"] = Country.Netherlands,
            ["Switzerland"] = Country.Switzerland,
            ["Austria"] = Country.Austria,
            ["Sweden"] = Country.Sweden,
            ["Norway"] = Country.Norway,
            ["Finland"] = Country.Finland,
            ["Denmark"] = Country.Denmark,
            ["South Africa"] = Country.SouthAfrica,
            ["Egypt"] = Country.Egypt,
            ["Nigeria"] = Country.Nigeria,
            ["Kenya"] = Country.Kenya,
            ["Ethiopia"] = Country.Ethiopia,
            ["Morocco"] = Country.Morocco,
            ["Algeria"] = Country.Algeria,
            ["Tunisia"] = Country.Tunisia,
            ["Libya"] = Country.Libya,
            ["Ghana"] = Country.Ghana,
            ["Senegal"] = Country.Senegal,
            ["Mali"] = Country.Mali,
            ["Niger"] = Country.Niger,
            ["Chad"] = Country.Chad,
            ["Cameroon"] = Country.Cameroon,
            ["Angola"] = Country.Angola,
            ["Mozambique"] = Country.Mozambique,
            ["Zambia"] = Country.Zambia,
            ["Zimbabwe"] = Country.Zimbabwe,
            ["Botswana"] = Country.Botswana,
            ["Namibia"] = Country.Namibia,
            ["Madagascar"] = Country.Madagascar,
            ["Thailand"] = Country.Thailand,
            ["Vietnam"] = Country.Vietnam,
            ["Indonesia"] = Country.Indonesia,
            ["Malaysia"] = Country.Malaysia,
            ["Philippines"] = Country.Philippines,
            ["Singapore"] = Country.Singapore,
            ["Bangladesh"] = Country.Bangladesh,
            ["Pakistan"] = Country.Pakistan,
            ["Afghanistan"] = Country.Afghanistan,
            ["Nepal"] = Country.Nepal,
            ["Iran"] = Country.Iran,
            ["Iraq"] = Country.Iraq,
            ["Israel"] = Country.Israel,
            ["Jordan"] = Country.Jordan,
            ["Lebanon"] = Country.Lebanon,
            ["Syria"] = Country.Syria,
            ["Syrian Arab Republic"] = Country.Syria,
            ["Iran (Islamic Republic of)"] = Country.Iran,
            ["Venezuela (Bolivarian Republic of)"] = Country.Venezuela,
            ["Bolivia (Plurinational State of)"] = Country.Bolivia,
            ["Tanzania, United Republic of"] = Country.Tanzania,
            ["Korea, Republic of"] = Country.SouthKorea,
            ["Korea Rep."] = Country.SouthKorea,
            ["Korea, Dem. People's Rep. of"] = Country.NorthKorea,
            ["Korea DPR"] = Country.NorthKorea,
            ["Lao People's Democratic Republic"] = Country.Laos,

            // CONGO VARIATIONS
            ["Congo"] = Country.RepublicOfCongo,
            ["Congo, Rep."] = Country.RepublicOfCongo,
            ["Congo Republic"] = Country.RepublicOfCongo,
            ["Congo-Brazzaville"] = Country.RepublicOfCongo,
            ["Republic of the Congo"] = Country.RepublicOfCongo,
            ["Congo, Dem. Rep."] = Country.DemocraticRepublicOfCongo,
            ["Dem. Rep. Congo"] = Country.DemocraticRepublicOfCongo,
            ["Democratic Rep. Congo"] = Country.DemocraticRepublicOfCongo,
            ["Congo DR"] = Country.DemocraticRepublicOfCongo,
            ["Congo DRC"] = Country.DemocraticRepublicOfCongo,
            ["Congo-Kinshasa"] = Country.DemocraticRepublicOfCongo,
            ["Democratic Republic of the Congo"] = Country.DemocraticRepublicOfCongo,
            ["Zaire"] = Country.DemocraticRepublicOfCongo,
            ["DRC"] = Country.DemocraticRepublicOfCongo,

            // CENTRAL AFRICAN REPUBLIC
            ["Central African Rep."] = Country.CentralAfricanRepublic,
            ["Central African Republic"] = Country.CentralAfricanRepublic,
            ["CAR"] = Country.CentralAfricanRepublic,

            // SOUTH SUDAN
            ["S. Sudan"] = Country.SouthSudan,
            ["South Sudan"] = Country.SouthSudan,
            ["S Sudan"] = Country.SouthSudan,
            ["Republic of South Sudan"] = Country.SouthSudan,

            // SUDAN
            ["Sudan"] = Country.Sudan,
            ["Republic of Sudan"] = Country.Sudan,
            ["Republic of the Sudan"] = Country.Sudan,

            // GEORGIA / ARMENIA
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

            // ICELAND / IRELAND
            ["Iceland"] = Country.Iceland,
            ["Republic of Iceland"] = Country.Iceland,
            ["Ireland"] = Country.Ireland,
            ["Republic of Ireland"] = Country.Ireland,
            ["Éire"] = Country.Ireland,

            // TIMOR-LESTE
            ["Timor-Leste"] = Country.TimorLeste,
            ["East Timor"] = Country.TimorLeste,
            ["Democratic Republic of Timor-Leste"] = Country.TimorLeste,

            // AFRICAN VARIATIONS
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
            ["Western Sahara"] = Country.WesternSahara,

            // MIDDLE EAST
            ["United Arab Emirates"] = Country.UAE,
            ["UAE"] = Country.UAE,
            ["Saudi Arabia"] = Country.SaudiArabia,
            ["Kingdom of Saudi Arabia"] = Country.SaudiArabia,

            // ASIA
            ["Myanmar"] = Country.Myanmar,
            ["Burma"] = Country.Myanmar,
            ["Sri Lanka"] = Country.SriLanka,
            ["Ceylon"] = Country.SriLanka,
            ["North Korea"] = Country.NorthKorea,
            ["DPRK"] = Country.NorthKorea,
            ["Democratic People's Republic of Korea"] = Country.NorthKorea,
            ["South Korea"] = Country.SouthKorea,
            ["Republic of Korea"] = Country.SouthKorea,
            ["Korea"] = Country.SouthKorea,

            // EUROPEAN SPECIALS
            ["Czech Republic"] = Country.CzechRepublic,
            ["Czechia"] = Country.CzechRepublic,
            ["Bosnia and Herzegovina"] = Country.BosniaAndHerzegovina,
            ["Bosnia & Herzegovina"] = Country.BosniaAndHerzegovina,
            ["BiH"] = Country.BosniaAndHerzegovina,
            ["North Macedonia"] = Country.NorthMacedonia,
            ["Macedonia"] = Country.NorthMacedonia,
            ["FYROM"] = Country.NorthMacedonia,

            // SMALL ISLAND STATES & TERRITORIES
            ["Trinidad and Tobago"] = Country.Trinidad,
            ["Trinidad & Tobago"] = Country.Trinidad,
            ["Saint Lucia"] = Country.StLucia,
            ["St. Lucia"] = Country.StLucia,
            ["Saint Vincent and the Grenadines"] = Country.StVincent,
            ["St. Vincent and the Grenadines"] = Country.StVincent,
            ["Saint Kitts and Nevis"] = Country.StKittsNevis,
            ["St. Kitts and Nevis"] = Country.StKittsNevis,
            ["Antigua and Barbuda"] = Country.AntiguaBarbuda,
            ["Grenada"] = Country.Grenada,
            ["Dominica"] = Country.Dominica,
            ["Barbados"] = Country.Barbados,
            ["Bahamas"] = Country.Bahamas,
            ["The Bahamas"] = Country.Bahamas,

            // MICROSTATES
            ["San Marino"] = Country.SanMarino,
            ["Vatican City"] = Country.VaticanCity,
            ["Holy See"] = Country.VaticanCity,
            ["Vatican"] = Country.VaticanCity,

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

            // ADDITIONAL FROM DEBUG OUTPUT
            ["Lesotho"] = Country.Lesotho,
            ["Eswatini"] = Country.Eswatini,
            ["Swaziland"] = Country.Eswatini,
            ["Benin"] = Country.Benin,
            ["Togo"] = Country.Togo,
            ["Djibouti"] = Country.Djibouti,
            ["Eritrea"] = Country.Eritrea,
            ["Comoros"] = Country.Comoros,
            ["Seychelles"] = Country.Seychelles,
            ["Mauritius"] = Country.Mauritius,
            ["Cabo Verde"] = Country.CaboVerde,
            ["Cape Verde"] = Country.CaboVerde,
            ["Brunei"] = Country.Brunei,
            ["Brunei Darussalam"] = Country.Brunei,

            // BOSNIA VARIANTS
            ["Bosnia & Herzegovina"] = Country.BosniaAndHerzegovina,
            ["BiH"] = Country.BosniaAndHerzegovina,

            // MACEDONIA VARIANTS
            ["The former Yugoslav Republic of Macedonia"] = Country.NorthMacedonia,

            // VATICAN VARIANTS
            ["Vatican City State"] = Country.VaticanCity,
            ["Holy See (Vatican City State)"] = Country.VaticanCity,

            // SAO TOME VARIANTS
            ["São Tomé and Príncipe"] = Country.SaoTomeAndPrincipe,
            ["Sao Tome & Principe"] = Country.SaoTomeAndPrincipe,

            // KOSOVO (newly added)
            ["Kosovo"] = Country.Kosovo,
            ["Republic of Kosovo"] = Country.Kosovo,
            ["Kosova"] = Country.Kosovo,
            ["Kosovë"] = Country.Kosovo,

            // Edge formatting
            ["United States (USA)"] = Country.UnitedStates,
            ["United States, The"] = Country.UnitedStates,
            ["The United States"] = Country.UnitedStates,
            ["UNITED-STATES"] = Country.UnitedStates,
            ["United_States"] = Country.UnitedStates,
            ["United.States"] = Country.UnitedStates,

            // NEW VARIANTS FROM DIAGNOSTIC (previously unmapped)
            ["Aland"] = Country.AlandIslands,
            ["Åland"] = Country.AlandIslands,
            ["Antigua and Barb."] = Country.AntiguaBarbuda,
            ["Ashmore and Cartier Is."] = Country.Australia,
            ["Ashmore and Cartier Islands"] = Country.Australia,
            ["Australian Indian Ocean Territories"] = Country.Australia,
            ["Bosnia and Herz."] = Country.BosniaAndHerzegovina,
            ["Br. Indian Ocean Ter."] = Country.BritishIndianOceanTerritory,
            ["British Virgin Is."] = Country.VirginIslandsBritish,
            ["British Virgin Islands"] = Country.VirginIslandsBritish,
            ["Cayman Is."] = Country.CaymanIslands,
            ["Cook Is."] = Country.CookIslands,
            ["Dem. Rep. Korea"] = Country.NorthKorea,
            ["Eq. Guinea"] = Country.EquatorialGuinea,
            ["Faeroe Is."] = Country.FaroeIslands,
            ["Faeroe Islands"] = Country.FaroeIslands,
            ["Falkland Is."] = Country.FalklandIslands,
            ["Falkland Islands / Malvinas"] = Country.FalklandIslands,
            ["Federated States of Micronesia"] = Country.Micronesia,
            ["Fr. Polynesia"] = Country.FrenchPolynesia,
            ["Fr. S. Antarctic Lands"] = Country.FrenchSouthernTerritories,
            ["French Southern and Antarctic Lands"] = Country.FrenchSouthernTerritories,
            ["Heard I. and McDonald Is."] = Country.HeardIslandAndMcDonaldIslands,
            ["Heard I. and McDonald Islands"] = Country.HeardIslandAndMcDonaldIslands,
            ["Indian Ocean Ter."] = Country.BritishIndianOceanTerritory,
            ["Indian Ocean Territories"] = Country.BritishIndianOceanTerritory,
            ["Kingdom of eSwatini"] = Country.Eswatini,
            ["Marshall Is."] = Country.MarshallIslands,
            ["N. Cyprus"] = Country.Cyprus,
            ["Northern Cyprus"] = Country.Cyprus,
            ["Turkish Republic of Northern Cyprus"] = Country.Cyprus,
            ["N. Mariana Is."] = Country.NorthernMarianaIslands,
            ["People's Republic of China"] = Country.China,
            ["Pitcairn Is."] = Country.Pitcairn,
            ["Pitcairn Islands"] = Country.Pitcairn,
            ["Republic of Cabo Verde"] = Country.CaboVerde,
            ["Republic of Serbia"] = Country.Serbia,
            ["S. Geo. and the Is."] = Country.SouthGeorgiaAndSouthSandwichIslands,
            ["South Georgia and the Islands"] = Country.SouthGeorgiaAndSouthSandwichIslands,
            ["Saint Barthelemy"] = Country.SaintBarthelemy,
            ["St-Barthélemy"] = Country.SaintBarthelemy,
            ["St-Martin"] = Country.SaintMartin,
            ["Saint Helena"] = Country.SaintHelenaAscensionAndTristanDaCunha,
            ["São Tomé and Principe"] = Country.SaoTomeAndPrincipe,
            ["Siachen Glacier"] = Country.India,
            ["Solomon Is."] = Country.SolomonIslands,
            ["Somaliland"] = Country.Somalia,
            ["South Georgia and the South Sandwich Islands"] = Country.SouthGeorgiaAndSouthSandwichIslands,
            ["St. Pierre and Miquelon"] = Country.SaintPierreAndMiquelon,
            ["St. Vin. and Gren."] = Country.StVincent,
            ["The Gambia"] = Country.Gambia,
            ["Turks and Caicos Is."] = Country.TurksAndCaicosIslands,
            ["U.S. Virgin Is."] = Country.VirginIslandsUS,
            ["United States Virgin Islands"] = Country.VirginIslandsUS,
            ["W. Sahara"] = Country.WesternSahara,
            ["Wallis and Futuna Is."] = Country.WallisAndFutuna,
            ["Wallis and Futuna Islands"] = Country.WallisAndFutuna,
        };

        /// <summary>
        /// Attempts to map a GeoJSON country name to a Country enum value using multiple strategies.
        /// </summary>
        public static Country MapGeoJsonNameToCountry(string geoJsonName)
        {
            if (string.IsNullOrWhiteSpace(geoJsonName))
                return Country.Unknown;

            // 1. Direct dictionary lookup
            if (JsonNameToCountry.TryGetValue(geoJsonName, out var direct))
                return direct;

            // 2. Remove parenthetical segments e.g. "Iran (Islamic Republic of)" -> "Iran"
            var stripped = StripParenthetical(geoJsonName);
            if (!string.Equals(stripped, geoJsonName, StringComparison.Ordinal))
            {
                if (JsonNameToCountry.TryGetValue(stripped, out var parenMatch))
                    return parenMatch;
            }

            // 3. Country name variations (maps to CountryCode first)
            var code = CountryNameVariationsExtensions.ParseCountryName(geoJsonName);
            if (code != CountryCode.Unknown)
            {
                var viaCode = code.ToCountry();
                if (viaCode != Country.Unknown) return viaCode;
            }

            // 4. Try stripped version through variations
            if (!string.Equals(stripped, geoJsonName, StringComparison.Ordinal))
            {
                var code2 = CountryNameVariationsExtensions.ParseCountryName(stripped);
                if (code2 != CountryCode.Unknown)
                {
                    var c2 = code2.ToCountry();
                    if (c2 != Country.Unknown) return c2;
                }
            }

            // 5. Normalized dictionary in MapDictionaries
            var normalized = NormalizeName(geoJsonName);
            if (MapDictionaries.NormalizedNameToCountry.TryGetValue(normalized, out var normCountry) && normCountry != Country.Unknown)
                return normCountry;

            // 6. Try removing commas and retry
            var noComma = geoJsonName.Replace(",", "").Trim();
            if (!string.Equals(noComma, geoJsonName, StringComparison.OrdinalIgnoreCase))
            {
                var code3 = CountryNameVariationsExtensions.ParseCountryName(noComma);
                if (code3 != CountryCode.Unknown)
                {
                    var c3 = code3.ToCountry();
                    if (c3 != Country.Unknown) return c3;
                }
            }

            // 7. Fallback: exact compare against CountryToName display names
            foreach (var kvp in MapDictionaries.CountryToName)
            {
                if (string.Equals(kvp.Value, geoJsonName, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }

            // 8. Final attempt: normalized compare vs display names
            var normalizedStripped = NormalizeName(stripped);
            foreach (var kvp in MapDictionaries.CountryToName)
            {
                if (NormalizeName(kvp.Value) == normalized || NormalizeName(kvp.Value) == normalizedStripped)
                    return kvp.Key;
            }

            return Country.Unknown;
        }

        public static List<string> GetGeoJsonVariations(Country country) =>
            JsonNameToCountry.Where(kvp => kvp.Value == country).Select(kvp => kvp.Key).ToList();

        private static string NormalizeName(string name) =>
            string.IsNullOrWhiteSpace(name)
                ? string.Empty
                : new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLowerInvariant();

        private static string StripParenthetical(string name)
        {
            var idx = name.IndexOf('(');
            if (idx > 0)
            {
                var core = name[..idx].Trim();
                // Remove trailing commas / spaces
                core = core.Trim().TrimEnd(',').Trim();
                return core;
            }
            return name;
        }

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