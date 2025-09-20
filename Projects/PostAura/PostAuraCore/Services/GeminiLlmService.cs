// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using System.Text;
using System.Text.Json;

namespace PostAuraCore.Services;

public class GeminiLlmService : ILlmService
{
    #region Fields

    // Use latest model naming to reduce 400 issues if old alias deprecated.
    private const string Model = "gemini-2.0-flash"; // Adjust as needed.

    private static readonly HttpClient _http = new();
    private string _apiKey = string.Empty; // Set during InitializeAsync.
    private bool _initialized;

    #endregion Fields

    #region Methods

    public async Task GenerateAsync(string prompt, Func<string, Task> onToken, CancellationToken cancellationToken = default)
    {
        if (!_initialized) throw new InvalidOperationException("Service not initialized");
        if (string.IsNullOrWhiteSpace(prompt)) return;
        var uri = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        req.Headers.Accept.ParseAdd("application/json");

        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var raw = await resp.Content.ReadAsStringAsync(cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            string message = TryExtractError(raw) ?? $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}";
            throw new InvalidOperationException($"Gemini request failed: {message}");
        }

        using var doc = JsonDocument.Parse(raw);
        if (!doc.RootElement.TryGetProperty("candidates", out var candidates)) return;
        foreach (var cand in candidates.EnumerateArray())
        {
            if (!cand.TryGetProperty("content", out var content)) continue;
            if (!content.TryGetProperty("parts", out var parts)) continue;
            foreach (var part in parts.EnumerateArray())
            {
                if (!part.TryGetProperty("text", out var textEl)) continue;
                var txt = textEl.GetString();
                if (string.IsNullOrEmpty(txt)) continue;
                foreach (var token in Tokenize(txt))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await onToken(token);
                }
            }
        }
    }

    public Task InitializeAsync(string? keyOrConfig = null, CancellationToken cancellationToken = default)
    {
        _apiKey = keyOrConfig ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("Gemini API key not provided");
        _initialized = true;
        return Task.CompletedTask;
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
            yield return words[i] + (i < words.Length - 1 ? " " : string.Empty);
    }

    private static string? TryExtractError(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("error", out var err))
            {
                var code = err.TryGetProperty("code", out var c) ? c.GetInt32().ToString() : null;
                var msg = err.TryGetProperty("message", out var m) ? m.GetString() : null;
                return code != null ? $"{code}: {msg}" : msg;
            }
        }
        catch { }
        return null;
    }

    #endregion Methods
}