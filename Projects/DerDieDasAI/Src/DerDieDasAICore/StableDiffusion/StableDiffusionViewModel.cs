// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.StableDiffusion
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using DerDieDasAICore.AI;
    using DerDieDasAICore.Common;

    /// <summary>
    /// ViewModel for StableDiffusionViewer (visual layer only).
    /// </summary>
    public class StableDiffusionViewModel : NotifyPropertyChanged
    {
        private readonly StableDiffusionClient _client;
        private readonly StableDiffusionDiagnostics _diagnostics;
        private bool _initialized;
        private string _positivePrompt = string.Empty;
        private string _negativePrompt = string.Empty;
        private double _cfgScale = 7.5;
        private object _selectedSampler;
        private object _selectedScheduler;
        private int _seed = -1;
        private bool _isGenerating;
        private string _statusMessage = "Idle";

        public StableDiffusionViewModel(string baseUrl = "http://127.0.0.1:7860/")
        {
            _client = new StableDiffusionClient(baseUrl);
            _diagnostics = new StableDiffusionDiagnostics(baseUrl);

            Samplers = new ObservableCollection<object>();
            Schedulers = new ObservableCollection<object>();
            Images = new ObservableCollection<BitmapImage>();

            GenerateCommand = new RelayCommand(async _ => await GenerateAsync(), _ => !IsGenerating);
            TestConnectionCommand = new RelayCommand(async _ => await TestConnectionAsync(), _ => !IsGenerating);
            RandomizeSeedCommand = new RelayCommand(_ => Seed = new Random().Next(int.MinValue, int.MaxValue));
        }

        public ObservableCollection<object> Samplers { get; }
        public ObservableCollection<object> Schedulers { get; }
        public ObservableCollection<BitmapImage> Images { get; }

        public string PositivePrompt { get => _positivePrompt; set { if (_positivePrompt != value) { _positivePrompt = value; OnPropertyChanged(); } } }
        public string NegativePrompt { get => _negativePrompt; set { if (_negativePrompt != value) { _negativePrompt = value; OnPropertyChanged(); } } }
        public double CfgScale { get => _cfgScale; set { if (Math.Abs(_cfgScale - value) > double.Epsilon) { _cfgScale = value; OnPropertyChanged(); } } }
        public object SelectedSampler { get => _selectedSampler; set { if (_selectedSampler != value) { _selectedSampler = value; OnPropertyChanged(); } } }
        public object SelectedScheduler { get => _selectedScheduler; set { if (_selectedScheduler != value) { _selectedScheduler = value; OnPropertyChanged(); } } }
        public int Seed { get => _seed; set { if (_seed != value) { _seed = value; OnPropertyChanged(); } } }
        public bool IsGenerating { get => _isGenerating; private set { if (_isGenerating != value) { _isGenerating = value; OnPropertyChanged(); } } }
        public string StatusMessage { get => _statusMessage; private set { if (_statusMessage != value) { _statusMessage = value; OnPropertyChanged(); } } }

        public ICommand GenerateCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand RandomizeSeedCommand { get; }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;
            await LoadSamplersAsync();
        }

        private async Task LoadSamplersAsync()
        {
            try
            {
                var list = await Task.Run(() => _client.DefaultApi.GetSamplersSdapiV1SamplersGet());
                Samplers.Clear();
                foreach (var s in list)
                {
                    Samplers.Add(s);
                }
                SelectedSampler = Samplers.Count > 0 ? Samplers[0] : null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Sampler load failed: {ex.Message}";
            }
        }

        private async Task TestConnectionAsync()
        {
            StatusMessage = "Testing connection...";
            var result = await _diagnostics.TestConnectionAsync();
            StatusMessage = result.Success ? $"Connection OK ({result.Latency.TotalMilliseconds:F0} ms)" : $"Failed: {result.Message}";
        }

        private async Task GenerateAsync()
        {
            if (IsGenerating) return;
            IsGenerating = true;
            StatusMessage = "Generating...";
            try
            {
                var request = _client.StableDiffusionProcessingTxt2Img;
                request.Prompt = PositivePrompt;
                request.NegativePrompt = NegativePrompt;
                request.CfgScale = (decimal)CfgScale;
                request.Seed = Seed;

                var response = await Task.Run(() => _client.DefaultApi.Text2imgapiSdapiV1Txt2imgPost(request));
                Images.Clear();
                foreach (var img64 in response.Images)
                {
                    var bytes = Convert.FromBase64String(img64);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = new System.IO.MemoryStream(bytes);
                    bmp.EndInit();
                    bmp.Freeze();
                    Images.Add(bmp);
                }
                StatusMessage = $"Generated {Images.Count} image(s).";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Generation failed: {ex.Message}";
            }
            finally
            {
                IsGenerating = false;
            }
        }
    }
}
