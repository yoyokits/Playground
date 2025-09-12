// ========================================== //
// Developer: Auto Generated                  //
// ========================================== //
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using WorldMapControls.Infrastructure; // explicit
using WorldMapControls.Services;
using WorldMapControls.Models.Enums;

namespace WorldMapTest.Services
{
    [TestClass]
    public class UnmappedGeoJsonCountriesTests
    {
        [TestMethod]
        public void AllGeoJsonCountries_ShouldMapToKnownCountry()
        {
            var loader = new WorldMapControls.Infrastructure.ResourceLoader();
            var geo = loader.LoadGeoJsonAsync().GetAwaiter().GetResult();
            var analysis = GeoJsonCountryExtractor.AnalyzeCountryNames(geo);

            if (analysis.UnmappedCountries.Count > 0)
            {
                var hints = string.Join("\n", analysis.UnmappedCountries.Select(u => $"  \"{u}\" = Country.???,"));
                Assert.Fail($"Unmapped GeoJSON country names found (count={analysis.UnmappedCountries.Count}):\n{hints}");
            }

            analysis.MappedCount.Should().BeGreaterThan(150); // sanity
        }
    }
}
