using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TravelCamApp.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ImageSource? _lastCaptureImage = ImageSource.FromFile("dotnet_bot.png");
        private bool _isRecording;
        private string _recordingTimeText = "00:00";
        private CaptureMode _selectedMode = CaptureMode.Photo;

        #endregion

        #region Properties

        public ImageSource? LastCaptureImage
        {
            get => _lastCaptureImage;
            set
            {
                if (Equals(_lastCaptureImage, value))
                {
                    return;
                }

                _lastCaptureImage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (_isRecording == value)
                {
                    return;
                }

                _isRecording = value;
                OnPropertyChanged();
            }
        }

        public CaptureMode SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (_selectedMode == value)
                {
                    return;
                }

                _selectedMode = value;
                OnPropertyChanged();
            }
        }

        public ICommand SetPhotoModeCommand { get; }

        public ICommand SetVideoModeCommand { get; }

        public string RecordingTimeText
        {
            get => _recordingTimeText;
            set
            {
                if (_recordingTimeText == value)
                {
                    return;
                }

                _recordingTimeText = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public MainPageViewModel()
        {
            SetPhotoModeCommand = new Command(() => SelectedMode = CaptureMode.Photo);
            SetVideoModeCommand = new Command(() => SelectedMode = CaptureMode.Video);
        }

        #endregion

        #region Methods

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public enum CaptureMode
    {
        Photo,
        Video
    }
}
