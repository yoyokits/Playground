// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //
//
// MainPage code-behind:
// - Passes the CameraView reference to the ViewModel on OnAppearing.
// - Shows/hides the settings overlay as a modal.
// - Does NOT dispose the camera when overlays appear or
//   OnDisappearing fires. Camera lifecycle is managed by the
//   ViewModel via Window.Resumed/Stopped events.

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel ViewModel => (MainPageViewModel)BindingContext;
        private readonly SensorValueSettingsViewModel _settingsViewModel;

        public MainPage(MainPageViewModel viewModel, SensorValueSettingsViewModel settingsViewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _settingsViewModel = settingsViewModel;
            SettingsOverlay.BindingContext = _settingsViewModel;

            // When settings overlay opens, populate the lists from SensorItems
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainPageViewModel.IsSettingsVisible)
                    && viewModel.IsSettingsVisible)
                {
                    _settingsViewModel.LoadFromSensorItems(viewModel.SensorItems);
                }
            };

            // When settings overlay requests close, save and hide
            SettingsOverlay.CloseRequested += async (s, e) => await HideSettingsAsync();

            // Subscribe to media capture events from the CameraView
            CameraView.MediaCaptured += OnMediaCaptured;
            CameraView.MediaCaptureFailed += OnMediaCaptureFailed;
        }

        /// <summary>
        /// Called each time this page becomes visible.
        /// Triggers camera initialization / preview restart via the ViewModel.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("[MainPage] OnAppearing — notifying ViewModel");
            await ViewModel.OnViewReady(CameraView);
        }

        /// <summary>
        /// Routes the MediaCaptured event from CameraView to the ViewModel.
        /// </summary>
        private async void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
        {
            await ViewModel.OnMediaCaptured(e.Media);
        }

        /// <summary>
        /// Routes the MediaCaptureFailed event from CameraView to the ViewModel.
        /// </summary>
        private void OnMediaCaptureFailed(object? sender, MediaCaptureFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPage] Media capture failed: {e.FailureReason}");
            ViewModel.OnMediaCaptureFailed();
        }

        /// <summary>
        /// Saves settings and hides the settings overlay.
        /// </summary>
        private async Task HideSettingsAsync()
        {
            await _settingsViewModel.SaveSettingsAsync();
            _settingsViewModel.ApplyToSensorItems(ViewModel.SensorItems);
            ViewModel.IsSettingsVisible = false;
        }
    }
}
