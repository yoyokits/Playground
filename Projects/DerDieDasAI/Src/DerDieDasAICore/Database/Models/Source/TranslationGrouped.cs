// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models.Source;

public partial class TranslationGrouped
{
    #region Properties

    public string Lexentry { get; set; }

    public string MinSenseNum { get; set; }

    public double? Score { get; set; }

    public string SenseList { get; set; }

    public byte[] TransList { get; set; }

    public string WrittenRep { get; set; }

    #endregion Properties
}