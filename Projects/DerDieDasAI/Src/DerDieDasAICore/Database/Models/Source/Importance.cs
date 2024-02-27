// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models.Source;

public partial class Importance
{
    #region Properties

    public double? Score { get; set; }

    public string Vocable { get; set; }

    public string WrittenRepGuess { get; set; }

    #endregion Properties

    #region Methods

    public override string ToString()
    {
        var message = $"{WrittenRepGuess}:Importance:{Score:0.##}";
        return message;
    }

    #endregion Methods
}