// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Extensions
{
    using System.Linq;

    public static class StringExtesion
    {
        #region Methods

        public static bool IsAnyNullOrEmpty(params string[] strings)
        {
            return strings.Any(string.IsNullOrEmpty);
        }

        #endregion Methods
    }
}