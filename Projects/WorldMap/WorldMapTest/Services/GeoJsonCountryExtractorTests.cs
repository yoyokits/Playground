// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using WorldMapControls.Services;
using System.Text.Json;

namespace WorldMapTest.Services
{
    [TestClass]
    public class GeoJsonCountryExtractorTests
    {
        private const string SampleGeoJson = @"{
            ""type"": ""FeatureCollection"",
            ""features"": [
                {
                    ""type"": ""Feature"",
                    ""properties"": {
                        ""NAME"": ""United States"",
                        ""ADMIN"": ""United States of America"",
                        ""ISO_A2"": ""US""
                    },
                    ""geometry"": {
                        ""type"": ""Polygon"",
                        ""coordinates"": [[]]
                    }
                },
                {
                    ""type"": ""Feature"",
                    ""properties"": {
                        ""NAME"": ""Germany"",
                        ""ADMIN"": ""Germany"",
                        ""ISO_A2"": ""DE""
                    },
                    ""geometry"": {
                        ""type"": ""Polygon"",
                        ""coordinates"": [[]]
                    }
                },
                {
                    ""type"": ""Feature"",
                    ""properties"": {
                        ""NAME"": ""Japan"",
                        ""name"": ""Japan"",
                        ""country"": ""Japan""
                    },
                    ""geometry"": {
                        ""type"": ""Polygon"",
                        ""coordinates"": [[]]
                    }
                }
            ]
        }";

