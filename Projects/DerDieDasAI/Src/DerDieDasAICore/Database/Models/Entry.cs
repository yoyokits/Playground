using System;
using System.Collections.Generic;

namespace DerDieDasAICore.Database.Models;

public partial class Entry
{
    public string Lexentry { get; set; }

    public string Vocable { get; set; }

    public string WrittenRep { get; set; }

    public string PartOfSpeech { get; set; }

    public string Gender { get; set; }

    public string PronunList { get; set; }
}
