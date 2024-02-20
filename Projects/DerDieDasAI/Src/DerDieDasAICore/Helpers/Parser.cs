// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Helpers
{
    public static class Parser
    {
        #region Methods

        public static bool IsBaseWord(this string word)
        {
            return !string.IsNullOrEmpty(word) && !word.Contains('-') && !word.Contains(' ');
        }

        #endregion Methods
    }
}