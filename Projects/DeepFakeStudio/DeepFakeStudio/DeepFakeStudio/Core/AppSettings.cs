namespace DeepFakeStudio.Core
{
    using System.Windows;

    /// <summary>
    /// Defines the <see cref="AppSettings" />.
    /// </summary>
    public class AppSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets the WindowHeight.
        /// </summary>
        public int WindowHeight { get; set; } = 1080;

        /// <summary>
        /// Gets or sets the WindowState.
        /// </summary>
        public WindowState WindowState { get; set; } = WindowState.Normal;

        /// <summary>
        /// Gets or sets the WindowWidth.
        /// </summary>
        public int WindowWidth { get; set; } = 1920;

        /// <summary>
        /// Gets or sets the WorkspacePath.
        /// </summary>
        public string WorkspacePath { get; set; }

        #endregion Properties
    }
}