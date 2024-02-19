// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore
{
    using System.Diagnostics;
    using System.Reflection;

    internal static class Environment
    {
        #region Constructors

        static Environment()
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

        public static string DbVersion { get; } = "2024.0.1";

        public static string RootPath { get; set; } = @"C:\Temp\derdiedas";

        public static string Version { get; set; }

        #endregion Properties
    }
}