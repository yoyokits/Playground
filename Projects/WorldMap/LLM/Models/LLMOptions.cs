// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Models
{
    using LLM.Models.Enums;

    public sealed class LLMOptions
    {
        #region Properties

        public int MaxTokens { get; init; } = 16384; // ~16k tokens (16 * 1024)
        public LLMProvider Provider { get; init; } = LLMProvider.GPT4All;
        public string? SelectedModel { get; init; }
        public string SystemPrompt { get; init; } = "You are a helpful assistant.";
        public float Temperature { get; init; } = 0.7f;

        #endregion Properties
    }
}