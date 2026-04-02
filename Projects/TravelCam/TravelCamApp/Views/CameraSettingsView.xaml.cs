// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace TravelCamApp.Views
{
    public partial class CameraSettingsView : ContentView
    {
        /// <summary>Raised when the user closes the panel (× button or Done).</summary>
        public event EventHandler? CloseRequested;

        public CameraSettingsView()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object? sender, EventArgs e)
            => CloseRequested?.Invoke(this, EventArgs.Empty);

        private void OnBackdropTapped(object? sender, TappedEventArgs e)
            => CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
