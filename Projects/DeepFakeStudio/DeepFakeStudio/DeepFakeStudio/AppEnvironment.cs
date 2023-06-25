﻿namespace DeepFakeStudio
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Defines the <see cref="AppEnvironment" />.
    /// </summary>
    public static class AppEnvironment
    {
        #region Constants

        public const string LongName = "Cekli Deep Fake Studio";

        public const string Name = "Deep Fake Studio";

        public const int ProgressUpdateDelay = 250;

        private const string BinariesDirectory = "Projects";

        private const string LogsDirectory = "Logs";

        #endregion Constants

        #region Fields

        public static readonly string ShortName = Assembly.GetExecutingAssembly().GetName().Name;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the AppBinaryDirectory.
        /// </summary>
        public static string AppBinaryDirectory => Path.Combine(AppDirectory, BinariesDirectory);

        /// <summary>
        /// Gets the AppDirectory.
        /// </summary>
        public static string AppDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Gets the Author.
        /// </summary>
        public static string Author => "Yohanes Wahyu Nurcahyo";

        /// <summary>
        /// Gets the HomeUrl.
        /// </summary>
        public static string HomeUrl => "https://yoyokits.github.io/BarChartRaceNet/welcome.html";

        /// <summary>
        /// Gets the ProjectUrl.
        /// </summary>
        public static string ProjectUrl { get; } = "https://github.com/yoyokits/BarChartRaceNet";

        /// <summary>
        /// Gets the UserDocumentsFolder.
        /// </summary>
        public static string UserDocumentsFolder { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Gets the UserLocalApplicationData.
        /// </summary>
        public static string UserLocalApplicationData { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Gets the UserProfile.
        /// </summary>
        public static string UserProfile { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        /// <summary>
        /// Gets the Version.
        /// </summary>
        public static string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version}";

        /// <summary>
        /// Gets the WorkspaceFolder.
        /// </summary>
        public static string WorkspaceFolder { get; } = $"{AppDirectory}workspace\\";

        #endregion Properties

        #region Methods

        /// <summary>
        /// Returns the local app data directory for this program. Also makes sure the directory exists.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetAppDataDirectory()
        {
            var path = Path.Combine(UserLocalApplicationData, AppEnvironment.ShortName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// The GetAppDataFolder.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetAppDataFolder() => GetFolder(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// The application folder.
        /// </summary>
        /// <param name="specialFolder">The specialFolder<see cref="Environment.SpecialFolder"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetFolder(Environment.SpecialFolder specialFolder) => Environment.GetFolderPath(specialFolder);

        /// <summary>
        /// Returns the logs directory for this program. Also makes sure the directory exists.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetLogsDirectory()
        {
            var path = Path.Combine(GetAppDataDirectory(), LogsDirectory);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// The GetUserLocalApplicationData.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetUserLocalApplicationData()
        {
            var userFolder = UserLocalApplicationData;
            var appFolder = Path.Combine(userFolder, ShortName);
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return appFolder;
        }

        #endregion Methods
    }
}