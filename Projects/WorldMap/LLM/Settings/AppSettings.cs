// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Settings
{
    using System.Collections.Generic;
    using LLM.Models.Enums;

    public sealed class AppSettings
    {
        #region Properties

        public int SchemaVersion { get; set; } // 0 = legacy, 2 = current
        public bool EnterSends { get; set; } = true;
        public int MaxTokens { get; set; } = 16384; // ~16k tokens (16 * 1024)
        public string? SelectedModel { get; set; }
        public LLMProvider SelectedProvider { get; set; } = LLMProvider.Ollama; // new default
        public string SystemPrompt { get; set; } = "You are a helpful assistant.";
        public float Temperature { get; set; } = 0.7f;
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public double WindowLeft { get; set; } = -1;
        public double WindowTop { get; set; } = -1;
        public bool WindowMaximized { get; set; }
        public Dictionary<LLMProvider, string?> ProviderModels { get; set; } = new();
        public double OutlineThickness { get; set; } = 1.0; // World map outline thickness slider (0-5 recommended). Default 1.0
        public string OutlineColor { get; set; } = "#8C8C8C"; // World map outline color in hex format. Default gray
        public string DefaultFillColor { get; set; } = "#F5F5F5"; // World map default fill color in hex format. Default light gray

        #endregion Properties
    }
}