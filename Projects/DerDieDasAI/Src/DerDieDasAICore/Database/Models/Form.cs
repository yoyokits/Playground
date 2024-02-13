using System;
using System.Collections.Generic;

namespace DerDieDasAICore.Database.Models;

public partial class Form
{
    public string Lexentry { get; set; }

    public string OtherWrittenFull { get; set; }

    public string Pos { get; set; }

    public int? Rank { get; set; }

    public string Number { get; set; }

    public string Mood { get; set; }

    public string Person { get; set; }

    public string Tense { get; set; }

    public string Voice { get; set; }

    public string Case { get; set; }

    public string Definiteness { get; set; }

    public string Inflection { get; set; }

    public string OtherWritten { get; set; }
}
