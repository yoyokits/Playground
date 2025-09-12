// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
using System.IO;
using System.Text.Json;
using LLM.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldMapTest.Settings
{
    [TestClass]
    public class SettingsPersistenceTests
    {
        private string _tempDir = null!;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "WorldMapApp_TestSettings" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            Environment.SetEnvironmentVariable("WORLD_MAP_APP_SETTINGS_DIR", _tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); } catch { }
            Environment.SetEnvironmentVariable("WORLD_MAP_APP_SETTINGS_DIR", null);
        }

        [TestMethod]
        public async Task Settings_SaveAndLoad_ShouldPersistAllScalarProperties()
        {
            var s = new AppSettings
            {
                SchemaVersion = 2,
                EnterSends = false,
                MaxTokens = 9999,
                SelectedModel = "test-model",
                SystemPrompt = "prompt",
                Temperature = 0.42f,
                WindowWidth = 1234,
                WindowHeight = 777,
                WindowLeft = 222,
                WindowTop = 333,
                WindowMaximized = true,
                SelectedProvider = LLM.Models.Enums.LLMProvider.Ollama
            };
            s.ProviderModels[LLM.Models.Enums.LLMProvider.Ollama] = "test-model";
            SettingsService.Update(s);
            await SettingsService.SaveAsync();

            // fresh load
            SettingsService.Update(new AppSettings());
            await SettingsService.LoadAsync();
            var re = SettingsService.Current;
            re.SchemaVersion.Should().Be(s.SchemaVersion);
            re.EnterSends.Should().Be(s.EnterSends);
            re.MaxTokens.Should().Be(s.MaxTokens);
            re.SelectedModel.Should().Be(s.SelectedModel);
            re.SystemPrompt.Should().Be(s.SystemPrompt);
            re.Temperature.Should().Be(s.Temperature);
            re.WindowWidth.Should().Be(s.WindowWidth);
            re.WindowHeight.Should().Be(s.WindowHeight);
            re.WindowLeft.Should().Be(s.WindowLeft);
            re.WindowTop.Should().Be(s.WindowTop);
            re.WindowMaximized.Should().Be(s.WindowMaximized);
            re.SelectedProvider.Should().Be(s.SelectedProvider);
            re.ProviderModels.Should().ContainKey(s.SelectedProvider);
            re.ProviderModels[s.SelectedProvider].Should().Be("test-model");
        }
    }
}
