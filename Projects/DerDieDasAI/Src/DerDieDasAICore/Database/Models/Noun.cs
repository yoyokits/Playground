// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Noun : INotifyPropertyChanged, IEquatable<Noun>
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        [Column(Order = 2)]
        public string Gender { get; set; }

        [Column(Order = 0)]
        public int Id { get; set; }

        [Column(Order = 3)]
        public double Importance { get; set; }

        [Column(Order = 7)]
        public string Information { get; set; }

        [Column(Order = 4)]
        public string Pronounce { get; set; }

        [Column(Order = 6)]
        public string Sense { get; set; }

        [Column(Order = 5)]
        public string Translation { get; set; }

        [Column(Order = 1)]
        public string Word { get; set; }

        #endregion Properties

        #region Methods

        public bool Equals(Noun other)
        {
            if (this is null || other is null)
            {
                return false;
            }

            return this.Word == other.Word;
        }

        public override int GetHashCode()
        {
            return this.Word.GetHashCode();
        }

        public override string ToString()
        {
            var message = $"{this.Word}:Gender:{this.Gender};Pronounce:{this.Pronounce}";
            return message;
        }

        #endregion Methods
    }
}