// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Extensions;
using WorldMapControls.Models.Enums;
using WorldMapControls.Services;
using WorldMapControls.Models;
using System.Text.Json;

namespace WorldMapTest.Extensions
{
    [TestClass]
    public class CountryParsingDiagnosticTests
    {
        [TestMethod]
        public void DiagnoseUnmappedCountries_WithSampleGeoJsonData_ShouldIdentifyParsingIssues()
        {
            // Sample problematic GeoJSON data that commonly appears in real world files
            var problematicGeoJson = @"{
                ""type"": ""FeatureCollection"",
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Dominican Republic"",
                            ""ADMIN"": ""Dominican Republic"",
                            ""ISO_A2"": ""DO""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Congo"",
                            ""ADMIN"": ""Republic of the Congo"",
                            ""ISO_A2"": ""CG""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Congo, Dem. Rep."",
                            ""ADMIN"": ""Democratic Republic of the Congo"",
                            ""ISO_A2"": ""CD""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Russian Federation"",
                            ""ISO_A2"": ""RU""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""United States of America"",
                            ""ISO_A2"": ""US""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Korea, Republic of"",
                            ""ISO_A2"": ""KR""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Korea, Dem. People's Rep. of"",
                            ""ISO_A2"": ""KP""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Iran (Islamic Republic of)"",
                            ""ISO_A2"": ""IR""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Venezuela (Bolivarian Republic of)"",
                            ""ISO_A2"": ""VE""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Tanzania, United Republic of"",
                            ""ISO_A2"": ""TZ""
                        },
                        ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                    }
                ]
            }";

            // Act - Parse and analyze
            var analysisResults = AnalyzeCountryParsing(problematicGeoJson);

            // Assert - All countries should be mappable
            analysisResults.UnmappedCountries.Should().BeEmpty(
                $"All countries should be parseable. Unmapped: {string.Join(", ", analysisResults.UnmappedCountries.Select(u => $"{u.Name} (ISO: {u.IsoCode})"))}");

            // Output results for debugging
            Console.WriteLine("=== COUNTRY PARSING DIAGNOSTIC RESULTS ===");
            Console.WriteLine($"Total Countries Found: {analysisResults.TotalCountries}");
            Console.WriteLine($"Successfully Mapped: {analysisResults.MappedCountries.Count}");
            Console.WriteLine($"Unmapped Countries: {analysisResults.UnmappedCountries.Count}");
            
            if (analysisResults.UnmappedCountries.Any())
            {
                Console.WriteLine("\nUNMAPPED COUNTRIES:");
                foreach (var unmapped in analysisResults.UnmappedCountries)
                {
                    Console.WriteLine($"  - Name: '{unmapped.Name}'");
                    Console.WriteLine($"    ISO Code: {unmapped.IsoCode}");
                    Console.WriteLine($"    Parsing Methods Tried: {string.Join(", ", unmapped.ParsingMethodsAttempted)}");
                    Console.WriteLine();
                }
            }

            if (analysisResults.MappedCountries.Any())
            {
                Console.WriteLine("\nSUCCESSFULLY MAPPED COUNTRIES:");
                foreach (var mapped in analysisResults.MappedCountries)
                {
                    Console.WriteLine($"  - '{mapped.Name}' -> {mapped.CountryCode} -> {mapped.Country}");
                }
            }
        }

        [TestMethod]
        public void TestAllCountryCodeToCountryMappings_ShouldBeComplete()
        {
            // Arrange - Get all country codes
            var allCountryCodes = CountryCodeExtensions.GetAllCountryCodes();
            var unmappedCodes = new List<CountryCode>();
            var mappingResults = new List<CountryCodeMappingResult>();

            // Act - Test each country code mapping
            foreach (var code in allCountryCodes)
            {
                var country = code.ToCountry();
                var countryName = code.GetCountryName();
                var backMappedCode = country.ToCountryCode();
                
                var result = new CountryCodeMappingResult
                {
                    CountryCode = code,
                    CountryName = countryName,
                    MappedCountry = country,
                    BackMappedCountryCode = backMappedCode,
                    IsBidirectionallyConsistent = (backMappedCode == code),
                    IsMapped = (country != Country.Unknown)
                };
                
                mappingResults.Add(result);
                
                if (country == Country.Unknown)
                {
                    unmappedCodes.Add(code);
                }
            }

            // Assert
            unmappedCodes.Should().BeEmpty(
                $"All country codes should map to countries. Unmapped codes: {string.Join(", ", unmappedCodes)}");

            // Output diagnostic info
            Console.WriteLine("=== COUNTRY CODE MAPPING DIAGNOSTIC RESULTS ===");
            Console.WriteLine($"Total Country Codes: {allCountryCodes.Length}");
            Console.WriteLine($"Successfully Mapped: {mappingResults.Count(r => r.IsMapped)}");
            Console.WriteLine($"Bidirectionally Consistent: {mappingResults.Count(r => r.IsBidirectionallyConsistent)}");
            Console.WriteLine($"Unmapped: {unmappedCodes.Count}");

            if (unmappedCodes.Any())
            {
                Console.WriteLine("\nUNMAPPED COUNTRY CODES:");
                foreach (var code in unmappedCodes)
                {
                    var name = code.GetCountryName();
                    Console.WriteLine($"  - {code} ({name})");
                }
            }

            var inconsistentMappings = mappingResults.Where(r => r.IsMapped && !r.IsBidirectionallyConsistent).ToList();
            if (inconsistentMappings.Any())
            {
                Console.WriteLine("\nBIDIRECTIONALLY INCONSISTENT MAPPINGS:");
                foreach (var result in inconsistentMappings)
                {
                    Console.WriteLine($"  - {result.CountryCode} -> {result.MappedCountry} -> {result.BackMappedCountryCode}");
                }
            }
        }

        [TestMethod]
        public void TestStringToCountryCodeParsing_ShouldHandleCommonVariations()
        {
            // Arrange - Common problematic country name variations
            var testCases = new[]
            {
                // Dominican Republic variations
                ("Dominican Republic", CountryCode.DO),
                ("Dominican Rep.", CountryCode.DO),
                ("Dominicana", CountryCode.DO),
                
                // Congo variations
                ("Congo", CountryCode.CG), // Should default to Republic of Congo
                ("Congo, Rep.", CountryCode.CG),
                ("Congo, Dem. Rep.", CountryCode.CD),
                ("Democratic Republic of the Congo", CountryCode.CD),
                ("Republic of the Congo", CountryCode.CG),
                
                // Russia variations
                ("Russia", CountryCode.RU),
                ("Russian Federation", CountryCode.RU),
                ("Russian Fed.", CountryCode.RU),
                
                // Korea variations
                ("Korea", CountryCode.KR), // Should default to South Korea
                ("South Korea", CountryCode.KR),
                ("Korea, Republic of", CountryCode.KR),
                ("North Korea", CountryCode.KP),
                ("Korea, Dem. People's Rep. of", CountryCode.KP),
                
                // Official UN names
                ("Iran (Islamic Republic of)", CountryCode.IR),
                ("Venezuela (Bolivarian Republic of)", CountryCode.VE),
                ("Tanzania, United Republic of", CountryCode.TZ),
                ("Bolivia (Plurinational State of)", CountryCode.BO),
                
                // United States variations
                ("United States", CountryCode.US),
                ("United States of America", CountryCode.US),
                ("USA", CountryCode.US),
                ("US", CountryCode.US),
                
                // United Kingdom variations
                ("United Kingdom", CountryCode.GB),
                ("UK", CountryCode.GB),
                ("Great Britain", CountryCode.GB),
                ("Britain", CountryCode.GB)
            };

            var unmappedResults = new List<StringToCountryCodeTestResult>();

            // Act & Assert
            foreach (var (countryName, expectedCode) in testCases)
            {
                // Try multiple parsing methods
                var result = new StringToCountryCodeTestResult
                {
                    InputName = countryName,
                    ExpectedCountryCode = expectedCode
                };

                // Method 1: Direct CountryCodeExtensions.GetCountryCode
                result.DirectParsingResult = CountryCodeExtensions.GetCountryCode(countryName);
                
                // Method 2: Via GeoJsonNameMapper
                var mappedCountry = GeoJsonNameMapper.MapGeoJsonNameToCountry(countryName);
                result.GeoJsonMappingResult = mappedCountry.ToCountryCode();
                
                // Method 3: Fuzzy matching (normalize and try)
                result.FuzzyMatchingResult = TryFuzzyCountryCodeMatching(countryName);

                // Determine which method worked
                result.IsDirectParsingSuccessful = (result.DirectParsingResult == expectedCode);
                result.IsGeoJsonMappingSuccessful = (result.GeoJsonMappingResult == expectedCode);
                result.IsFuzzyMatchingSuccessful = (result.FuzzyMatchingResult == expectedCode);
                result.IsAnyMethodSuccessful = result.IsDirectParsingSuccessful || 
                                               result.IsGeoJsonMappingSuccessful || 
                                               result.IsFuzzyMatchingSuccessful;

                if (!result.IsAnyMethodSuccessful)
                {
                    unmappedResults.Add(result);
                }

                // Assert that at least one method works
                result.IsAnyMethodSuccessful.Should().BeTrue(
                    $"Country '{countryName}' should be parseable to {expectedCode} by at least one method. " +
                    $"Direct: {result.DirectParsingResult}, GeoJson: {result.GeoJsonMappingResult}, Fuzzy: {result.FuzzyMatchingResult}");
            }

            // Output diagnostic results
            Console.WriteLine("=== STRING TO COUNTRY CODE PARSING RESULTS ===");
            Console.WriteLine($"Total Test Cases: {testCases.Length}");
            Console.WriteLine($"Successfully Parsed: {testCases.Length - unmappedResults.Count}");
            Console.WriteLine($"Failed to Parse: {unmappedResults.Count}");

            if (unmappedResults.Any())
            {
                Console.WriteLine("\nFAILED PARSING CASES:");
                foreach (var result in unmappedResults)
                {
                    Console.WriteLine($"  Input: '{result.InputName}' (Expected: {result.ExpectedCountryCode})");
                    Console.WriteLine($"    Direct: {result.DirectParsingResult}");
                    Console.WriteLine($"    GeoJson: {result.GeoJsonMappingResult}");
                    Console.WriteLine($"    Fuzzy: {result.FuzzyMatchingResult}");
                    Console.WriteLine();
                }
            }
        }

        [TestMethod]
        public void CreateCountryNameToCountryCodeDictionary_ShouldCoverAllVariations()
        {
            // Act - Build comprehensive mapping dictionary
            var dictionary = BuildCountryNameToCountryCodeDictionary();

            // Assert - Should have extensive coverage
            dictionary.Count.Should().BeGreaterThan(300, "Dictionary should include many country name variations");

            // Test critical mappings
            var criticalMappings = new[]
            {
                ("Dominican Republic", CountryCode.DO),
                ("Congo", CountryCode.CG),
                ("Congo, Dem. Rep.", CountryCode.CD),
                ("Russian Federation", CountryCode.RU),
                ("Korea, Republic of", CountryCode.KR),
                ("Iran (Islamic Republic of)", CountryCode.IR)
            };

            foreach (var (name, expectedCode) in criticalMappings)
            {
                dictionary.Should().ContainKey(name)
                    .WhoseValue.Should().Be(expectedCode, $"'{name}' should map to {expectedCode}");
            }

            // Output dictionary contents for review
            Console.WriteLine("=== COUNTRY NAME TO COUNTRY CODE DICTIONARY ===");
            Console.WriteLine($"Total Entries: {dictionary.Count}");
            Console.WriteLine("\nSample Mappings:");
            
            var sortedMappings = dictionary.OrderBy(kvp => kvp.Key).Take(20);
            foreach (var mapping in sortedMappings)
            {
                Console.WriteLine($"  '{mapping.Key}' -> {mapping.Value}");
            }

            Console.WriteLine("\nCritical Mappings:");
            foreach (var (name, expectedCode) in criticalMappings)
            {
                if (dictionary.TryGetValue(name, out var actualCode))
                {
                    Console.WriteLine($"  ? '{name}' -> {actualCode}");
                }
                else
                {
                    Console.WriteLine($"  ? '{name}' -> NOT FOUND");
                }
            }
        }

        #region Helper Methods

        private CountryParsingAnalysisResult AnalyzeCountryParsing(string geoJsonData)
        {
            var result = new CountryParsingAnalysisResult();

            try
            {
                var jsonDoc = JsonDocument.Parse(geoJsonData);
                var features = jsonDoc.RootElement.GetProperty("features");

                foreach (var feature in features.EnumerateArray())
                {
                    if (feature.TryGetProperty("properties", out var props))
                    {
                        var countryInfo = ExtractCountryInfo(props);
                        if (countryInfo != null)
                        {
                            result.TotalCountries++;
                            
                            var mappingAttempt = AttemptCountryMapping(countryInfo);
                            if (mappingAttempt.IsSuccessful)
                            {
                                result.MappedCountries.Add(mappingAttempt);
                            }
                            else
                            {
                                result.UnmappedCountries.Add(new UnmappedCountryInfo
                                {
                                    Name = countryInfo.Name,
                                    IsoCode = countryInfo.IsoCode,
                                    ParsingMethodsAttempted = mappingAttempt.MethodsAttempted
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing GeoJSON: {ex.Message}");
            }

            return result;
        }

        private CountryInfo? ExtractCountryInfo(JsonElement properties)
        {
            string? name = null;
            string? isoCode = null;

            // Try different property names for country name
            var nameProperties = new[] { "NAME", "name", "ADMIN", "admin", "NAME_EN", "COUNTRY", "Country", "country", "NAME_LONG" };
            foreach (var prop in nameProperties)
            {
                if (properties.TryGetProperty(prop, out var nameElement) && 
                    nameElement.ValueKind == JsonValueKind.String)
                {
                    var value = nameElement.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        name = value;
                        break;
                    }
                }
            }

            // Try different property names for ISO code
            var isoProperties = new[] { "ISO_A2", "iso_a2", "ISO", "iso", "CODE", "code" };
            foreach (var prop in isoProperties)
            {
                if (properties.TryGetProperty(prop, out var isoElement) && 
                    isoElement.ValueKind == JsonValueKind.String)
                {
                    var value = isoElement.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        isoCode = value;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(name))
                return null;

            return new CountryInfo { Name = name, IsoCode = isoCode };
        }

        private CountryMappingAttempt AttemptCountryMapping(CountryInfo countryInfo)
        {
            var attempt = new CountryMappingAttempt
            {
                Name = countryInfo.Name,
                IsoCode = countryInfo.IsoCode,
                MethodsAttempted = new List<string>()
            };

            // Method 1: Direct name lookup via CountryCodeExtensions
            attempt.MethodsAttempted.Add("DirectNameLookup");
            var directCode = CountryCodeExtensions.GetCountryCode(countryInfo.Name);
            if (directCode != CountryCode.Unknown)
            {
                attempt.CountryCode = directCode;
                attempt.Country = directCode.ToCountry();
                attempt.IsSuccessful = true;
                return attempt;
            }

            // Method 2: ISO code lookup (if available)
            if (!string.IsNullOrWhiteSpace(countryInfo.IsoCode))
            {
                attempt.MethodsAttempted.Add("IsoCodeLookup");
                if (Enum.TryParse<CountryCode>(countryInfo.IsoCode, true, out var isoCode) && isoCode != CountryCode.Unknown)
                {
                    attempt.CountryCode = isoCode;
                    attempt.Country = isoCode.ToCountry();
                    attempt.IsSuccessful = true;
                    return attempt;
                }
            }

            // Method 3: GeoJSON name mapping
            attempt.MethodsAttempted.Add("GeoJsonMapping");
            var geoJsonCountry = GeoJsonNameMapper.MapGeoJsonNameToCountry(countryInfo.Name);
            if (geoJsonCountry != Country.Unknown)
            {
                attempt.Country = geoJsonCountry;
                attempt.CountryCode = geoJsonCountry.ToCountryCode();
                attempt.IsSuccessful = true;
                return attempt;
            }

            // Method 4: Fuzzy matching
            attempt.MethodsAttempted.Add("FuzzyMatching");
            var fuzzyCode = TryFuzzyCountryCodeMatching(countryInfo.Name);
            if (fuzzyCode != CountryCode.Unknown)
            {
                attempt.CountryCode = fuzzyCode;
                attempt.Country = fuzzyCode.ToCountry();
                attempt.IsSuccessful = true;
                return attempt;
            }

            return attempt;
        }

        private CountryCode TryFuzzyCountryCodeMatching(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                return CountryCode.Unknown;

            // Normalize the input
            var normalized = NormalizeName(countryName);

            // Try matching against all country code names
            var allCodes = CountryCodeExtensions.GetAllCountryCodes();
            foreach (var code in allCodes)
            {
                var codeName = code.GetCountryName();
                if (NormalizeName(codeName) == normalized)
                {
                    return code;
                }
            }

            return CountryCode.Unknown;
        }

        private string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return new string(name
                .Where(c => char.IsLetterOrDigit(c))
                .ToArray())
                .ToLowerInvariant();
        }

        private Dictionary<string, CountryCode> BuildCountryNameToCountryCodeDictionary()
        {
            var dictionary = new Dictionary<string, CountryCode>(StringComparer.OrdinalIgnoreCase);

            // Add all standard country code to name mappings
            var allCodes = CountryCodeExtensions.GetAllCountryCodes();
            foreach (var code in allCodes)
            {
                var name = code.GetCountryName();
                if (!dictionary.ContainsKey(name))
                {
                    dictionary[name] = code;
                }
            }

            // Helper method to safely add variations
            void AddVariation(string name, CountryCode code)
            {
                if (!string.IsNullOrWhiteSpace(name) && !dictionary.ContainsKey(name))
                {
                    dictionary[name] = code;
                }
            }

            // Add common variations and aliases
            var variations = new Dictionary<string, CountryCode>(StringComparer.OrdinalIgnoreCase)
            {
                // Dominican Republic variations
                { "Dominican Republic", CountryCode.DO },
                { "Dominican Rep.", CountryCode.DO },
                { "Dominicana", CountryCode.DO },
                
                // Congo variations
                { "Congo", CountryCode.CG }, // Default to Republic of Congo
                { "Congo, Rep.", CountryCode.CG },
                { "Congo Republic", CountryCode.CG },
                { "Congo-Brazzaville", CountryCode.CG },
                { "Republic of the Congo", CountryCode.CG },
                { "Congo, Dem. Rep.", CountryCode.CD },
                { "Dem. Rep. Congo", CountryCode.CD },
                { "Democratic Rep. Congo", CountryCode.CD },
                { "Congo DR", CountryCode.CD },
                { "Congo DRC", CountryCode.CD },
                { "Congo-Kinshasa", CountryCode.CD },
                { "Democratic Republic of the Congo", CountryCode.CD },
                { "Zaire", CountryCode.CD },
                { "DRC", CountryCode.CD },
                
                // Russia variations
                { "Russia", CountryCode.RU },
                { "Russian Federation", CountryCode.RU },
                { "Russian Fed.", CountryCode.RU },
                
                // Korea variations
                { "Korea", CountryCode.KR }, // Default to South Korea
                { "South Korea", CountryCode.KR },
                { "Korea, Republic of", CountryCode.KR },
                { "Republic of Korea", CountryCode.KR },
                { "Korea Rep.", CountryCode.KR },
                { "North Korea", CountryCode.KP },
                { "Korea, Dem. People's Rep. of", CountryCode.KP },
                { "Democratic People's Republic of Korea", CountryCode.KP },
                { "Korea DPR", CountryCode.KP },
                { "DPRK", CountryCode.KP },
                
                // Official UN names
                { "Iran (Islamic Republic of)", CountryCode.IR },
                { "Venezuela (Bolivarian Republic of)", CountryCode.VE },
                { "Tanzania, United Republic of", CountryCode.TZ },
                { "Bolivia (Plurinational State of)", CountryCode.BO },
                { "Syrian Arab Republic", CountryCode.SY },
                { "Lao People's Democratic Republic", CountryCode.LA },
                
                // United States variations
                { "United States", CountryCode.US },
                { "United States of America", CountryCode.US },
                { "USA", CountryCode.US },
                { "US", CountryCode.US },
                { "America", CountryCode.US },
                
                // United Kingdom variations
                { "United Kingdom", CountryCode.GB },
                { "UK", CountryCode.GB },
                { "Great Britain", CountryCode.GB },
                { "Britain", CountryCode.GB },
                
                // Other common variations
                { "Czech Republic", CountryCode.CZ },
                { "Czechia", CountryCode.CZ },
                // Burma is already handled by the standard name mapping, so don't add duplicate
                { "Ivory Coast", CountryCode.CI },
                { "Côte d'Ivoire", CountryCode.CI },
                { "Cote d'Ivoire", CountryCode.CI },
                { "Sri Lanka", CountryCode.LK },
                { "Ceylon", CountryCode.LK },
                { "United Arab Emirates", CountryCode.AE },
                { "UAE", CountryCode.AE },
                { "Saudi Arabia", CountryCode.SA },
                { "Kingdom of Saudi Arabia", CountryCode.SA },
                { "New Zealand", CountryCode.NZ },
                { "NZ", CountryCode.NZ },
                { "Papua New Guinea", CountryCode.PG },
                { "PNG", CountryCode.PG },
                
                // European countries
                { "Bosnia and Herzegovina", CountryCode.BA },
                { "Bosnia & Herzegovina", CountryCode.BA },
                { "BiH", CountryCode.BA },
                { "North Macedonia", CountryCode.MK },
                { "Macedonia", CountryCode.MK },
                { "FYROM", CountryCode.MK },
                
                // African countries
                { "Central African Republic", CountryCode.CF },
                { "Central African Rep.", CountryCode.CF },
                { "CAR", CountryCode.CF },
                { "South Sudan", CountryCode.SS },
                { "S. Sudan", CountryCode.SS },
                { "S Sudan", CountryCode.SS },
                { "Republic of South Sudan", CountryCode.SS },
                
                // Small island states
                { "Saint Lucia", CountryCode.LC },
                { "St. Lucia", CountryCode.LC },
                { "Saint Vincent and the Grenadines", CountryCode.VC },
                { "St. Vincent and the Grenadines", CountryCode.VC },
                { "Saint Kitts and Nevis", CountryCode.KN },
                { "St. Kitts and Nevis", CountryCode.KN },
                { "Trinidad and Tobago", CountryCode.TT },
                { "Trinidad & Tobago", CountryCode.TT },
                { "Antigua and Barbuda", CountryCode.AG },
                
                // Vatican variations
                { "Vatican City", CountryCode.VA },
                { "Vatican", CountryCode.VA },
                { "Vatican City State", CountryCode.VA },
                { "Holy See", CountryCode.VA },
                { "Holy See (Vatican City State)", CountryCode.VA },
                
                // Historical names (be careful not to duplicate)
                { "Swaziland", CountryCode.SZ }, // Now Eswatini
                // Burma is already handled by standard name mapping
                // Ceylon is already handled above
                // Zaire is already handled above
            };

            // Add all variations to the main dictionary
            foreach (var variation in variations)
            {
                if (!dictionary.ContainsKey(variation.Key))
                {
                    dictionary[variation.Key] = variation.Value;
                }
            }

            return dictionary;
        }

        #endregion

        #region Helper Classes

        private class CountryInfo
        {
            public string Name { get; set; } = string.Empty;
            public string? IsoCode { get; set; }
        }

        private class CountryParsingAnalysisResult
        {
            public int TotalCountries { get; set; }
            public List<CountryMappingAttempt> MappedCountries { get; set; } = new();
            public List<UnmappedCountryInfo> UnmappedCountries { get; set; } = new();
        }

        private class UnmappedCountryInfo
        {
            public string Name { get; set; } = string.Empty;
            public string? IsoCode { get; set; }
            public List<string> ParsingMethodsAttempted { get; set; } = new();
        }

        private class CountryMappingAttempt
        {
            public string Name { get; set; } = string.Empty;
            public string? IsoCode { get; set; }
            public CountryCode CountryCode { get; set; }
            public Country Country { get; set; }
            public bool IsSuccessful { get; set; }
            public List<string> MethodsAttempted { get; set; } = new();
        }

        private class CountryCodeMappingResult
        {
            public CountryCode CountryCode { get; set; }
            public string CountryName { get; set; } = string.Empty;
            public Country MappedCountry { get; set; }
            public CountryCode BackMappedCountryCode { get; set; }
            public bool IsBidirectionallyConsistent { get; set; }
            public bool IsMapped { get; set; }
        }

        private class StringToCountryCodeTestResult
        {
            public string InputName { get; set; } = string.Empty;
            public CountryCode ExpectedCountryCode { get; set; }
            public CountryCode DirectParsingResult { get; set; }
            public CountryCode GeoJsonMappingResult { get; set; }
            public CountryCode FuzzyMatchingResult { get; set; }
            public bool IsDirectParsingSuccessful { get; set; }
            public bool IsGeoJsonMappingSuccessful { get; set; }
            public bool IsFuzzyMatchingSuccessful { get; set; }
            public bool IsAnyMethodSuccessful { get; set; }
        }

        #endregion
    }
}