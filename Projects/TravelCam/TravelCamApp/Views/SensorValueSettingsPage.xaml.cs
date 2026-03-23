using TravelCamApp.ViewModels;
using TravelCamApp.Models;

namespace TravelCamApp.Views
{
    public partial class SensorValueSettingsPage : ContentPage
    {
        private SensorValueSettingsViewModel? _viewModel;

        public SensorValueSettingsPage()
        {
            InitializeComponent();
            _viewModel = new SensorValueSettingsViewModel();
            BindingContext = _viewModel;
        }

        private SensorValueSettingsViewModel ViewModel
        {
            get
            {
                if (_viewModel == null && BindingContext is SensorValueSettingsViewModel vm)
                {
                    _viewModel = vm;
                }

                return _viewModel ?? throw new InvalidOperationException("ViewModel is not set");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadSettingsAsync();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await ViewModel.SaveSettingsAsync();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private void OnVisibilityTabClicked(object sender, EventArgs e)
        {
            VisibilityTab.BackgroundColor = Color.FromArgb("#6750A4");
            VisibilityTab.TextColor = Colors.White;
            AppearanceTab.BackgroundColor = Color.FromArgb("#E7E0EC");
            AppearanceTab.TextColor = Color.FromArgb("#1C1B1F");
            VisibilityContent.IsVisible = true;
            AppearanceContent.IsVisible = false;
        }

        private void OnAppearanceTabClicked(object sender, EventArgs e)
        {
            AppearanceTab.BackgroundColor = Color.FromArgb("#6750A4");
            AppearanceTab.TextColor = Colors.White;
            VisibilityTab.BackgroundColor = Color.FromArgb("#E7E0EC");
            VisibilityTab.TextColor = Color.FromArgb("#1C1B1F");
            VisibilityContent.IsVisible = false;
            AppearanceContent.IsVisible = true;
        }

        private void OnVisibleListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as SensorItem;
            if (selectedItem != null)
            {
                AvailableSensorsList.SelectedItem = null;
            }
        }

        private void OnAvailableListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as SensorItem;
            if (selectedItem != null)
            {
                VisibleSensorsList.SelectedItem = null;
            }
        }

        private void OnVisibleListReorderCompleted(object sender, EventArgs e)
        {
            _ = ViewModel.SaveSettingsAsync();
        }

        private async void OnFontSizeChanged(object sender, EventArgs e)
        {
            await ViewModel.SaveSettingsAsync();
        }

        private async void OnMapOverlayToggled(object sender, ToggledEventArgs e)
        {
            await ViewModel.SaveSettingsAsync();
        }
    }
}
