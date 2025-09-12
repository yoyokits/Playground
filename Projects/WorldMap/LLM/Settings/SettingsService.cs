// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Settings
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using LLM.Models.Enums;

    public static class SettingsService
    {
        #region Fields

        private const int CurrentSchemaVersion = 2;
        private const string OverrideVar = "WORLD_MAP_APP_SETTINGS_DIR";
        private static bool _isLoaded;
        private static int _saveCount;

        #endregion Fields

        #region Events

        public static event EventHandler? SettingsChanged;

        #endregion Events

        #region Properties

        public static AppSettings Current { get; private set; } = new();
        public static bool IsLoaded => _isLoaded;

        // Expose settings file path for diagnostics
        public static string SettingsPath => ResolveSettingsPath();

        // When true, automatic saves from library (SetModel/SetProvider/Update) are suppressed.
        public static bool ApplicationOwnsPersistence { get; set; }

        #endregion Properties

        #region Methods

        public static AppSettings Clone() => new()
        {
            SelectedProvider = Current.SelectedProvider,
            SelectedModel = Current.SelectedModel,
            MaxTokens = Current.MaxTokens,
            Temperature = Current.Temperature,
            SystemPrompt = Current.SystemPrompt,
            EnterSends = Current.EnterSends,
            WindowWidth = Current.WindowWidth,
            WindowHeight = Current.WindowHeight,
            WindowLeft = Current.WindowLeft,
            WindowTop = Current.WindowTop,
            WindowMaximized = Current.WindowMaximized,
            SchemaVersion = Current.SchemaVersion,
            ProviderModels = new System.Collections.Generic.Dictionary<LLMProvider, string?>(Current.ProviderModels),
            OutlineThickness = Current.OutlineThickness,
            OutlineColor = Current.OutlineColor,
            DefaultFillColor = Current.DefaultFillColor
        };

        // Diagnostic dump
        public static void Dump(string tag)
        {
            try
            {
                var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
                Debug.WriteLine($"[Settings Dump] {tag} (SaveCount={_saveCount})");
                Debug.WriteLine($" Path: {SettingsPath}");
                Debug.WriteLine(json);
                Debug.WriteLine($" Window Bounds -> Left:{Current.WindowLeft} Top:{Current.WindowTop} Width:{Current.WindowWidth} Height:{Current.WindowHeight} Maximized:{Current.WindowMaximized}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Settings Dump] {tag} failed: {ex.Message}");
            }
        }

        public static async Task LoadAsync()
        {
            var path = ResolveSettingsPath();
            try
            {
                if (File.Exists(path))
                {
                    var json = await File.ReadAllTextAsync(path);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null) Current = loaded;
                }
            }
            catch
            {
                Current = new AppSettings();
            }
            finally
            {
                MigrateIfNeeded();
                if (string.IsNullOrWhiteSpace(Current.SelectedModel) &&
                    Current.ProviderModels.TryGetValue(Current.SelectedProvider, out var providerModel))
                    Current.SelectedModel = providerModel;

                _isLoaded = true;
                SettingsChanged?.Invoke(null, EventArgs.Empty);
                Dump("After Load");
            }
        }

        private static void MigrateIfNeeded()
        {
            var legacy = Current.SchemaVersion == 0;
            if (legacy)
            {
                if (Current.SelectedProvider == LLMProvider.GPT4All && string.IsNullOrWhiteSpace(Current.SelectedModel))
                    Current.SelectedProvider = LLMProvider.Ollama;
                Current.SchemaVersion = CurrentSchemaVersion;
            }
            else if (Current.SchemaVersion < CurrentSchemaVersion)
            {
                Current.SchemaVersion = CurrentSchemaVersion;
            }
        }

        private static void RaiseChanged(bool save)
        {
            SettingsChanged?.Invoke(null, EventArgs.Empty);
            if (save && !ApplicationOwnsPersistence)
                _ = SaveAsync();
        }

        public static void RaiseChangedAndSave() => RaiseChanged(true);

        public static async Task SaveAsync(bool force = false)
        {
            if (ApplicationOwnsPersistence && !force) { Dump("Save Suppressed (AppOwned)"); return; }
            try
            {
                var dir = ResolveAppDir();
                Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(ResolveSettingsPath(), json);
                _saveCount++;
                Dump("After Save");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Settings] Save failed: {ex.Message}");
            }
        }

        public static void Update(AppSettings updated)
        {
            Current = updated;
            RaiseChanged(true);
        }

        public static async Task UpdateAsync(AppSettings updated)
        {
            Current = updated;
            SettingsChanged?.Invoke(null, EventArgs.Empty);
            if (!ApplicationOwnsPersistence) await SaveAsync();
        }

        public static void SetProvider(LLMProvider provider)
        {
            if (Current.SelectedProvider == provider) return;
            if (!string.IsNullOrWhiteSpace(Current.SelectedModel))
                Current.ProviderModels[Current.SelectedProvider] = Current.SelectedModel;
            Current.SelectedProvider = provider;
            Current.SelectedModel = Current.ProviderModels.TryGetValue(provider, out var model) ? model : null;
            RaiseChanged(true);
        }

        public static void SetModel(string? model)
        {
            if (Current.SelectedModel == model) return;
            Current.SelectedModel = model;
            Current.ProviderModels[Current.SelectedProvider] = model;
            RaiseChanged(true);
        }

        public static void SetOutlineThickness(double thickness)
        {
            if (Math.Abs(Current.OutlineThickness - thickness) < 0.001) return;
            Current.OutlineThickness = thickness;
            RaiseChanged(true);
        }

        public static void SetOutlineColor(string color)
        {
            if (Current.OutlineColor == color) return;
            Current.OutlineColor = color;
            RaiseChanged(true);
        }

        public static void SetDefaultFillColor(string color)
        {
            if (Current.DefaultFillColor == color) return;
            Current.DefaultFillColor = color;
            RaiseChanged(true);
        }

        private static string ResolveAppDir()
        {
            var custom = Environment.GetEnvironmentVariable(OverrideVar);
            if (!string.IsNullOrWhiteSpace(custom)) return custom;
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldMapApp");
        }

        private static string ResolveSettingsPath() => System.IO.Path.Combine(ResolveAppDir(), "settings.json");

        #endregion Methods
    }
}