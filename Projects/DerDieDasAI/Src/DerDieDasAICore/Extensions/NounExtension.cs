// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Extensions
{
    using DerDieDasAIApp.UI.Models;
    using DerDieDasAICore.Contracts;
    using DerDieDasAICore.Database.Models;

    internal static class NounExtension
    {
        #region Methods

        internal static string GetFriendlyName(this Noun noun, Language language)
        {
            var genderDict = Languages.GenderDictionary[language];
            var gender = Languages.TextToGender(noun.Gender);
            var genderByLanguage = genderDict[gender];
            return $"{genderByLanguage}-{noun.Word}";
        }

        internal static string GetPath(this Noun noun, Language language)
        {
            var friendlyName = GetFriendlyName(noun, language);
            return Path.Combine(language.ToString(), friendlyName);
        }

        #endregion Methods
    }
}