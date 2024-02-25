// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.Models
{
    using DerDieDasAICore.Database.Models;
    using DerDieDasAICore.Database.Models.Source;
    using DerDieDasAICore.Extensions;
    using DerDieDasAICore.Helpers;
    using System.Linq;

    public class GenerateNounsTableProcess : ProcessItem
    {
        #region Fields

        private string _outputFolder = @"C:\Temp";

        #endregion Fields

        #region Properties

        public string OutputFolder { get => _outputFolder; set => _outputFolder = value; }

        #endregion Properties

        #region Constructors

        public GenerateNounsTableProcess()
        {
            Name = "Generate Nouns Table";
        }

        #endregion Constructors

        #region Methods

        internal static Noun EntryToNoun(Entry entry)
        {
            var word = entry.WrittenRep;
            if (StringExtesion.IsAnyNullOrEmpty(entry.Gender, word, entry.PartOfSpeech)
                || !word.IsBaseWord()
                || !entry.PartOfSpeech.Equals("noun", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return new Noun
            {
                Word = word,
                Gender = entry.Gender,
                Pronounce = entry.PronunList
            };
        }

        internal override void Execute()
        {
            var source = DeContext.Instance;
            var entries = source.Entries.ToArray();
            var nouns = GetNouns(entries);
            var importances = source.Importances.Where(item => item.WrittenRepGuess.IsBaseWord()).ToList();
            var importanceDict = importances.ToDictionary(i => i.WrittenRepGuess, i => i);
            InsertNounsParameter(nouns, importanceDict);
            UpdateNounDBAsync(nouns);
        }

        internal IList<Noun> GetNouns(IEnumerable<Entry> entries)
        {
            var nouns = new HashSet<Noun>();
            foreach (var entry in entries)
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

        private void InsertNounsParameter(IList<Noun> nouns, Dictionary<string, Importance> importanceDict)
        {
            foreach (var noun in nouns)
            {
                if (importanceDict.TryGetValue(noun.Word, out var importance) && importance.Score != null)
                {
                    noun.Importance = importance.Score.Value;
                }
            }
        }

        #endregion Methods
    }
}