// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Loads embedded world-countries GeoJSON from the control library or the entry assembly.
    /// </summary>
    public class ResourceLoader
    {
        #region Fields

        // Canonical file base (without path variations)
        private const string BaseFileName = "world-countries.geo.json";

        // Expected (primary) manifest name given your csproj settings
        // <EmbeddedResource Include="Maps\world-countries.geo.json" />
        // Assembly root namespace: WorldMapControls
        private static readonly string[] ExpectedNames =
        {
            "WorldMapControls.Maps.world-countries.geo.json",
            "WorldMapControls.Maps.world-countries.geojson",
            "WorldMapControls.world-countries.geo.json",
            "WorldMapControls.world-countries.geojson",
            "Maps.world-countries.geo.json",
            "Maps.world-countries.geojson",
            "world-countries.geo.json",
            "world-countries.geojson"
        };

        #endregion Fields

        #region Methods

        public async Task<string> LoadGeoJsonAsync(bool includeDiagnostics = false)
        {
            return await Task.Run(() =>
            {
                // 1. Assemblies to probe: control library + entry (app) assembly
                var controlAssembly = typeof(ResourceLoader).Assembly;
                var entryAssembly = Assembly.GetEntryAssembly();
                var assemblies = entryAssembly != null
                    ? new[] { controlAssembly, entryAssembly }.Distinct()
                    : new[] { controlAssembly };

                // 2. Try expected names first
                foreach (var asm in assemblies)
                {
                    var names = asm.GetManifestResourceNames();
                    foreach (var candidate in ExpectedNames)
                    {
                        var match = names.FirstOrDefault(n =>
                            string.Equals(n, candidate, StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            var content = Read(asm, match);
                            if (content != null) return content;
                        }
                    }
                }

                // 3. Fallback: any resource that ends with the base file name
                foreach (var asm in assemblies)
                {
                    var match = asm.GetManifestResourceNames()
                        .FirstOrDefault(n =>
                            n.EndsWith(BaseFileName, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        var content = Read(asm, match);
                        if (content != null) return content;
                    }
                }

                // 4. Try physical file locations (if switched to Copy to Output in future)
                var fsContent = TryFileSystem();
                if (fsContent != null) return fsContent;

                // 5. Diagnostics
                var sb = new StringBuilder();
                sb.AppendLine("GeoJSON resource not found.");
                sb.AppendLine("Looked for (case-insensitive):");
                foreach (var n in ExpectedNames) sb.AppendLine("  - " + n);
                sb.AppendLine("Assemblies probed & their resource names:");
                foreach (var asm in assemblies)
                {
                    sb.AppendLine($"Assembly: {asm.GetName().Name}");
                    foreach (var r in asm.GetManifestResourceNames())
                        sb.AppendLine("  * " + r);
                }

                if (!includeDiagnostics)
                    sb.AppendLine("Hint: Pass includeDiagnostics=true to see this detail at runtime.");

                throw new InvalidOperationException(sb.ToString());
            });
        }

        private static string? Read(Assembly asm, string name)
        {
            try
            {
                using var s = asm.GetManifestResourceStream(name);
                if (s == null) return null;
                using var r = new StreamReader(s);
                return r.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        private static string? TryFileSystem()
        {
            // Fallback if later you switch to CopyToOutputDirectory
            var fileCandidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Maps", BaseFileName),
                Path.Combine(AppContext.BaseDirectory, BaseFileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Maps", BaseFileName),
                Path.Combine(Directory.GetCurrentDirectory(), BaseFileName)
            };
            foreach (var path in fileCandidates)
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }
            return null;
        }

        #endregion Methods
    }
}