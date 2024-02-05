using System.ComponentModel;

namespace DerDieDasAICore.Db.Models
{
    public class Noun : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id { get; set; }

        public string Word { get; set; }

        public Gender Gender { get; set; }

        public string Information { get; set; }

        public string Translation { get; set; }
    }
}