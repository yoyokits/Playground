// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Settings
{
    using LLM.Models.Enums;

    public sealed class AppSettings
    {
        #region Properties

        public bool EnterSends { get; set; } = true;
        public int MaxTokens { get; set; } = 16384; // ~16k tokens (16 * 1024)
        public string? SelectedModel { get; set; }
        public LLMProvider SelectedProvider { get; set; } = LLMProvider.GPT4All;
        public string SystemPrompt { get; set; } = "You are a helpful assistant.";
        public float Temperature { get; set; } = 0.7f;

        #endregion Properties
    }
}