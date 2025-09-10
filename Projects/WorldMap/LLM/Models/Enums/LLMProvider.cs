// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Models.Enums
{
    /// <summary>
    /// Defines the available LLM providers.
    /// </summary>
    public enum LLMProvider
    {
        /// <summary>
        /// GPT4All local server (default port 4891).
        /// </summary>
        GPT4All = 0,

        /// <summary>
        /// Ollama local server (default port 11434).
        /// </summary>
        Ollama = 1
    }
}