// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.ViewModels
{
    using DerDieDasAICore.Common;
    using DerDieDasAICore.Database.Models;
    using DerDieDasAICore.Database.Models.Source;
    using DerDieDasAICore.Extensions;
    using DerDieDasAICore.Properties;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public class DataBaseViewModel : INotifyPropertyChanged
    {
        #region Fields

        private IList<Entry> _entries;

        private IList<Form> _forms;

        private IList<Importance> _importanceList;

        private IList<Noun> _nouns;

        private NamedItem _selectedTable;

        private IList<NamedItem> _tables;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public IList<Entry> Entries { get => _entries; set => this.Set(PropertyChanged, ref _entries, value); }

        public IList<Form> Forms { get => _forms; set => this.Set(PropertyChanged, ref _forms, value); }

        public IList<Importance> ImportanceList { get => _importanceList; set => this.Set(PropertyChanged, ref _importanceList, value); }

        public IList<Noun> Nouns { get => _nouns; set => this.Set(PropertyChanged, ref _nouns, value); }

        public NamedItem SelectedTable { get => _selectedTable; set => this.Set(PropertyChanged, ref _selectedTable, value); }

        public IList<NamedItem> Tables { get => _tables; set => this.Set(PropertyChanged, ref _tables, value); }

        #endregion Properties

        #region Constructors

        internal DataBaseViewModel()
        {
            DeContext.Instance.SavedChanges += OnDeContextSavedChanges;
            DictionaryContext.CreateInstance(Settings.Default.RootDirectory);
            DictionaryContext.Instance.SavedChanges += OnDictionaryContextSavedChanges;
            InitializeAsync();
        }

        private void OnDictionaryContextSavedChanges(object sender, Microsoft.EntityFrameworkCore.SavedChangesEventArgs e)
        {
            Nouns = DictionaryContext.Instance.Nouns.Take(1000).ToList();
        }

        #endregion Constructors

        #region Methods

        private async void InitializeAsync()
        {
            await Task.Run(() =>
            {
                Nouns = DictionaryContext.Instance.Nouns.Take(1000).ToList();
                Entries = DeContext.Instance.Entries.Take(1000).ToList();
                Forms = DeContext.Instance.Forms.Take(1000).ToList();
                ImportanceList = DeContext.Instance.Importances.Take(1000).ToList();
                Tables = new List<NamedItem>
                {
                    new("Entries", Entries),
                    new("Forms", Forms),
                    new("Importance List", ImportanceList),
                    new("Nouns", Nouns)
                };

                SelectedTable = Tables.FirstOrDefault();
            });
        }

        private void OnDeContextSavedChanges(object sender, Microsoft.EntityFrameworkCore.SavedChangesEventArgs e)
        {
            Entries = DeContext.Instance.Entries.Take(1000).ToList();
            Forms = DeContext.Instance.Forms.Take(1000).ToList();
            ImportanceList = DeContext.Instance.Importances.Take(1000).ToList();
        }

        #endregion Methods
    }
}