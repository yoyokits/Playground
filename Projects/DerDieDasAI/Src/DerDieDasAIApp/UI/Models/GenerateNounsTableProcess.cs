// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.Models
{
    using DerDieDasAICore.Database.Models;
    using DerDieDasAICore.Database.Models.Source;
    using System.Linq;

    public class GenerateNounsTableProcess : ProcessItem
    {
        #region Constructors

        public GenerateNounsTableProcess()
        {
            Name = "Generate Nouns Table";
        }

        #endregion Constructors

        #region Methods

        internal Noun EntryToNoun(Entry entry)
        {
            var noun = new Noun
            {
                Word = entry.
            };
            return noun;
        }

        internal override void Execute()
        {
            var source = new DeContext();
            var words = source.Entries.ToArray();
            foreach (var entry in words)
            {
            }
        }

        #endregion Methods
    }
}