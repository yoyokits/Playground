﻿// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace AdventureCamApp.Views
{
    using Camera.MAUI;

    public partial class MainPage : ContentPage
    {
        #region Constructors

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            cameraView.CamerasLoaded += OnCameraView_CamerasLoaded;
        }

        #endregion Constructors

        #region Methods

        private void OnCameraView_CamerasLoaded(object? sender, EventArgs e)
        {
            cameraView.FlashMode = FlashMode.Auto;
            if (cameraView.NumCamerasDetected > 0)
            {
                if (cameraView.NumMicrophonesDetected > 0)
                    cameraView.Microphone = cameraView.Microphones.First();
                cameraView.Camera = cameraView.Cameras.First();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (await cameraView.StartCameraAsync() == CameraResult.Success)
                    {
                        ////controlButton.Text = "Stop";
                        ////playing = true;
                    }
                });
            }
        }

        #endregion Methods
    }
}