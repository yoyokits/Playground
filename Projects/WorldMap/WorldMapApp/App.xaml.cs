// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapApp
{
    using System.Windows;
    using LLM.Settings;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        #region Methods

        protected override async void OnExit(ExitEventArgs e)
        {
            // ensure final bounds persisted and single forced save
            if (Current?.MainWindow is MainWindow mw)
                mw.PersistWindowBounds();
            await SettingsService.SaveAsync(force: true);
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingsService.ApplicationOwnsPersistence = true; // app controls when to write disk
            await SettingsService.LoadAsync();
            var win = new MainWindow();
            win.Show();
        }

        #endregion Methods
    }
}