// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapViewer
{
    using System.Threading.Tasks;
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
            await SettingsService.SaveAsync();
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await SettingsService.LoadAsync();
        }

        #endregion Methods
    }
}