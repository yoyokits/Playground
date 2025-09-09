// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Settings
{
    public sealed class AppSettings
    {
        #region Properties

        public bool EnterSends { get; set; } = true;
        public int MaxTokens { get; set; } = 512;
        public string? SelectedModel { get; set; }
        public string SystemPrompt { get; set; } = "You are a helpful assistant.";
        public float Temperature { get; set; } = 0.7f;

        #endregion Properties
    }
}