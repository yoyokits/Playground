// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.Models
{
    using DerDieDasAICore.Database.Models;
    using DerDieDasAICore.Database.Models.Source;
    using DerDieDasAICore.Extensions;
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
            if (StringExtesion.IsAnyNullOrEmpty(entry.Gender, entry.WrittenRep, entry.PartOfSpeech) || entry.WrittenRep.Contains('-') || entry.WrittenRep.Contains(' ') || !entry.PartOfSpeech.Equals("noun", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return new Noun
            {
                Word = entry.WrittenRep,
                Gender = entry.Gender,
                Pronounce = entry.PronunList
            };
        }

        internal override void Execute()
        {
            var nouns = GetNouns();
            UpdateNounDBAsync(nouns);
        }

        internal IList<Noun> GetNouns()
        {
            var source = DeContext.Instance;
            var words = source.Entries.ToArray();
            var nouns = new HashSet<Noun>();
            foreach (var entry in words)
            {
                var noun = EntryToNoun(entry);
                if (noun != null && !nouns.Contains(noun))
                {
                    nouns.Add(noun);
                }
            }

            return nouns.ToArray();
        }

        private static async void UpdateNounDBAsync(IList<Noun> nounsSource)
        {
            var dictionaryDB = DictionaryContext.Instance;
            var nounsTarget = dictionaryDB.Nouns;
            var nouns = nounsTarget.ToList();
            var nounWords = nouns.Select(noun => noun.Word).ToHashSet();
            foreach (var noun in nounsSource)
            {
                if (!nounWords.Contains(noun.Word))
                {
                    nounsTarget.Add(noun);
                }
            }

            await dictionaryDB.SaveChangesAsync().ConfigureAwait(false);
        }

        #endregion Methods
    }
}