using System;
using System.Collections.Generic;

namespace DerDieDasAICore.Database.Models.Source;

public partial class RelImportance
{
    public string Vocable { get; set; }

    public byte[] Score { get; set; }

    public double? RelScore { get; set; }

    public byte[] WrittenRepGuess { get; set; }
}
