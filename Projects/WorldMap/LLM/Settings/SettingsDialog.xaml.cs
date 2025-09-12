// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LLM.Models.Enums;
using LLM.Services;
using LLM.Settings;

namespace LLM.Settings
{
    public partial class SettingsDialog : Window, INotifyPropertyChanged
    {
        private AppSettings _working;
        private string _modelStatus = "Idle";
        public ObservableCollection<string> AvailableModels { get; } = new();
        public LLMProvider[] ProviderValues { get; } = Enum.GetValues(typeof(LLMProvider)).Cast<LLMProvider>().ToArray();
        private bool _loading;
        private bool _forceSelectFirstModel; // ensures first model is auto-selected after provider change

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsDialog()
        {
            InitializeComponent();
            _working = SettingsService.Clone();
            DataContext = this;
        }

        public AppSettings Working
        {
            get => _working;
            private set { _working = value; OnPropertyChanged(nameof(Working)); }
        }

        public string ModelStatus
        {
            get => _modelStatus;
            private set { _modelStatus = value; OnPropertyChanged(nameof(ModelStatus)); }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        { await LoadModelsAsync(); }

        private async Task LoadModelsAsync()
        {
            if (_loading) return;
            _loading = true;
            try
            {
                ModelStatus = "Loading models...";
                AvailableModels.Clear();
                var models = await ModelService.GetAvailableModelsAsync(Working.SelectedProvider);
                foreach (var m in models)
                    AvailableModels.Add(m);

                if (AvailableModels.Count > 0)
                {
                    if (_forceSelectFirstModel || string.IsNullOrEmpty(Working.SelectedModel) || !AvailableModels.Contains(Working.SelectedModel))
                    {
                        Working.SelectedModel = AvailableModels[0];
                        SettingsService.SetModel(Working.SelectedModel); // persist
                        OnPropertyChanged(nameof(Working));
                    }
                }

                ModelStatus = AvailableModels.Count == 0 ? "No models found" : $"Loaded {AvailableModels.Count} models";
            }
            catch
            { ModelStatus = "Error loading models"; }
            finally { _loading = false; }
        }

        private async void ProviderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            SettingsService.SetProvider(Working.SelectedProvider); // persist provider change
            _forceSelectFirstModel = true; // always pick first for new provider per requirement
            Working.SelectedModel = null; // clear current selection
            OnPropertyChanged(nameof(Working));
            await LoadModelsAsync();
            _forceSelectFirstModel = false; // reset
        }

        private async void RefreshModels_Click(object sender, RoutedEventArgs e) => await LoadModelsAsync();

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Push full snapshot including any scalar edits done via bindings
            await SettingsService.UpdateAsync(Working);
            DialogResult = true;
            Close();
        }

        private void ModelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            // Ensure selected model persisted immediately
            SettingsService.SetModel(Working.SelectedModel);
        }

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}