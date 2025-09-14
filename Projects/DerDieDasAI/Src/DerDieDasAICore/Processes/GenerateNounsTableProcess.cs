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
    using DerDieDasAICore.Properties;
    using System.Linq;

    public class GenerateNounsTableProcess : ProcessItem
    {
        #region Fields

        private string _outputFolder = Settings.Default.RootDirectory;

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
            InsertNounsParameter(nouns);
            UpdateNounDBAsync(nouns);
        }

        internal Dictionary<string, Importance> GetImportanceDictionary()
        {
            var importances = DeContext.Instance.Importances.ToArray();
            var validImportances = importances.Where(item => item.WrittenRepGuess.IsBaseWord()).ToList();
            var importanceDict = validImportances.ToDictionary(i => i.WrittenRepGuess, i => i);
            return importanceDict;
        }

        internal List<Noun> GetNouns(IEnumerable<Entry> entries)
        {
            var nouns = new HashSet<Noun>();
            foreach (var entry in entries)
            {
                var noun = EntryToNoun(entry);
                if (noun != null)
                {
                    if (nouns.Contains(noun))
                    {
                        nouns.Remove(noun);
                    }
                    else
                    {
                        nouns.Add(noun);
                    }
                }
            }

            return nouns.ToList();
        }

        internal Dictionary<string, Translation> GetTranslationDictionary()
        {
            var translations = DeEnContext.Instance.Translations.ToArray();
            var validTranslations = translations.Where(item => item.WrittenRep.IsBaseWord()).ToList();
            var translationsDict = new Dictionary<string, Translation>(validTranslations.Count);
            foreach (var translation in validTranslations)
            {
                if (!translationsDict.ContainsKey(translation.WrittenRep))
                {
                    translationsDict[translation.WrittenRep] = translation;
                }
            }

            return translationsDict;
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

        private void InsertNounsParameter(IList<Noun> nouns)
        {
            var importanceDict = GetImportanceDictionary();
            var translationDict = GetTranslationDictionary();
            var invalidNouns = new List<Noun>();
            foreach (var noun in nouns)
            {
                var remove = false;
                if (importanceDict.TryGetValue(noun.Word, out var importance) && importance.Score != null)
                {
                    noun.Importance = importance.Score.Value;
                }

                if (translationDict.TryGetValue(noun.Word, out var translation))
                {
                    noun.Translation = translation.TransList;
                    noun.Sense = translation.Sense;
                    if (string.IsNullOrEmpty(noun.Translation))
                    {
                        remove = true;
                    }
                }
                else
                {
                    remove = true;
                }

                if (remove)
                {
                    invalidNouns.Add(noun);
                }
            }

            foreach (var invalidNoun in invalidNouns)
            {
                if (nouns.Contains(invalidNoun))
                {
                    nouns.Remove(invalidNoun);
                }
            }
        }

        #endregion Methods
    }
}