// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Windows.Media;
    using WorldMapControls.Extensions;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Parses flexible JSON region (country / continent) mappings supporting:
    /// 1. Country name  -> Color
    /// 2. Country code  -> Color
    /// 3. Country name  -> Numeric value (mapped to color map)
    /// 4. Country code  -> Numeric value
    /// 5. Continent name -> Numeric value (applied to all countries in continent)
    /// 6. Continent code -> Numeric value
    /// 7. Continent name -> Color
    /// 8. Continent code -> Color
    /// Also tolerant to slightly malformed JSON missing a closing '}' or ']';
    /// will auto-correct in simple cases.
    /// </summary>
    public static class CountryColorJsonParser
    {
        #region Public API

        /// <summary>
        /// Parses a JSON string producing country color mappings. Numeric values are mapped through the supplied color map.
        /// </summary>
        /// <param name="json">Input JSON (object or array forms supported)</param>
        /// <param name="colorMapType">Colormap to use for numeric values</param>
        /// <returns>Parse result containing color mappings and statistics</returns>
        public static CountryColorParseResult Parse(string? json, ColorMapType colorMapType)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new CountryColorParseResult(Array.Empty<CountryColorMapping>(),
                    0, 0, "Color overrides cleared.", new Dictionary<Country, double>());
            }

            json = TryAutoFixJson(json!);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var explicitColorMappings = new Dictionary<Country, Brush>();
                var numericValues = new Dictionary<Country, double>();
                int success = 0; int failures = 0;

                void ApplyColor(Country c, Brush b)
                {
                    if (c == Country.Unknown) { failures++; return; }
                    explicitColorMappings[c] = b; success++;
                }

                void ApplyNumeric(Country c, double v)
                {
                    if (c == Country.Unknown) { failures++; return; }
                    numericValues[c] = v; success++;
                }

                IReadOnlyList<Country> GetCountriesForContinent(Continent cont)
                {
                    if (MapDictionaries.ContinentToCountries.TryGetValue(cont, out var list)) return list;
                    return Array.Empty<Country>();
                }

                // Asia user expectation often includes Middle East; merge them here
                IEnumerable<Country> ExpandLogicalContinent(Continent continent)
                {
                    if (continent == Continent.Asia)
                    {
                        // Union Asia + MiddleEast (avoid duplicates)
                        var asia = GetCountriesForContinent(Continent.Asia);
                        var me = GetCountriesForContinent(Continent.MiddleEast);
                        return asia.Concat(me).Distinct();
                    }
                    return GetCountriesForContinent(continent);
                }

                void ExpandContinentColor(Continent continent, Brush brush)
                {
                    if (continent == Continent.Unknown) { failures++; return; }
                    var countries = ExpandLogicalContinent(continent);
                    int count = 0;
                    foreach (var c in countries) { ApplyColor(c, brush); count++; }
                    if (count == 0) failures++;
                }

                void ExpandContinentNumeric(Continent continent, double value)
                {
                    if (continent == Continent.Unknown) { failures++; return; }
                    var countries = ExpandLogicalContinent(continent);
                    int count = 0;
                    foreach (var c in countries) { ApplyNumeric(c, value); count++; }
                    if (count == 0) failures++;
                }

                // Core dispatcher for key/value pair
                void ProcessPair(string key, JsonElement valueElement)
                {
                    if (string.IsNullOrWhiteSpace(key)) { failures++; return; }

                    // Determine if key refers to country or continent
                    var country = ResolveCountry(key);
                    var continent = country == Country.Unknown ? ResolveContinent(key) : Continent.Unknown;

                    // Try interpret value
                    switch (valueElement.ValueKind)
                    {
                        case JsonValueKind.String:
                            var s = valueElement.GetString();
                            if (string.IsNullOrWhiteSpace(s)) { failures++; return; }
                            if (TryCreateBrush(s!, out var brush))
                            {
                                if (country != Country.Unknown) ApplyColor(country, brush!);
                                else if (continent != Continent.Unknown) ExpandContinentColor(continent, brush!);
                                else failures++;
                            }
                            else if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var strNum))
                            {
                                if (country != Country.Unknown) ApplyNumeric(country, strNum);
                                else if (continent != Continent.Unknown) ExpandContinentNumeric(continent, strNum);
                                else failures++;
                            }
                            else failures++;
                            break;
                        case JsonValueKind.Number:
                            if (valueElement.TryGetDouble(out var num))
                            {
                                if (country != Country.Unknown) ApplyNumeric(country, num);
                                else if (continent != Continent.Unknown) ExpandContinentNumeric(continent, num);
                                else failures++;
                            }
                            else failures++;
                            break;
                        default:
                            failures++;
                            break;
                    }
                }

                ParseRoot(root, ProcessPair);

                // Convert numeric values to colors (without overriding explicit color entries)
                if (numericValues.Count > 0)
                {
                    var ordered = numericValues.ToList();
                    var colors = ColorMapCalculator.MapValues(ordered.Select(o => o.Value), colorMapType);
                    for (int i = 0; i < ordered.Count; i++)
                    {
                        var c = ordered[i].Key;
                        if (explicitColorMappings.ContainsKey(c)) continue; // explicit wins
                        var clr = colors[i];
                        var brush = new SolidColorBrush(clr); brush.Freeze();
                        explicitColorMappings[c] = brush;
                    }
                }

                var mappings = explicitColorMappings.Select(kv => new CountryColorMapping(kv.Key, kv.Value)).ToArray();
                var message = BuildMessage(mappings.Length, failures, numericValues.Count);
                return new CountryColorParseResult(mappings, mappings.Length, failures, message, numericValues);
            }
            catch (Exception ex)
            {
                return new CountryColorParseResult(Array.Empty<CountryColorMapping>(), 0, 1, $"Invalid JSON: {ex.Message}", new Dictionary<Country, double>());
            }
        }

        #endregion Public API

        #region JSON Parsing Helpers

        private static void ParseRoot(JsonElement root, Action<string, JsonElement> processPair)
        {
            switch (root.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in root.EnumerateObject())
                        processPair(prop.Name, prop.Value);
                    break;
                case JsonValueKind.Array:
                    foreach (var el in root.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.Object)
                        {
                            // Accept { "country": "France", "value": 0.8 } or { "code": "FR", "color": "#FF0000" }
                            var key = ExtractIdentifier(el);
                            var valueEl = ExtractValueElement(el);
                            if (key != null && valueEl.HasValue)
                                processPair(key, valueEl.Value);
                        }
                        else if (el.ValueKind == JsonValueKind.Array && el.GetArrayLength() == 2 && el[0].ValueKind == JsonValueKind.String)
                        {
                            processPair(el[0].GetString()!, el[1]);
                        }
                    }
                    break;
            }
        }

        private static string? ExtractIdentifier(JsonElement obj)
        {
            var idProps = new[] { "country", "countryCode", "code", "id", "name", "continent" };
            foreach (var p in idProps)
                if (obj.TryGetProperty(p, out var el) && el.ValueKind == JsonValueKind.String)
                    return el.GetString();
            return null;
        }

        private static JsonElement? ExtractValueElement(JsonElement obj)
        {
            var valProps = new[] { "value", "color", "fill", "v" };
            foreach (var p in valProps)
                if (obj.TryGetProperty(p, out var el))
                    return el;
            // Fallback: if object has exactly 2 props and one is identifier, take the other
            if (obj.ValueKind == JsonValueKind.Object)
            {
                var props = obj.EnumerateObject().ToList();
                if (props.Count == 2)
                {
                    foreach (var pr in props)
                    {
                        if (pr.Value.ValueKind != JsonValueKind.String || !IsIdentifierProperty(pr.Name))
                            return pr.Value; // naive but covers simple pairs
                    }
                }
            }
            return null;
        }

        private static bool IsIdentifierProperty(string name) =>
            name.Equals("country", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("code", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("id", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("name", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("countryCode", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("continent", StringComparison.OrdinalIgnoreCase);

        #endregion JSON Parsing Helpers

        #region Resolution Helpers

        private static Country ResolveCountry(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return Country.Unknown;
            identifier = identifier.Trim();

            // Country code first
            if (identifier.Length <= 3 && Enum.TryParse<CountryCode>(identifier.ToUpperInvariant(), out var cc) && cc != CountryCode.Unknown)
            {
                var ctry = cc.ToCountry();
                if (ctry != Country.Unknown) return ctry;
            }

            // Direct enum
            if (Enum.TryParse<Country>(identifier.Replace(" ", ""), true, out var ce) && ce != Country.Unknown)
                return ce;

            // Integer id
            if (int.TryParse(identifier, out var intVal) && Enum.IsDefined(typeof(Country), intVal))
                return (Country)intVal;

            // Normalized dictionary
            var norm = Normalize(identifier);
            if (MapDictionaries.NormalizedNameToCountry.TryGetValue(norm, out var byName))
                return byName;

            return Country.Unknown;
        }

        private static Continent ResolveContinent(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return Continent.Unknown;
            var cleaned = identifier.Replace('/', ' ').Replace('-', ' ');
            var norm = Normalize(cleaned);

            // Direct enum
            if (Enum.TryParse<Continent>(cleaned.Replace(" ", ""), true, out var cont) && cont != Continent.Unknown)
                return cont;

            // Codes / aliases
            return norm switch
            {
                "af" or "afr" or "africa" => Continent.Africa,
                "an" or "ant" or "antarctica" => Continent.Antarctica,
                "as" or "asia" => Continent.Asia,
                "eu" or "eur" or "europe" => Continent.Europe,
                "na" or "nam" or "northamerica" => Continent.NorthAmerica,
                "sa" or "sam" or "southamerica" => Continent.SouthAmerica,
                "oc" or "oce" or "oceania" or "australia" or "australasia" or "australiaoceania" => Continent.Oceania,
                "me" or "middleeast" or "mid" => Continent.MiddleEast,
                _ => Continent.Unknown
            };
        }

        private static string Normalize(string v) => new string(v.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        private static bool TryCreateBrush(string colorStr, out Brush? brush)
        {
            brush = null;
            try
            {
                // If looks like hex or named color – attempt convert
                if (!colorStr.StartsWith('#'))
                {
                    // Try named – if fails treat as not a color
                    var named = ColorConverter.ConvertFromString(colorStr);
                    if (named is Color nc)
                    {
                        var b = new SolidColorBrush(nc); b.Freeze(); brush = b; return true;
                    }
                }
                else
                {
                    var c = (Color)ColorConverter.ConvertFromString(colorStr);
                    var b = new SolidColorBrush(c); b.Freeze(); brush = b; return true;
                }
            }
            catch { return false; }
            return false;
        }

        #endregion Resolution Helpers

        #region Utility

        private static string BuildMessage(int colorCount, int failures, int numericCount)
        {
            var parts = new List<string>();
            if (colorCount > 0) parts.Add($"Applied {colorCount} overrides.");
            if (numericCount > 0) parts.Add($"Mapped {numericCount} numeric values.");
            if (colorCount == 0 && numericCount == 0) parts.Add("No valid mappings found.");
            if (failures > 0) parts.Add($"{failures} entries failed.");
            return string.Join(' ', parts);
        }

        private static string TryAutoFixJson(string json)
        {
            // Simple heuristic: balance braces/brackets. Only add at end (non-destructive quick fix)
            int openObj = json.Count(c => c == '{');
            int closeObj = json.Count(c => c == '}');
            int openArr = json.Count(c => c == '['); int closeArr = json.Count(c => c == ']');
            if (closeObj < openObj) json += new string('}', openObj - closeObj);
            if (closeArr < openArr) json += new string(']', openArr - closeArr);
            return json;
        }

        #endregion Utility
    }

    /// <summary>
    /// Result of parsing flexible region JSON.
    /// </summary>
    public record CountryColorParseResult(
        CountryColorMapping[] ColorMappings,
        int SuccessCount,
        int FailureCount,
        string Message,
        IReadOnlyDictionary<Country, double> NumericValues
    );
}