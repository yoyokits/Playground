// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.App.UI.Models
{
    using DerDieDasAIApp.UI.Models;
    using DerDieDasAICore.Database.Models;
    using FluentAssertions;
    using System.Linq;

    [TestClass]
    public class GenerateNounsTableProcessTest
    {
        #region Methods

        [TestMethod]
        public void Execute()
        {
            var process = new GenerateNounsTableProcess();
            process.Execute();
            var dictionaryDB = DictionaryContext.Instance;
            var nouns = dictionaryDB.Nouns.ToArray();
            nouns.Should().HaveCountGreaterThan(0);
        }

        #endregion Methods
    }
}