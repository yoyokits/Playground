using TravelCamApp.ViewModels;
using TravelCamApp.Models;

namespace TravelCamApp.Views
{
    public partial class SensorValueSettingsView : ContentView
    {
        private SensorValueSettingsViewModel? _viewModel;

        private SensorValueSettingsViewModel ViewModel =>
            _viewModel
            ?? (BindingContext as SensorValueSettingsViewModel)
            ?? throw new InvalidOperationException("SensorValueSettingsView ViewModel not set");

        public SensorValueSettingsView()
        {
            InitializeComponent();
        }

        public SensorValueSettingsView(SensorValueSettingsViewModel viewModel) : this()
        {
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        private void OnVisibleListReorderCompleted(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[SensorValueSettingsView] Reorder completed");
        }

        private void OnAddButtonClicked(object? sender, EventArgs e)
        {
            if (AvailableSensorsList.SelectedItem is SensorItem item)
            {
                ViewModel.MoveToVisible(item);
                AvailableSensorsList.SelectedItem = null;
            }
        }

        private void OnRemoveButtonClicked(object? sender, EventArgs e)
        {
            if (VisibleSensorsList.SelectedItem is SensorItem item)
            {
                ViewModel.MoveToAvailable(item);
                VisibleSensorsList.SelectedItem = null;
            }
        }

        private async void OnCloseClicked(object? sender, EventArgs e)
        {
            await ViewModel.SaveSettingsAsync();

            // Notify parent page to hide settings via the ViewModel
            if (Application.Current?.Windows[0].Page is AppShell shell &&
                shell.CurrentPage is MainPage mainPage)
            {
                await mainPage.HideSettingsAsync();
            }
        }
    }
}
