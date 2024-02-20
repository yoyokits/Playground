// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Noun : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        [Column(Order = 2)]
        public string Gender { get; set; }

        [Column(Order = 0)]
        public int Id { get; set; }

        public double Importance { get; set; }

        public string Information { get; set; }

        [Column(Order = 3)]
        public string Pronounce { get; set; }

        public string Translation { get; set; }

        [Column(Order = 1)]
        public string Word { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            var message = $"{this.Word}:Gender:{this.Gender};Pronounce:{this.Pronounce}";
            return message;
        }

        #endregion Methods
    }
}