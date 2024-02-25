// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Common
{
    public class NamedItem
    {
        #region Constructors

        public NamedItem(string name, object content)
        {
            this.Name = name;
            this.Content = content;
        }

        #endregion Constructors

        #region Properties

        public object Content { get; set; }

        public string Name { get; set; }

        #endregion Properties
    }
}