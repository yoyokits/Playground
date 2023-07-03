namespace DeepFakeStudio.Core
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using DeepFakeStudio.Common;

    /// <summary>
    /// Defines the <see cref="AppSettingsController" />.
    /// </summary>
    public class AppSettingsController : NotifyPropertyChanged
    {
        #region Fields

        private AppSettings _appSettings = new();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the AppSettings.
        /// </summary>
        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set
            {
                if (_appSettings != value)
                {
                    _appSettings = value;
                    OnPropertyChanged(nameof(AppSettings));
                }
            }
        }

        /// <summary>
        /// Gets the SettingsFileName.
        /// </summary>
        public string SettingsFileName { get; } = "AppSettings.xml";

        /// <summary>
        /// Gets or sets the MessageHandler.
        /// </summary>
        internal MessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Gets the SettingsPath.
        /// </summary>
        private string SettingsPath => Path.Combine(AppEnvironment.AppDirectory, SettingsFileName);

        #endregion Properties

        #region Methods

        /// <summary>
        /// The Load.
        /// </summary>
        internal void Load()
        {
            var path = SettingsPath;
            if (!File.Exists(path))
            {
                return;
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var serializer = new XmlSerializer(typeof(AppSettings));
                var settings = serializer.Deserialize(fileStream) as AppSettings;
                if (settings != null)
                {
                    AppSettings = settings;
                    if (string.IsNullOrEmpty(AppSettings.WorkspaceFolder))
                    {
                        AppSettings.WorkspaceFolder = AppEnvironment.WorkspaceFolder;
                    }
                }
            }
            catch (Exception e)
            {
                var message = $"Open settings {path} failed";
                MessageHandler.SendError(message);
                MessageHandler.SendError(e.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        /// <summary>
        /// The Save.
        /// </summary>
        internal void Save()
        {
            var path = SettingsPath;
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                var serializer = new XmlSerializer(typeof(AppSettings));
                serializer.Serialize(fileStream, AppSettings);
            }
            catch (Exception e)
            {
                var message = $"Save settings {path} failed";
                MessageHandler.SendError(message);
                MessageHandler.SendError(e.Message);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        #endregion Methods
    }
}