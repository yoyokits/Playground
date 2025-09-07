// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles resource loading operations.
    /// </summary>
    public class ResourceLoader
    {
        #region Fields

        /// <summary>
        /// Defines the _possibleResourceNames
        /// </summary>
        private readonly string[] _possibleResourceNames =
        {
            "WorldMapApp.world-countries.geojson",
            "WorldMapApp.world-countries.geo.json",
            "world-countries.geojson",
            "world-countries.geo.json"
        };

        #endregion Fields

        #region Methods

        /// <summary>
        /// The LoadGeoJsonAsync.
        /// </summary>
        /// <returns>The <see cref="Task{string}"/>.</returns>
        public async Task<string> LoadGeoJsonAsync()
        {
            return await Task.Run(() =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                // Try exact matches first
                foreach (var possibleName in _possibleResourceNames)
                {
                    var content = TryLoadResource(assembly, possibleName);
                    if (content != null) return content;
                }

                // Try finding any GeoJSON resource
                var geoJsonResource = resourceNames.FirstOrDefault(name =>
                    name.Contains("geojson", StringComparison.OrdinalIgnoreCase));

                if (geoJsonResource != null)
                {
                    var content = TryLoadResource(assembly, geoJsonResource);
                    if (content != null) return content;
                }

                var available = string.Join(", ", resourceNames);
                throw new InvalidOperationException($"No GeoJSON resource found. Available: {available}");
            });
        }

        /// <summary>
        /// The TryLoadResource.
        /// </summary>
        /// <param name="assembly">The assembly<see cref="Assembly"/>.</param>
        /// <param name="resourceName">The resourceName<see cref="string"/>.</param>
        /// <returns>The <see cref="string?"/>.</returns>
        private string? TryLoadResource(Assembly assembly, string resourceName)
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