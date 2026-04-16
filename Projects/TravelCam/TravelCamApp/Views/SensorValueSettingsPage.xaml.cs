using TravelCamApp.ViewModels;
using TravelCamApp.Models;

namespace TravelCamApp.Views
{
    public partial class OverlaySettingsPage : ContentPage
    {
        private OverlaySettingsViewModel? _viewModel;

        public OverlaySettingsPage()
        {
            InitializeComponent();
            _viewModel = new OverlaySettingsViewModel();
            BindingContext = _viewModel;
        }

        private OverlaySettingsViewModel ViewModel
        {
            get
            {
                if (_viewModel == null && BindingContext is OverlaySettingsViewModel vm)
                {
                    _viewModel = vm;
                }

                return _viewModel ?? throw new InvalidOperationException("ViewModel is not set");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // await ViewModel.LoadSettingsAsync();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            // await ViewModel.SaveSettingsAsync();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OverlaySettingsPage] OnBackClicked error: {ex.Message}");
            }
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
            var selectedItem = e.CurrentSelection.FirstOrDefault() as OverlayItem;
            if (selectedItem != null)
            {
                AvailableItemsList.SelectedItem = null;
            }
        }

        private void OnAvailableListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as OverlayItem;
            if (selectedItem != null)
            {
                VisibleItemsList.SelectedItem = null;
            }
        }

        private async void OnVisibleListReorderCompleted(object sender, EventArgs e)
        {
            try { await ViewModel.SaveSettingsAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OverlaySettingsPage] OnVisibleListReorderCompleted error: {ex.Message}");
            }
        }

        private async void OnFontSizeChanged(object sender, EventArgs e)
        {
            try { await ViewModel.SaveSettingsAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OverlaySettingsPage] OnFontSizeChanged error: {ex.Message}");
            }
        }

        private async void OnMapOverlayToggled(object sender, ToggledEventArgs e)
        {
            try { await ViewModel.SaveSettingsAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OverlaySettingsPage] OnMapOverlayToggled error: {ex.Message}");
            }
        }
    }
}
