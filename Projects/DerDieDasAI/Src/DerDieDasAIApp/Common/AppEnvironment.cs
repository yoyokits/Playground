namespace DerDieDasAIApp.Common
{
    using DerDieDasAIApp.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class AppEnvironment : INotifyPropertyChanged
    {
        public static readonly AppEnvironment Instance = new AppEnvironment();

        public string RootDirectory { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        private AppEnvironment()
        {
            this.LoadSettings();
        }

        internal void SaveSettings()
        {
            Settings.Default.RootDirectory = this.RootDirectory;
            Settings.Default.Save();
        }

        internal void LoadSettings()
        {
            this.RootDirectory = Settings.Default.RootDirectory ;
        }
    }
}
