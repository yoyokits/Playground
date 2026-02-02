using TravelCamApp.Helpers;
using TravelCamApp.ViewModels;

namespace TravelCamApp.Views
{
    public partial class MainPage : ContentPage
    {
        #region Fields

        private readonly IDispatcherTimer _recordingTimer;
        private DateTime _recordingStart;
        private readonly Border? _shutterFlash;

        #endregion

        #region Constructors

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel();

            _shutterFlash = FindByName("ShutterFlash") as Border;

            _recordingTimer = Dispatcher.CreateTimer();
            _recordingTimer.Interval = TimeSpan.FromSeconds(1);
            _recordingTimer.Tick += OnRecordingTimerTick;

#if DEBUG
            ExitButton.IsVisible = true;
            ExitButton.Clicked += ExitButton_Clicked;
#endif

            CameraView.CamerasLoaded += CameraView_CamerasLoaded;
        }

        #endregion

        #region Methods

        private void CameraView_CamerasLoaded(object? sender, EventArgs e)
        {
            CameraHelper.ConfigureDevices(CameraView);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await CameraHelper.StartPreviewAsync(CameraView);
            });
        }

#if DEBUG
        private void ExitButton_Clicked(object? sender, EventArgs e)
        {
            Application.Current?.Quit();
        }
#endif

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (await CameraHelper.RequestPermissionsAsync())
            {
                await CameraHelper.StopPreviewAsync(CameraView);
                CameraView_CamerasLoaded(this, EventArgs.Empty);
            }
        }

        protected override async void OnDisappearing()
        {
            await CameraHelper.StopPreviewAsync(CameraView);
            base.OnDisappearing();
        }

        private async void OnShutterTapped(object? sender, EventArgs e)
        {
            if (BindingContext is not MainPageViewModel viewModel)
            {
                return;
            }

            if (viewModel.SelectedMode == CaptureMode.Video)
            {
                await ToggleVideoRecordingAsync(viewModel);
                return;
            }

            await PlayShutterAnimationAsync();

            var stream = await CameraHelper.TakePhotoAsync(CameraView);
            if (stream is null)
            {
                return;
            }

            var filePath = await FileHelper.SavePhotoAsync(stream, "CekliCam");
            viewModel.LastCaptureImage = ImageSource.FromFile(filePath);
        }

        private async Task PlayShutterAnimationAsync()
        {
            if (_shutterFlash is null)
            {
                return;
            }

            _shutterFlash.BackgroundColor = Colors.Black;
            _shutterFlash.Opacity = 1;

            var colorAnimation = new Animation(
                v => _shutterFlash.BackgroundColor = new Color((float)v, (float)v, (float)v),
                0,
                1,
                Easing.CubicOut);

            var tcs = new TaskCompletionSource();
            colorAnimation.Commit(this, "ShutterFlashColor", length: 400, finished: (_, _) => tcs.SetResult());
            await tcs.Task;
            await _shutterFlash.FadeTo(0, 240, Easing.CubicOut);
        }

        private void OnRecordingTimerTick(object? sender, EventArgs e)
        {
            if (BindingContext is not MainPageViewModel viewModel)
            {
                return;
            }

            var elapsed = DateTime.UtcNow - _recordingStart;
            viewModel.RecordingTimeText = elapsed.ToString(@"mm\:ss");
        }

        private async Task ToggleVideoRecordingAsync(MainPageViewModel viewModel)
        {
            if (viewModel.IsRecording)
            {
                await CameraHelper.StopRecordingAsync(CameraView);
                _recordingTimer.Stop();
                viewModel.IsRecording = false;
                viewModel.RecordingTimeText = "00:00";
                return;
            }

            var filePath = FileHelper.CreateVideoPath("CekliCam");
            await CameraHelper.StartRecordingAsync(CameraView, filePath);
            _recordingStart = DateTime.UtcNow;
            viewModel.IsRecording = true;
            _recordingTimer.Start();
        }

        #endregion
    }
}
