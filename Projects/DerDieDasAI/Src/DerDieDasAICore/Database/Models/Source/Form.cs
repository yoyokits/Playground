// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.Database.Models.Source;

public partial class Form
{
    #region Properties

    public string Case { get; set; }

    public string Definiteness { get; set; }

    public string Inflection { get; set; }

    public string Lexentry { get; set; }

    public string Mood { get; set; }

    public string Number { get; set; }

    public string OtherWritten { get; set; }

    public string OtherWrittenFull { get; set; }

    public string Person { get; set; }

    public string Pos { get; set; }

    public int? Rank { get; set; }

    public string Tense { get; set; }

    public string Voice { get; set; }

    #endregion Properties
}