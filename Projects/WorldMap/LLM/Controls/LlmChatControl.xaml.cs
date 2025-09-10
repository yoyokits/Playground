// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using LLM.Models;
    using LLM.Models.Enums;
    using LLM.Services;
    using LLM.Settings;

    public partial class LlmChatControl : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty LastOutputProperty =
            DependencyProperty.Register(
                nameof(LastOutput),
                typeof(string),
                typeof(LlmChatControl),
                new PropertyMetadata(string.Empty));

        public string LastOutput
        {
            get => (string)GetValue(LastOutputProperty);
            set => SetValue(LastOutputProperty, value);
        }

        #endregion Dependency Properties

        #region Fields

        private readonly ObservableCollection<ChatMessage> _conversation = new();
        private readonly ObservableCollection<string> _modelNames = new();
        private LLMAccessor? _accessor;
        private int _activeAssistantIndex = -1;
        private CancellationTokenSource? _cts;
        private bool _updatingFromSettings;

        #endregion Fields

        #region Properties

        public ObservableCollection<string> ModelNames => _modelNames;

        #endregion Properties

        #region Constructors

        public LlmChatControl()
        {
            InitializeComponent();
            ConversationItems.ItemsSource = _conversation;
            DataContext = this;
            InitializeProviderCombo();
        }

        #endregion Constructors

        #region Lifecycle

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsService.SettingsChanged += SettingsService_SettingsChanged;

            if (!SettingsService.IsLoaded)
                await Task.Delay(100);

            await RefreshModelsAsync();
            UpdateUIFromSettings();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) =>
            SettingsService.SettingsChanged -= SettingsService_SettingsChanged;

        #endregion Lifecycle

        #region Settings / Models

        private void InitializeProviderCombo()
        {
            ProviderCombo.Items.Add("GPT4All");
            ProviderCombo.Items.Add("Ollama");
            ProviderCombo.SelectedIndex = 0; // Default to GPT4All
        }

        private async Task RefreshModelsAsync()
        {
            try
            {
                var provider = SettingsService.Current.SelectedProvider;
                AppendSystem($"Fetching models from {provider}...");

                var models = await ModelService.GetAvailableModelsAsync(provider);
                _modelNames.Clear();

                if (models.Length == 0)
                {
                    AppendSystem($"No models found for {provider}. Ensure {provider} is running and has models loaded.");
                    return;
                }

                foreach (var m in models)
                {
                    _modelNames.Add(m);
                }

                AppendSystem($"Found {models.Length} models: {string.Join(", ", models.Take(3))}{(models.Length > 3 ? "..." : "")}");

                // Auto-select first model if none selected
                if (string.IsNullOrEmpty(SettingsService.Current.SelectedModel) && _modelNames.Count > 0)
                {
                    var updatedSettings = new AppSettings
                    {
                        SelectedProvider = SettingsService.Current.SelectedProvider,
                        SelectedModel = _modelNames[0],
                        MaxTokens = SettingsService.Current.MaxTokens,
                        Temperature = SettingsService.Current.Temperature,
                        SystemPrompt = SettingsService.Current.SystemPrompt,
                        EnterSends = SettingsService.Current.EnterSends
                    };
                    SettingsService.Update(updatedSettings);
                    AppendSystem($"Auto-selected model: {_modelNames[0]}");
                }
            }
            catch (Exception ex)
            {
                AppendSystem($"Failed to load models: {ex.Message}");

                // Provide specific troubleshooting for each provider
                var provider = SettingsService.Current.SelectedProvider;
                if (provider == LLMProvider.Ollama)
                {
                    AppendSystem("Troubleshooting Ollama:\n• Ensure Ollama is running: 'ollama serve'\n• Check if models are installed: 'ollama list'\n• Try pulling a model: 'ollama pull llama3.2'");
                }
                else if (provider == LLMProvider.GPT4All)
                {
                    AppendSystem("Troubleshooting GPT4All:\n• Ensure GPT4All application is running\n• Enable 'LocalServer' in GPT4All settings\n• Load a model in GPT4All interface");
                }
            }
        }

        private void SettingsService_SettingsChanged(object? sender, EventArgs e) =>
            Dispatcher.Invoke(UpdateUIFromSettings);

        private void UpdateUIFromSettings()
        {
            _updatingFromSettings = true;
            try
            {
                EnterSendsCheckBox.IsChecked = SettingsService.Current.EnterSends;
                ProviderCombo.SelectedIndex = (int)SettingsService.Current.SelectedProvider;

                if (!string.IsNullOrEmpty(SettingsService.Current.SelectedModel) &&
                    _modelNames.Contains(SettingsService.Current.SelectedModel))
                {
                    ModelCombo.SelectedItem = SettingsService.Current.SelectedModel;
                }
            }
            finally
            {
                _updatingFromSettings = false;
            }
        }

        #endregion Settings / Models

        #region UI Event Handlers

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelButton.IsEnabled = false;
            _cts?.Cancel();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _conversation.Clear();
            StatusBlock.Text = "Cleared";
            _activeAssistantIndex = -1;
            LastOutput = string.Empty;
        }

        private void CopyMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string text || string.IsNullOrEmpty(text)) return;
            try
            {
                Clipboard.SetText(text);
                var original = btn.Content;
                btn.Content = "\u2713";
                btn.Background = Brushes.Green;
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
                timer.Tick += (_, _) =>
                {
                    btn.Content = original;
                    btn.Background = Brushes.Transparent;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                AppendSystem($"Failed to copy: {ex.Message}");
            }
        }

        private void EnterSendsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_updatingFromSettings) return;
            SettingsService.Current.EnterSends = EnterSendsCheckBox.IsChecked ?? true;
            SettingsService.RaiseChangedAndSave();
        }

        private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SettingsService.Current.EnterSends)
            {
                bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                if (!ctrl)
                {
                    e.Handled = true;
                    _ = SendAsync();
                }
                else
                {
                    int pos = InputBox.CaretIndex;
                    InputBox.Text = InputBox.Text.Insert(pos, Environment.NewLine);
                    InputBox.CaretIndex = pos + Environment.NewLine.Length;
                    e.Handled = true;
                }
            }
        }

        private void ModelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingFromSettings) return;
            if (ModelCombo.SelectedItem is string name)
            {
                SettingsService.Current.SelectedModel = name;
                SettingsService.RaiseChangedAndSave();
                ResetAccessor();
                AppendSystem($"Model selected: {name}");
            }
        }

        private async void ProviderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingFromSettings) return;

            var newProvider = (LLMProvider)ProviderCombo.SelectedIndex;

            // Update settings properly
            var currentSettings = SettingsService.Current;
            var updatedSettings = new AppSettings
            {
                SelectedProvider = newProvider,
                SelectedModel = null, // Clear model when changing provider
                MaxTokens = currentSettings.MaxTokens,
                Temperature = currentSettings.Temperature,
                SystemPrompt = currentSettings.SystemPrompt,
                EnterSends = currentSettings.EnterSends
            };

            SettingsService.Update(updatedSettings);

            ResetAccessor();
            AppendSystem($"LLM Provider changed to {newProvider}. Refreshing models...");

            // Add status feedback
            StatusBlock.Text = $"Loading {newProvider} models...";

            try
            {
                await RefreshModelsAsync();
                StatusBlock.Text = $"Loaded {_modelNames.Count} {newProvider} models";
            }
            catch (Exception ex)
            {
                StatusBlock.Text = $"Failed to load {newProvider} models";
                AppendSystem($"Error loading models: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = RefreshModelsAsync();

        private void ReuseUserMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string text && !string.IsNullOrWhiteSpace(text))
            {
                InputBox.Text = text;
                InputBox.CaretIndex = InputBox.Text.Length;
                InputBox.Focus();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => _ = SendAsync();

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true)
                ResetAccessor();
        }

        #endregion UI Event Handlers

        #region Core Chat Logic

        private void AppendSystem(string text)
        {
            _conversation.Add(new ChatMessage(ChatRole.System, text));
            ScrollToEnd();
        }

        private LLMOptions BuildOptions()
        {
            var s = SettingsService.Current;
            return new LLMOptions
            {
                SelectedModel = s.SelectedModel,
                MaxTokens = s.MaxTokens,
                Temperature = s.Temperature,
                SystemPrompt = s.SystemPrompt,
                Provider = s.SelectedProvider
            };
        }

        private void FinalizeAssistant(string content)
        {
            if (_activeAssistantIndex < 0 || _activeAssistantIndex >= _conversation.Count) return;
            var current = _conversation[_activeAssistantIndex];
            _conversation[_activeAssistantIndex] = current with { Content = content };
            LastOutput = content; // Update dependency property
            ScrollToEnd();
            _activeAssistantIndex = -1;
        }

        private void ResetAccessor()
        {
            _accessor = null;
            AppendSystem("Accessor reset. Next send will reconnect.");
        }

        private void ScrollToEnd() =>
            Dispatcher.InvokeAsync(() => ConversationScroll?.ScrollToEnd());

        private async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(SettingsService.Current.SelectedModel))
            {
                AppendSystem($"No model selected. Refresh models or ensure {SettingsService.Current.SelectedProvider} is running.");
                return;
            }

            var userText = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(userText)) return;

            StatusBlock.Text = "Sending...";
            Progress.Visibility = Visibility.Visible;
            SendButton.IsEnabled = false;
            InputBox.IsEnabled = false;
            CancelButton.IsEnabled = true;

            InputBox.Clear();
            _conversation.Add(new ChatMessage(ChatRole.User, userText));
            var assistant = new ChatMessage(ChatRole.Assistant, string.Empty);
            _conversation.Add(assistant);
            _activeAssistantIndex = _conversation.Count - 1;
            ScrollToEnd();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            try
            {
                _accessor ??= await LLMAccessor.CreateAsync(BuildOptions());
                var sb = new StringBuilder();
                string final = await _accessor.GetChatAnswerAsync(
                    _conversation,
                    token =>
                    {
                        sb.Append(token);
                        UpdateAssistantStreaming(sb.ToString());
                    },
                    ct);

                FinalizeAssistant(final);
                StatusBlock.Text = $"Done ({final.Length} chars)";
            }
            catch (OperationCanceledException)
            {
                StatusBlock.Text = "Canceled";
            }
            catch (Exception ex)
            {
                StatusBlock.Text = "Error";
                AppendSystem($"Error: {ex.Message}");
            }
            finally
            {
                Progress.Visibility = Visibility.Collapsed;
                SendButton.IsEnabled = true;
                InputBox.IsEnabled = true;
                CancelButton.IsEnabled = false;
                InputBox.Focus();
            }
        }

        private void UpdateAssistantStreaming(string content)
        {
            if (_activeAssistantIndex < 0 || _activeAssistantIndex >= _conversation.Count) return;
            Dispatcher.Invoke(() =>
            {
                var current = _conversation[_activeAssistantIndex];
                _conversation[_activeAssistantIndex] = current with { Content = content };
                ScrollToEnd();
            });
        }

        #endregion Core Chat Logic
    }
}