        private const string CongoTestGeoJson = @"{
            ""type"": ""FeatureCollection"",
            ""features"": [
                {
                    ""type"": ""Feature"",
                    ""properties"": {
                        ""NAME"": ""Congo"",
                        ""ADMIN"": ""Republic of the Congo""
                    },
                    ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                },
                {
                    ""type"": ""Feature"",
                    ""properties"": {
                        ""NAME"": ""Congo, Dem. Rep."",
                        ""ADMIN"": ""Democratic Republic of the Congo""
                    },
                    ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                },
                {
                    ""type"": ""Feature"",
                    ""properties"": {
                        ""NAME"": ""Central African Rep."",
                        ""ADMIN"": ""Central African Republic""
                    },
                    ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[]] }
                }
            ]
        }";

        [TestMethod]
        public void ExtractAllCountryNames_WithValidGeoJson_ShouldReturnCountryNames()
        {
            // Act
            var countryNames = GeoJsonCountryExtractor.ExtractAllCountryNames(SampleGeoJson);

            // Assert
            countryNames.Should().NotBeEmpty("Should extract country names from GeoJSON");
            countryNames.Should().Contain("United States");
            countryNames.Should().Contain("Germany");
            countryNames.Should().Contain("Japan");
        }

        [TestMethod]
        public void ExtractAllCountryNames_WithMultiplePropertyNames_ShouldFindAllVariations()
        {
            // Act
            var countryNames = GeoJsonCountryExtractor.ExtractAllCountryNames(SampleGeoJson);

            // Assert - Should find countries from different property names
            countryNames.Should().Contain("United States"); // from NAME
            countryNames.Should().Contain("United States of America"); // from ADMIN
            countryNames.Should().Contain("Germany"); // from NAME and ADMIN
            countryNames.Should().Contain("Japan"); // from NAME, name, and country
            
            // Should be sorted
            var sortedNames = countryNames.OrderBy(n => n).ToList();
            countryNames.Should().BeEquivalentTo(sortedNames, options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void ExtractAllCountryNames_WithEmptyGeoJson_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyGeoJson = @"{""type"": ""FeatureCollection"", ""features"": []}";

            // Act
            var countryNames = GeoJsonCountryExtractor.ExtractAllCountryNames(emptyGeoJson);

            // Assert
            countryNames.Should().BeEmpty();
        }

        [TestMethod]
        public void ExtractAllCountryNames_WithInvalidGeoJson_ShouldReturnEmptyList()
        {
            // Act & Assert
            GeoJsonCountryExtractor.ExtractAllCountryNames("invalid json").Should().BeEmpty();
            GeoJsonCountryExtractor.ExtractAllCountryNames("").Should().BeEmpty();
            GeoJsonCountryExtractor.ExtractAllCountryNames(null!).Should().BeEmpty();
        }

        [TestMethod]
        public void ExtractAllCountryNames_WithMissingProperties_ShouldHandleGracefully()
        {
            // Arrange
            var geoJsonWithMissingProps = @"{
                ""type"": ""FeatureCollection"",
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""geometry"": {
                            ""type"": ""Polygon"",
                            ""coordinates"": [[]]
                        }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {},
                        ""geometry"": {
                            ""type"": ""Polygon"",
                            ""coordinates"": [[]]
                        }
                    }
                ]
            }";

            // Act
            var countryNames = GeoJsonCountryExtractor.ExtractAllCountryNames(geoJsonWithMissingProps);

            // Assert
            countryNames.Should().BeEmpty("Should handle features without country name properties");
        }

        [TestMethod]
        public void AnalyzeCountryNames_WithValidGeoJson_ShouldAnalyzeMappings()
        {
            // Act
            var analysis = GeoJsonCountryExtractor.AnalyzeCountryNames(SampleGeoJson);

            // Assert
            analysis.Should().NotBeNull("Analysis result should not be null");
            // Note: Specific structure depends on implementation
        }

        [TestMethod]
        public void AnalyzeCountryNames_WithCongoGeoJson_ShouldHandleCongoCorrectly()
        {
            // Act  
            var analysis = GeoJsonCountryExtractor.AnalyzeCountryNames(CongoTestGeoJson);

            // Assert
            analysis.Should().NotBeNull("Congo analysis should not be null");
            // Note: Specific validation depends on GeoJsonAnalysisResult implementation
        }

        [TestMethod]
        public void GenerateCountryNamesReport_WithValidGeoJson_ShouldGenerateReport()
        {
            // Act
            var report = GeoJsonCountryExtractor.GenerateCountryNamesReport(SampleGeoJson);

            // Assert
            report.Should().NotBeNullOrEmpty("Should generate a report");
            report.Should().Contain("United States", "Report should contain country names");
            report.Should().Contain("Germany", "Report should contain country names");
            report.Should().Contain("Japan", "Report should contain country names");
        }

        [TestMethod]
        public void GenerateCountryNamesReport_ShouldShowMappingStatus()
        {
            // Act
            var report = GeoJsonCountryExtractor.GenerateCountryNamesReport(CongoTestGeoJson);

            // Assert
            report.Should().NotBeNullOrEmpty();
            
            // Should show mapped countries with checkmarks or positive indicators
            report.Should().Contain("Congo", "Should include Congo in report");
            report.Should().Contain("Congo, Dem. Rep.", "Should include DRC variant in report");
            report.Should().Contain("Central African Rep.", "Should include CAR variant in report");
            
            // Report should show mapping status with visual indicators
            report.Should().MatchRegex(@"(FIXED|BROKEN|SUCCESS|MAPPED|\+|\-)", "Report should contain status indicators");
        }

        [TestMethod]
        public void ExtractAllCountryNames_ShouldHandleDifferentPropertyNames()
        {
            // Arrange - GeoJSON with various property name patterns
            var variousPropsGeoJson = @"{
                ""type"": ""FeatureCollection"",
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""NAME"": ""Country1"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""ADMIN"": ""Country2"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""name"": ""Country3"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""admin"": ""Country4"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""NAME_EN"": ""Country5"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""COUNTRY"": ""Country6"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""Country"": ""Country7"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""country"": ""Country8"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": { ""NAME_LONG"": ""Country9"" },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    }
                ]
            }";

            // Act
            var countryNames = GeoJsonCountryExtractor.ExtractAllCountryNames(variousPropsGeoJson);

            // Assert
            countryNames.Should().Contain("Country1", "Should extract from NAME");
            countryNames.Should().Contain("Country2", "Should extract from ADMIN");
            countryNames.Should().Contain("Country3", "Should extract from name");
            countryNames.Should().Contain("Country4", "Should extract from admin");
            countryNames.Should().Contain("Country5", "Should extract from NAME_EN");
            countryNames.Should().Contain("Country6", "Should extract from COUNTRY");
            countryNames.Should().Contain("Country7", "Should extract from Country");
            countryNames.Should().Contain("Country8", "Should extract from country");
            countryNames.Should().Contain("Country9", "Should extract from NAME_LONG");
            
            countryNames.Count.Should().Be(9, "Should extract all 9 unique country names");
        }

        [TestMethod]
        public void ExtractAllCountryNames_ShouldDeduplicateNames()
        {
            // Arrange - GeoJSON with duplicate country names in different properties
            var duplicateNamesGeoJson = @"{
                ""type"": ""FeatureCollection"",
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""United States"",
                            ""ADMIN"": ""United States"",
                            ""NAME_EN"": ""United States""
                        },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    },
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": ""Germany"",
                            ""ADMIN"": ""Germany""
                        },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    }
                ]
            }";

            // Act
            var countryNames = GeoJsonCountryExtractor.ExtractAllCountryNames(duplicateNamesGeoJson);

            // Assert
            countryNames.Should().Contain("United States");
            countryNames.Should().Contain("Germany");
            countryNames.Count.Should().Be(2, "Should deduplicate country names");
        }

        [TestMethod]
        public void ExtractAllCountryNames_WithNullOrEmptyPropertyValues_ShouldSkipThem()
        {
            // Arrange
            var nullPropsGeoJson = @"{
                ""type"": ""FeatureCollection"",
                ""features"": [
                    {
                        ""type"": ""Feature"",
                        ""properties"": {
                            ""NAME"": null,
                            ""ADMIN"": """",
                            ""name"": ""   "",
                            ""country"": ""ValidCountry""
                        },
                        ""geometry"": { ""type"": ""Point"", ""coordinates"": [0, 0] }
                    }
                ]
            }";

            // Act
            var countryNames = GeoJsonCountryExtractor.ExtractAllCountryNames(nullPropsGeoJson);

            // Assert
            countryNames.Should().OnlyContain(name => !string.IsNullOrWhiteSpace(name));
            countryNames.Should().Contain("ValidCountry");
            countryNames.Should().HaveCount(1);
        }
    }
}