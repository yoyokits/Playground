// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAIApp.UI.Models
{
    using DerDieDasAICore.Database.Models;
    using DerDieDasAICore.Database.Models.Source;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GenerateNounsTableProcess : ProcessItem
    {
        #region Constructors

        public GenerateNounsTableProcess()
        {
            this.Name = "Generate Nouns Table";
        }

        #endregion Constructors

        #region Methods

        internal Noun EntryToNoun(Entry entry)
        {
            var noun = new Noun { };
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