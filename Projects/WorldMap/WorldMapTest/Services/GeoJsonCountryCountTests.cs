using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldMapControls.Infrastructure;
using WorldMapControls.Services;

namespace WorldMapTest.Services
{
    [TestClass]
    public class GeoJsonCountryCountTests
    {
        [TestMethod]
        public void PrintGeoJsonCountryCounts()
        {
            var loader = new WorldMapControls.Infrastructure.ResourceLoader();
            var geo = loader.LoadGeoJsonAsync().GetAwaiter().GetResult();
            var analysis = GeoJsonCountryExtractor.AnalyzeCountryNames(geo);
            System.Console.WriteLine($"Total unique country/territory feature names in GeoJSON: {analysis.TotalCountries}");
            System.Console.WriteLine($"Mapped: {analysis.MappedCount} Unmapped: {analysis.UnmappedCount}");
            // Simple assertion just to keep test green
            Assert.IsTrue(analysis.TotalCountries > 0);
        }
    }
}
