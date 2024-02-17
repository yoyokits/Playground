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
        #region Constructors

        static Languages()
        {
            GenderDictionary[Language.German] = GetGenderDictionary("die", "der", "das");
            GenderDictionary[Language.France] = GetGenderDictionary("la", "le", string.Empty);
        }

        #endregion Constructors

        #region Properties

        public static IDictionary<Language, IDictionary<string, Gender>> GenderDictionary { get; } = new Dictionary<Language, IDictionary<string, Gender>>();

        #endregion Properties

        #region Methods

        private static IDictionary<string, Gender> GetGenderDictionary(string feminine, string masculine, string neutral)
        {
            var dict = new Dictionary<string, Gender>
            {
                [feminine] = Gender.Feminine,
                [masculine] = Gender.Masculine,
                [neutral] = Gender.Neutral
            };

            return dict;
        }

        #endregion Methods
    }
}