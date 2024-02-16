// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.Database.Models.Source
{
    using DerDieDasAICore.Database.Models.Source;
    using FluentAssertions;

    [TestClass]
    public class LoadSourceDbTest
    {
        #region Methods

        [TestMethod]
        public void LoadSource()
        {
            var deContext = new DeContext();
            var list = deContext.Entries.ToList();
            list.Count.Should().BeGreaterThan(0);
        }

        #endregion Methods
    }
}