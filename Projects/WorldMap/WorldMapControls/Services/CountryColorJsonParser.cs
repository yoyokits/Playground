// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json;
    using System.Windows.Media;
    using WorldMapControls.Extensions;
    using WorldMapControls.Models;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Parses JSON country color mappings and converts them to CountryColorMapping objects.
    /// </summary>
    public static class CountryColorJsonParser
    {
        #region Methods

        /// <summary>
        /// Parses JSON string containing country-color mappings into CountryColorMapping objects.
        /// Supports multiple JSON formats and country identification strategies.
        /// </summary>
        /// <param name="json">JSON string to parse</param>
        /// <returns>Parse result containing color mappings and statistics</returns>
        public static CountryColorParseResult Parse(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new CountryColorParseResult(
                    Array.Empty<CountryColorMapping>(),
                    0,
                    0,
                    "Color overrides cleared."
                );
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var list = new List<CountryColorMapping>();
                int successCount = 0;
                int failureCount = 0;

                void TryAddSmart(string? identifier, string? colorStr)
                {
                    if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(colorStr))
                    {
                        failureCount++;
                        return;
                    }

                    var country = ResolveCountry(identifier);
                    if (country == null || country == Country.Unknown)
                    {
                        failureCount++;
                        Debug.WriteLine($"[CountryColorParser] Failed to map: '{identifier}'");
                        return;
                    }

                    var brush = CreateBrush(colorStr);
                    if (brush == null)
                    {
                        failureCount++;
                        return;
                    }

                    list.Add(new CountryColorMapping(country.Value, brush));
                    successCount++;
                    Debug.WriteLine($"[CountryColorParser] Mapped: '{identifier}' -> {country.Value}({(int)country.Value})");
                }

                var root = doc.RootElement;
                ParseJsonElement(root, TryAddSmart);

                var message = BuildResultMessage(successCount, failureCount);
                return new CountryColorParseResult(
                    list.ToArray(),
                    successCount,
                    failureCount,
                    message
                );
            }
            catch (Exception ex)
            {
                return new CountryColorParseResult(
                    Array.Empty<CountryColorMapping>(),
                    0,
                    1,
                    $"Invalid color JSON: {ex.Message}"
                );
            }
        }

        private static string BuildResultMessage(int successCount, int failureCount)
        {
            var message = successCount > 0
                ? $"Applied {successCount} country color overrides successfully."
                : "No valid country colors found.";

            if (failureCount > 0)
                message += $" {failureCount} entries failed to parse.";

            return message;
        }

        private static Brush? CreateBrush(string colorStr)
        {
            try
            {
                // Normalize color
                if (!colorStr.StartsWith("#", StringComparison.Ordinal))
                {
                    var named = (Color)ColorConverter.ConvertFromString(colorStr);
                    colorStr = $"#{named.A:X2}{named.R:X2}{named.G:X2}{named.B:X2}";
                }

                var c = (Color)ColorConverter.ConvertFromString(colorStr);
                var brush = new SolidColorBrush(c);
                brush.Freeze();
                return brush;
            }
            catch
            {
                return null;
            }
        }

        private static void ParseJsonElement(JsonElement root, Action<string?, string?> tryAdd)
        {
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in root.EnumerateObject())
                {
                    var colorVal = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
                    tryAdd(prop.Name, colorVal);
                }
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in root.EnumerateArray())
                {
                    switch (el.ValueKind)
                    {
                        case JsonValueKind.Object:
                            ParseObjectElement(el, tryAdd);
                            break;

                        case JsonValueKind.Array:
                            if (el.GetArrayLength() == 2 &&
                                el[0].ValueKind == JsonValueKind.String &&
                                el[1].ValueKind == JsonValueKind.String)
                            {
                                tryAdd(el[0].GetString(), el[1].GetString());
                            }
                            break;
                    }
                }
            }
        }

        private static void ParseObjectElement(JsonElement element, Action<string?, string?> tryAdd)
        {
            string? identifier = null;
            string? color = null;

            // Try multiple property names for flexibility
            var identifierProps = new[] { "countryCode", "code", "id", "country", "name" };
            var colorProps = new[] { "color", "value", "fill" };

            foreach (var prop in identifierProps)
            {
                if (element.TryGetProperty(prop, out var idEl) && idEl.ValueKind == JsonValueKind.String)
                {
                    identifier = idEl.GetString();
                    break;
                }
            }

            foreach (var prop in colorProps)
            {
                if (element.TryGetProperty(prop, out var colEl) && colEl.ValueKind == JsonValueKind.String)
                {
                    color = colEl.GetString();
                    break;
                }
            }

            tryAdd(identifier, color);
        }

        private static Country? ResolveCountry(string identifier)
        {
            // Strategy 1: Try as country code first (2-3 characters, most reliable)
            if (identifier.Length <= 3 && Enum.TryParse<CountryCode>(identifier.ToUpperInvariant(), out var countryCode))
            {
                var country = countryCode.ToCountry();
                if (country != Country.Unknown)
                    return country;
            }

            // Strategy 2: Try direct enum ID parsing (for debugging: "UnitedStates", "1", etc.)
            if (Enum.TryParse<Country>(identifier.Replace(" ", ""), true, out var directEnum) && directEnum != Country.Unknown)
            {
                return directEnum;
            }

            // Try parsing as enum integer value
            if (int.TryParse(identifier, out var enumInt) && Enum.IsDefined(typeof(Country), enumInt))
            {
                return (Country)enumInt;
            }

            // Strategy 3: Try as country name (fallback)
            var normalized = new string(identifier.ToLowerInvariant()
                .Replace("'", "")
                .Replace("-", "")
                .Replace(" ", "")
                .ToCharArray());

            if (MapDictionaries.NormalizedNameToCountry.TryGetValue(normalized, out var countryByName))
            {
                return countryByName;
            }

            return null;
        }

        #endregion Methods
    }

    /// <summary>
    /// Result of country color JSON parsing operation.
    /// </summary>
    /// <param name="ColorMappings">Successfully parsed color mappings</param>
    /// <param name="SuccessCount">Number of successful mappings</param>
    /// <param name="FailureCount">Number of failed mappings</param>
    /// <param name="Message">Status message</param>
    public record CountryColorParseResult(
        CountryColorMapping[] ColorMappings,
        int SuccessCount,
        int FailureCount,
        string Message
    );
}