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

        private async Task RefreshModelsAsync()
        {
            try
            {
                var models = await ModelService.GetAvailableModelsAsync();
                _modelNames.Clear();
                foreach (var m in models) _modelNames.Add(m);

                if (string.IsNullOrEmpty(SettingsService.Current.SelectedModel) && _modelNames.Count > 0)
                {
                    SettingsService.Current.SelectedModel = _modelNames[0];
                    SettingsService.RaiseChangedAndSave();
                }
            }
            catch (Exception ex)
            {
                AppendSystem($"Failed to load models: {ex.Message}");
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
                SystemPrompt = s.SystemPrompt
            };
        }

        private void FinalizeAssistant(string content)
        {
            if (_activeAssistantIndex < 0 || _activeAssistantIndex >= _conversation.Count) return;
            var current = _conversation[_activeAssistantIndex];
            _conversation[_activeAssistantIndex] = current with { Content = content };
            LastOutput = content; // <-- update dependency property
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
                AppendSystem("No model selected. Refresh models or load one in GPT4All.");
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