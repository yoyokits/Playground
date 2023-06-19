#nullable disable

namespace DeepFakeStudio
{
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        #region Constants

        internal const string _appName = nameof(DeepFakeStudio);

        #endregion Constants

        #region Fields

        private static Mutex _mutex;

        #endregion Fields

        #region Methods

        /// <summary>
        /// The OnStartup.
        /// </summary>
        /// <param name="e">The e<see cref="StartupEventArgs"/>.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, _appName, out createdNew);
            if (!createdNew)
            {
                // App is already running! Exiting the application
                MessageBox.Show("The application is already running", $"Open Secondary {nameof(DeepFakeStudio)} Not Allowed");
                Current.Shutdown();
            }

            base.OnStartup(e);
        }

        #endregion Methods
    }
}