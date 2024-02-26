// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models.Source;

public partial class Translation
{
    #region Properties

    public int? IsGood { get; set; }

    public string Lexentry { get; set; }

    public int? Score { get; set; }

    public string Sense { get; set; }

    public string SenseNum { get; set; }

    public string TransList { get; set; }

    public string WrittenRep { get; set; }

    #endregion Properties
}