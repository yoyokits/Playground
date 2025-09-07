// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Loads embedded GeoJSON resources.
    /// </summary>
    public class ResourceLoader
    {
        #region Methods

        public async Task<string> LoadGeoJsonAsync()
        {
            return await Task.Run(() =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName().Name;
                var resourceNames = assembly.GetManifestResourceNames();

                var candidates = new[]
                {
                    $"{assemblyName}.Maps.world-countries.geo.json",
                    $"{assemblyName}.Maps.world-countries.geojson",
                    $"{assemblyName}.world-countries.geo.json",
                    $"{assemblyName}.world-countries.geojson",
                    "Maps.world-countries.geo.json",
                    "Maps.world-countries.geojson",
                    "world-countries.geo.json",
                    "world-countries.geojson"
                };

                foreach (var name in candidates)
                {
                    var content = TryLoad(assembly, name);
                    if (content != null) return content;
                }

                var fallback = resourceNames.FirstOrDefault(r =>
                    r.Contains("geojson", StringComparison.OrdinalIgnoreCase) ||
                    r.Contains("geo.json", StringComparison.OrdinalIgnoreCase));

                if (fallback != null)
                {
                    var content = TryLoad(assembly, fallback);
                    if (content != null) return content;
                }

                throw new InvalidOperationException($"GeoJSON resource not found. Available: {string.Join(", ", resourceNames)}");
            });
        }

        private string? TryLoad(Assembly assembly, string resourceName)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) return null;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        #endregion Methods
    }
}