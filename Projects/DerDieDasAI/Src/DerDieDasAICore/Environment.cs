// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore
{
    using System.Diagnostics;
    using System.Reflection;

    internal class Environment
    {
        #region Properties

        public static Environment Instance { get; } = new();

        #endregion Properties

        #region Constructors

        private Environment()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fileVersionInfo.ProductVersion;
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
        }

        #endregion Constructors



        #region Properties

        public string DbVersion { get; } = "2024.0.1";

        public string RootPath { get; set; } = @"C:\Temp\derdiedas";

        public string Version { get; set; }

        #endregion Properties

        internal void LoadSettings()
        {
            ////this.RootDirectory = Settings.Default.RootDirectory;
        }

        internal void SaveSettings()
        {
            ////Settings.
            ////Settings.Default.RootDirectory = this.RootDirectory;
            ////Settings.Default.Save();
        }
    }
}