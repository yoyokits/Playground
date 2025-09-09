// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Settings
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    // NOTE: Made single declaration (removed duplicate partial class) to fix CS0260.
    public static class SettingsService
    {
        #region Fields

        private static readonly string AppDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WorldMapApp");

        private static readonly string SettingsPath = Path.Combine(AppDir, "settings.json");
        private static bool _isLoaded;

        #endregion Fields

        #region Events

        public static event EventHandler? SettingsChanged;

        #endregion Events

        #region Properties

        public static AppSettings Current { get; private set; } = new();
        public static bool IsLoaded => _isLoaded;

        #endregion Properties

        #region Methods

        public static AppSettings Clone() =>
            new()
            {
                SelectedModel = Current.SelectedModel,
                MaxTokens = Current.MaxTokens,
                Temperature = Current.Temperature,
                SystemPrompt = Current.SystemPrompt,
                EnterSends = Current.EnterSends
            };

        public static async Task LoadAsync()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = await File.ReadAllTextAsync(SettingsPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                        Current = loaded;
                }
            }
            catch
            {
                Current = new AppSettings();
            }
            finally
            {
                _isLoaded = true;
                SettingsChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static void RaiseChangedAndSave()
        {
            SettingsChanged?.Invoke(null, EventArgs.Empty);
            _ = SaveAsync();
        }

        public static async Task SaveAsync()
        {
            try
            {
                Directory.CreateDirectory(AppDir);
                var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SettingsPath, json);
            }
            catch
            {
                // swallow persistence errors
            }
        }

        public static void Update(AppSettings updated)
        {
            Current = updated;
            SettingsChanged?.Invoke(null, EventArgs.Empty);
            _ = SaveAsync();
        }

        public static async Task UpdateAsync(AppSettings updated)
        {
            Current = updated;
            SettingsChanged?.Invoke(null, EventArgs.Empty);
            await SaveAsync();
        }

        #endregion Methods
    }
}