using System;
using System.Collections.Generic;

namespace DerDieDasAICore.Database.Models;

public partial class Importance
{
    public string Vocable { get; set; }

    public double? Score { get; set; }

    public string WrittenRepGuess { get; set; }
}
