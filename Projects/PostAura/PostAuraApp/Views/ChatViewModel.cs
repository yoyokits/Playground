using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PostAuraCore.Services;

namespace PostAuraApp.Views;

public class ChatViewModel : INotifyPropertyChanged
{
    private readonly ILlmService _service;
    private CancellationTokenSource? _cts;
    private bool _initialized;

    private string _input = string.Empty;
    public string Input
    {
        get => _input;
        set { Set(ref _input, value); OnPropertyChanged(nameof(CanSend)); }
    }

    private string _output = string.Empty;
    public string Output
    {
        get => _output;
        private set => Set(ref _output, value);
    }

    private bool _isGenerating;
    public bool IsGenerating
    {
        get => _isGenerating;
        private set { if (Set(ref _isGenerating, value)) { OnPropertyChanged(nameof(CanSend)); ((Command)CancelCommand).ChangeCanExecute(); } }
    }

    public bool CanSend => !IsGenerating && !string.IsNullOrWhiteSpace(Input);

    public ICommand SendCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExitCommand { get; }

    public ChatViewModel(ILlmService service)
    {
        _service = service;
        SendCommand = new Command(async () => await SendAsync(), () => CanSend);
        CancelCommand = new Command(() => _cts?.Cancel(), () => IsGenerating);
        ExitCommand = new Command(() => Application.Current?.Quit());
        PropertyChanged += (_, __) => ((Command)SendCommand).ChangeCanExecute();
    }

    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Input)) return;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        var prompt = Input; // capture early
        Output = string.Empty;
        try
        {
            if (!_initialized)
            {
                await Task.Run(() => _service.InitializeAsync(null, token), token); // uses environment variable
                _initialized = true;
            }
            IsGenerating = true;
            await Task.Run(async () =>
            {
                await _service.GenerateAsync(prompt, async t =>
                {
                    token.ThrowIfCancellationRequested();
                    await MainThread.InvokeOnMainThreadAsync(() => Output += t);
                }, token);
            }, token);
        }
        catch (OperationCanceledException) { await MainThread.InvokeOnMainThreadAsync(() => Output += "\n[Cancelled]"); }
        catch (Exception ex) { await MainThread.InvokeOnMainThreadAsync(() => Output += $"\n[Error] {ex.Message}"); }
        finally { IsGenerating = false; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); return true;
    }
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
