// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Loads embedded GeoJSON resources.
    /// </summary>
    public class ResourceLoader
    {
        #region Methods

        public async Task<string> LoadGeoJsonAsync()
        {
            // Try embedded first
            var controlAssembly = typeof(ResourceLoader).Assembly;
            var entryAssembly = Assembly.GetEntryAssembly();
            var assemblies = entryAssembly != null
                ? new[] { controlAssembly, entryAssembly }.Distinct()
                : new[] { controlAssembly };

            foreach (var asm in assemblies)
            {
                var content = TryLoadFromAssembly(asm);
                if (content != null)
                    return content;
            }

            // Try filesystem fallbacks
            var fileContent = TryLoadFromFileSystem();
            if (fileContent != null)
                return fileContent;

            throw new FileNotFoundException(BuildFailureReport(assemblies));
        }

        private static string? Read(Assembly asm, string name)
        {
            try
            {
                using var stream = asm.GetManifestResourceStream(name);
                if (stream == null) return null;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        private string BuildFailureReport(System.Collections.Generic.IEnumerable<Assembly> assemblies)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Could not locate world-countries.geo.json.");
            sb.AppendLine("Checked embedded resources in assemblies:");

            foreach (var asm in assemblies)
            {
                sb.AppendLine($"- {asm.GetName().Name}:");
                foreach (var r in asm.GetManifestResourceNames())
                    sb.AppendLine($"    {r}");
            }

            sb.AppendLine("Also checked filesystem relative to:");
            sb.AppendLine($"  AppContext.BaseDirectory: {AppContext.BaseDirectory}");
            sb.AppendLine($"  CurrentDirectory: {Directory.GetCurrentDirectory()}");
            return sb.ToString();
        }

        private string? TryLoadFromAssembly(Assembly assembly)
        {
            var resources = assembly.GetManifestResourceNames();
            // Preferred exact names
            var matches = new[]
            {
                "world-countries.geo.json",
                "world-countries.geojson"
            };

            foreach (var res in resources)
            {
                if (matches.Any(m => res.EndsWith(m, StringComparison.OrdinalIgnoreCase))
                    && res.Contains(".Maps.", StringComparison.OrdinalIgnoreCase))
                {
                    return Read(assembly, res);
                }
            }

            // Fallback loose match
            var loose = resources.FirstOrDefault(r =>
                r.EndsWith("world-countries.geo.json", StringComparison.OrdinalIgnoreCase) ||
                r.EndsWith("world-countries.geojson", StringComparison.OrdinalIgnoreCase));

            return loose != null ? Read(assembly, loose) : null;
        }

        private string? TryLoadFromFileSystem()
        {
            var candidates = new[]
            {
                "world-countries.geo.json",
                "world-countries.geojson"
            };

            string? Probe(string path) => File.Exists(path) ? File.ReadAllText(path) : null;

            var baseDirs = new[]
            {
                AppContext.BaseDirectory,
                AppDomain.CurrentDomain.BaseDirectory,
                Directory.GetCurrentDirectory()
            }
            .Distinct()
            .Where(Directory.Exists)
            .ToList();

            foreach (var dir in baseDirs)
            {
                foreach (var file in candidates)
                {
                    var direct = Path.Combine(dir, file);
                    var maps = Path.Combine(dir, "Maps", file);
                    var parent = Directory.GetParent(dir)?.FullName;
                    var parentMaps = parent != null ? Path.Combine(parent, "Maps", file) : null;

                    if (Probe(direct) is { } d) return d;
                    if (Probe(maps) is { } m) return m;
                    if (parentMaps != null && Probe(parentMaps) is { } pm) return pm;
                }
            }

            return null;
        }

        #endregion Methods
    }
}