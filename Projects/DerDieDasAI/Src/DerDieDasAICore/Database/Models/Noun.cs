// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models
{
    using System.ComponentModel;

    public class Noun : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public Gender Gender { get; set; }

        public int Id { get; set; }

        public string Information { get; set; }

        public string Translation { get; set; }

        public string Word { get; set; }

        #endregion Properties
    }
}