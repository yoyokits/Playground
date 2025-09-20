namespace PostAuraCore.Services;

public interface ILlmService
{
    Task InitializeAsync(string? keyOrConfig = null, CancellationToken cancellationToken = default);
    Task GenerateAsync(string prompt, Func<string, Task> onToken, CancellationToken cancellationToken = default);
}