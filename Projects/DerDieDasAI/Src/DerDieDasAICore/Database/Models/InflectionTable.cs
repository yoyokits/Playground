using System;
using System.Collections.Generic;

namespace DerDieDasAICore.Database.Models;

public partial class InflectionTable
{
    public string Pos { get; set; }

    public int? Rank { get; set; }

    public string Number { get; set; }

    public string Mood { get; set; }

    public string Person { get; set; }

    public string Tense { get; set; }

    public string Voice { get; set; }

    public byte[] TenseName { get; set; }

    public string Case { get; set; }

    public byte[] Definiteness { get; set; }
}
