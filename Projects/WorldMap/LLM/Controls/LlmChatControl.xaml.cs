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
            string? text = null;
            
            // Handle both button clicks and menu item clicks
            if (sender is Button btn && btn.Tag is string buttonText)
            {
                text = buttonText;
            }
            else if (sender is MenuItem menuItem && menuItem.Tag is string menuText)
            {
                text = menuText;
            }
            
            if (string.IsNullOrEmpty(text)) return;
            
            try
            {
                Clipboard.SetText(text);
                
                // Only show visual feedback for buttons, not menu items
                if (sender is Button button)
                {
                    var original = button.Content;
                    button.Content = "\u2713";
                    button.Background = Brushes.Green;
                    var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
                    timer.Tick += (_, _) =>
                    {
                        button.Content = original;
                        button.Background = Brushes.Transparent;
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                AppendSystem($"Failed to copy: {ex.Message}");
            }
        }

        private void SelectAllMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                // Find the TextBox that owns this context menu
                var contextMenu = menuItem.Parent as ContextMenu;
                if (contextMenu?.PlacementTarget is TextBox textBox)
                {
                    textBox.SelectAll();
                    textBox.Focus();
                }
            }
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Handle Ctrl+A for Select All
                if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    textBox.SelectAll();
                    e.Handled = true;
                }
                // Handle Ctrl+C for Copy (this is usually handled by default, but we can ensure it works)
                else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (!string.IsNullOrEmpty(textBox.SelectedText))
                    {
                        Clipboard.SetText(textBox.SelectedText);
                        e.Handled = true;
                    }
                    else if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        // If no text is selected, copy all text
                        Clipboard.SetText(textBox.Text);
                        e.Handled = true;
                    }
                }
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

        private void MessageTextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ChatMessage msg)
            {
                // Only trigger message selection if user didn't select any text
                // This allows for both text selection and message selection functionality
                if (string.IsNullOrEmpty(textBox.SelectedText))
                {
                    // Check if this is an assistant or system message
                    if (msg.Role == ChatRole.Assistant || msg.Role == ChatRole.System)
                    {
                        if (!string.IsNullOrWhiteSpace(msg.Content))
                        {
                            Debug.WriteLine($"[LlmChatControl] OutputSelected from TextBox click FULL TEXT:\n{msg.Content}");
                            OutputSelected?.Invoke(this, msg.Content);
                            LastOutput = msg.Content; // reflect selection
                        }
                    }
                }
                // If text was selected, don't trigger message selection to allow for copy operations
            }
        }

        private void ApplyToMap_Click(object sender, RoutedEventArgs e)
        {
            string? content = null;
            
            // Handle both button clicks and menu item clicks
            if (sender is Button btn && btn.Tag is string buttonContent)
            {
                content = buttonContent;
            }
            else if (sender is MenuItem menuItem && menuItem.Tag is string menuContent)
            {
                content = menuContent;
            }
            
            if (string.IsNullOrWhiteSpace(content)) return;
            
            Debug.WriteLine($"[LlmChatControl] ApplyToMap click FULL TEXT:\n{content}");
            OutputSelected?.Invoke(this, content);
            LastOutput = content; // reflect selection
            
            // Update status to indicate message was applied to map
            StatusBlock.Text = "Applied to map";
            
            // Provide visual feedback only for buttons, not menu items
            if (sender is Button button)
            {
                ShowApplyToMapFeedback(button);
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

        private void ShowApplyToMapFeedback(Button button)
        {
            // Change button appearance briefly to show it was clicked
            var original = button.Content;
            var originalBackground = button.Background;
            
            button.Content = "✓";
            button.Background = Brushes.Green;
            
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            timer.Tick += (_, _) =>
            {
                button.Content = original;
                button.Background = originalBackground;
                timer.Stop();
            };
            timer.Start();
        }

        private void DeleteConversation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Conversation conversation)
            {
                // Show confirmation dialog
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the conversation '{conversation.Title}'?\n\nThis action cannot be undone.",
                    "Delete Conversation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ConversationStore.RemoveConversation(conversation);
                    _ = ConversationStore.SaveAsync();
                    StatusBlock.Text = "Conversation deleted";
                }
            }
        }

        private void DeleteMessage_Click(object sender, RoutedEventArgs e)
        {
            ChatMessage? messageToDelete = null;
            
            // Handle both button clicks and menu item clicks
            if (sender is Button btn && btn.Tag is ChatMessage buttonMessage)
            {
                messageToDelete = buttonMessage;
            }
            else if (sender is MenuItem menuItem && menuItem.Tag is ChatMessage menuMessage)
            {
                messageToDelete = menuMessage;
            }

            if (messageToDelete == null) return;

            // Show confirmation dialog
            var preview = messageToDelete.Content.Length > 50 
                ? messageToDelete.Content.Substring(0, 47) + "..."
                : messageToDelete.Content;

            var result = MessageBox.Show(
                $"Are you sure you want to delete this {messageToDelete.Role.ToString().ToLower()} message?\n\n\"{preview}\"\n\nThis action cannot be undone.",
                "Delete Message",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _conversation.Remove(messageToDelete);
                StatusBlock.Text = "Message deleted";
                
                // Save the conversation after deleting the message
                _ = SaveActiveConversationAsync();
                
                // Provide visual feedback for button clicks
                if (sender is Button button)
                {
                    ShowDeleteFeedback(button);
                }
            }
        }

        private void ShowDeleteFeedback(Button button)
        {
            // Brief visual feedback that the message was deleted
            var original = button.Content;
            var originalBackground = button.Background;
            
            button.Content = "✓";
            button.Background = Brushes.Red;
            
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += (_, _) =>
            {
                button.Content = original;
                button.Background = originalBackground;
                timer.Stop();
            };
            timer.Start();
        }
    }
}