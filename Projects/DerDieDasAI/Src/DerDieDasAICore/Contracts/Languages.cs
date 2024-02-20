// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.Models
{
    using DerDieDasAICore.Contracts;
    using DerDieDasAICore.Database.Models;
    using System.Collections.Generic;

    public static class Languages
    {
        #region Fields

        #region Properties

        internal static IDictionary<string, Gender> StringToGenderDictionary { get; }

        #endregion Properties

        internal const string Feminine = "feminine";

        internal const string Masculine = "masculine";

        internal const string Neuter = "neuter";

        #endregion Fields

        #region Constructors

        static Languages()
        {
            GenderDictionary[Language.German] = GetGenderDictionary("die", "der", "das");
            GenderDictionary[Language.France] = GetGenderDictionary("la", "le", string.Empty);

            StringToGenderDictionary = new Dictionary<string, Gender>
            {
                [Languages.Feminine] = Gender.Feminine,
                [Languages.Masculine] = Gender.Masculine,
                [Languages.Neuter] = Gender.Neutral
            };
        }

        #endregion Constructors

        #region Properties

        public static IDictionary<Language, IDictionary<Gender, string>> GenderDictionary { get; } = new Dictionary<Language, IDictionary<Gender, string>>();

        #endregion Properties

        #region Methods

        public static Gender TextToGender(string genderText)
        {
            return genderText switch
            {
                Languages.Feminine => Gender.Feminine,
                Languages.Masculine => Gender.Masculine,
                Languages.Neuter => Gender.Neutral,
                _ => Gender.Neutral,
            };
        }

        private static IDictionary<Gender, string> GetGenderDictionary(string feminine, string masculine, string neutral)
        {
            var dict = new Dictionary<Gender, string>
            {
                [Gender.Feminine] = feminine,
                [Gender.Masculine] = masculine,
                [Gender.Neutral] = neutral
            };

            return dict;
        }

        #endregion Methods
    }
}