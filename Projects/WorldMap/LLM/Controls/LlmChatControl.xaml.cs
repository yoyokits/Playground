// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel; // Added for INotifyPropertyChanged
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Diagnostics; // for debug logging
    using LLM.Models;
    using LLM.Models.Enums;
    using LLM.Services;
    using LLM.Settings;

    public partial class LlmChatControl : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<string>? OutputSelected; // fired when a previous assistant/system output is clicked

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

        public ObservableCollection<Conversation> Conversations => ConversationStore.Conversations;

        private Conversation? _activeConversation;
        private bool _updatingActiveConversation;

        private readonly ObservableCollection<ChatMessage> _conversation = new();
        private LLMAccessor? _accessor;
        private int _activeAssistantIndex = -1;
        private CancellationTokenSource? _cts;
        private bool _updatingFromSettings;
        private bool _loadingConversation;

        #endregion Fields

        #region Properties

        public Conversation? ActiveConversation
        {
            get => _activeConversation;
            set
            {
                if (ReferenceEquals(_activeConversation, value)) return;
                if (_activeConversation != null)
                {
                    _ = SaveActiveConversationAsync(generateTitle: true);
                }
                _activeConversation = value;
                OnPropertyChanged(nameof(ActiveConversation));
                if (!_updatingActiveConversation && value != null && !ReferenceEquals(ConversationStore.ActiveConversation, value))
                {
                    ConversationStore.Activate(value);
                }
                LoadActiveConversationMessages();
            }
        }

        #endregion Properties

        #region Constructors

        public LlmChatControl()
        {
            InitializeComponent();
            ConversationItems.ItemsSource = _conversation;
            DataContext = this;
            ConversationStore.ActiveConversationChanged += ConversationStore_ActiveConversationChanged;
            Loaded += async (_, _) => await ConversationStore.LoadAsync();
            Unloaded += async (_, _) => await ConversationStore.SaveAsync();
            _activeConversation = ConversationStore.ActiveConversation;
            OnPropertyChanged(nameof(ActiveConversation));
        }

        #endregion Constructors

        #region Lifecycle

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsService.SettingsChanged += SettingsService_SettingsChanged;
            if (!SettingsService.IsLoaded)
                await Task.Delay(100);
            UpdateUIFromSettings();
            _updatingActiveConversation = true;
            try { ActiveConversation = ConversationStore.ActiveConversation; }
            finally { _updatingActiveConversation = false; }
            LoadActiveConversationMessages();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) =>
            SettingsService.SettingsChanged -= SettingsService_SettingsChanged;

        #endregion Lifecycle

        #region Settings

        private void SettingsService_SettingsChanged(object? sender, EventArgs e) =>
            Dispatcher.Invoke(UpdateUIFromSettings);

        private void UpdateUIFromSettings()
        {
            _updatingFromSettings = true;
            try
            {
                EnterSendsCheckBox.IsChecked = SettingsService.Current.EnterSends;
            }
            finally { _updatingFromSettings = false; }
        }

        #endregion Settings

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
                }; timer.Start();
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
            {
                _accessor = null; // force rebuild with new settings
            }
        }

        private void NewConversation_Click(object sender, RoutedEventArgs e) => _ = CreateNewConversationAsync();

        private async Task CreateNewConversationAsync()
        {
            await SaveActiveConversationAsync(generateTitle: true);
            ConversationStore.AddNew();
        }

        private async void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingConversation) return;
            await Task.CompletedTask; // logic handled by binding
        }

        private void ConversationStore_ActiveConversationChanged(object? sender, EventArgs e)
        {
            _updatingActiveConversation = true;
            try
            {
                _activeConversation = ConversationStore.ActiveConversation;
                OnPropertyChanged(nameof(ActiveConversation));
                LoadActiveConversationMessages();
            }
            finally { _updatingActiveConversation = false; }
        }

        private void MessageBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.DataContext is ChatMessage msg)
            {
                if (msg.Role == ChatRole.Assistant || msg.Role == ChatRole.System)
                {
                    if (!string.IsNullOrWhiteSpace(msg.Content))
                    {
                        Debug.WriteLine($"[LlmChatControl] OutputSelected click FULL TEXT:\n{msg.Content}");
                        OutputSelected?.Invoke(this, msg.Content);
                        LastOutput = msg.Content; // reflect selection
                    }
                }
            }
        }

        #endregion UI Event Handlers

        #region Conversation Logic

        private void LoadActiveConversationMessages()
        {
            _loadingConversation = true;
            try
            {
                _conversation.Clear();
                var conv = ConversationStore.ActiveConversation;
                if (conv != null)
                {
                    foreach (var m in conv.Messages)
                        _conversation.Add(m);
                }
                StatusBlock.Text = conv == null ? "No conversation" : conv.Title;
            }
            finally { _loadingConversation = false; }
        }

        private async Task SaveActiveConversationAsync(bool generateTitle = false)
        {
            if (ConversationStore.ActiveConversation == null) return;
            ConversationStore.UpdateFromMessages(_conversation);
            if (generateTitle)
                await TryGenerateTitleAsync(ConversationStore.ActiveConversation);
            await ConversationStore.SaveAsync();
        }

        private async Task TryGenerateTitleAsync(Conversation conv)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(conv.Title) && conv.Title != "New Conversation" && conv.Title.Length < 60) return;
                var userTexts = conv.Messages.Where(m => m.Role == ChatRole.User).Select(m => m.Content).Where(c => !string.IsNullOrWhiteSpace(c)).Take(3).ToArray();
                if (userTexts.Length == 0) return;
                var prompt = "Create a very short (max 6 words) title summarizing this chat: \n" + string.Join("\n", userTexts) + "\nTitle:";
                _accessor ??= await LLMAccessor.CreateAsync(BuildOptions());
                var titleRaw = await _accessor.GetAnswerAsync(prompt, null);
                var title = new string(titleRaw.Trim().Trim('"').Take(60).ToArray());
                if (!string.IsNullOrWhiteSpace(title)) conv.Title = title;
            }
            catch { }
        }

        private void AppendSystem(string text)
        {
            if (_loadingConversation) return;
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
            LastOutput = content;
            ScrollToEnd();
            _activeAssistantIndex = -1;
            _ = SaveActiveConversationAsync();
        }

        private void ScrollToEnd() => Dispatcher.InvokeAsync(() => ConversationScroll?.ScrollToEnd());

        private async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(SettingsService.Current.SelectedModel))
            {
                AppendSystem($"No model selected. Open Settings and pick a model for {SettingsService.Current.SelectedProvider}.");
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
                    }, ct);
                FinalizeAssistant(final);
                StatusBlock.Text = $"Done ({final.Length} chars)";
            }
            catch (OperationCanceledException) { StatusBlock.Text = "Canceled"; }
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

        #endregion Conversation Logic

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion INotifyPropertyChanged
    }
}