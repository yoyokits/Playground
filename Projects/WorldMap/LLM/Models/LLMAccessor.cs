// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Models
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using LLM.Models.Enums;

    public sealed class LLMAccessor : IAsyncDisposable
    {
        #region Fields

        private static readonly HttpClient SharedClient = new() { Timeout = TimeSpan.FromMinutes(5) };
        private readonly string _baseUrl;
        private readonly bool _needsAuth;
        private readonly LLMOptions _options;
        private bool _disposed;

        #endregion Fields

        #region Constructors

        private LLMAccessor(LLMOptions options)
        {
            _options = options;
            (_baseUrl, _needsAuth) = GetProviderInfo(options.Provider);
        }

        #endregion Constructors

        #region Methods

        public static async Task<LLMAccessor> CreateAsync(LLMOptions options)
        {
            var (baseUrl, needsAuth) = GetProviderInfo(options.Provider);
            var testEndpoint = options.Provider == LLMProvider.Ollama
                ? $"{baseUrl}/api/tags"
                : $"{baseUrl}/v1/chat/completions";

            try
            {
                using var testRequest = new HttpRequestMessage(
                    options.Provider == LLMProvider.Ollama ? HttpMethod.Get : HttpMethod.Post,
                    testEndpoint);

                if (needsAuth)
                {
                    testRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer no_key_needed");
                }

                if (options.Provider == LLMProvider.GPT4All)
                {
                    testRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                }

                _ = await SharedClient.SendAsync(testRequest);
            }
            catch (HttpRequestException)
            {
                var providerName = options.Provider.ToString();
                var port = options.Provider == LLMProvider.Ollama ? "11434" : "4891";
                throw new Exception($"Cannot connect to {providerName} server at {baseUrl}.\nEnsure {providerName} is running on port {port}.");
            }
            catch (TaskCanceledException)
            {
                throw new Exception($"{options.Provider} server connection timeout.");
            }
            return new LLMAccessor(options);
        }

        public ValueTask DisposeAsync()
        {
            _disposed = true;
            return ValueTask.CompletedTask;
        }

        public Task<string> GetAnswerAsync(string prompt, Action<string>? onToken, CancellationToken cancellationToken = default)
                    => CallChatAsync(new[] { new { role = "user", content = prompt } }, onToken, cancellationToken);

        public Task<string> GetChatAnswerAsync(
            IReadOnlyList<ChatMessage> messages,
            Action<string>? onToken,
            CancellationToken cancellationToken = default)
            => CallChatAsync(ConvertMessages(messages), onToken, cancellationToken);

        private static (string baseUrl, bool needsAuth) GetProviderInfo(LLMProvider provider)
        {
            return provider switch
            {
                LLMProvider.GPT4All => ("http://localhost:4891", true),
                LLMProvider.Ollama => ("http://localhost:11434", false),
                _ => ("http://localhost:4891", true)
            };
        }

        private async Task<string> CallChatAsync(
                    object[] messages,
            Action<string>? onToken,
            CancellationToken ct)
        {
            if (_options.Provider == LLMProvider.Ollama)
            {
                return await CallOllamaAsync(messages, onToken, ct);
            }
            else
            {
                return await CallGPT4AllAsync(messages, onToken, ct);
            }
        }

        private async Task<string> CallGPT4AllAsync(object[] messages, Action<string>? onToken, CancellationToken ct)
        {
            var requestBody = new Dictionary<string, object?>
            {
                ["messages"] = messages,
                ["max_tokens"] = _options.MaxTokens,
                ["temperature"] = _options.Temperature
            };

            if (!string.IsNullOrWhiteSpace(_options.SelectedModel))
                requestBody["model"] = _options.SelectedModel;

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/chat/completions")
            {
                Content = content
            };

            if (_needsAuth)
            {
                request.Headers.TryAddWithoutValidation("Authorization", "Bearer no_key_needed");
            }

            try
            {
                using var response = await SharedClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    throw new Exception($"GPT4All API error ({response.StatusCode}): {err}");
                }

                var responseJson = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson);
                string full = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "No response";

                if (onToken != null)
                    await SimulateStreaming(full, onToken, ct);

                return full;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error connecting to GPT4All: {ex.Message}");
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request to GPT4All timed out.");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Invalid JSON from GPT4All: {ex.Message}");
            }
        }

        private async Task<string> CallOllamaAsync(object[] messages, Action<string>? onToken, CancellationToken ct)
        {
            // Use Ollama's /api/chat endpoint which supports full conversation context
            var requestBody = new Dictionary<string, object>
            {
                ["model"] = _options.SelectedModel ?? "llama3.2",
                ["messages"] = messages, // Send ALL messages for context
                ["stream"] = false,
                ["options"] = new Dictionary<string, object>
                {
                    ["temperature"] = _options.Temperature,
                    ["num_predict"] = _options.MaxTokens
                }
            };

            System.Diagnostics.Debug.WriteLine($"[Ollama] Sending {messages.Length} messages for context");
            System.Diagnostics.Debug.WriteLine($"[Ollama] Messages: {string.Join(" | ", messages.Select(m => $"{((dynamic)m).role}: {((dynamic)m).content}"))}");

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/chat") // Changed to /api/chat
            {
                Content = content
            };

            try
            {
                using var response = await SharedClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    throw new Exception($"Ollama API error ({response.StatusCode}): {err}");
                }

                var responseJson = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"[Ollama] Response: {responseJson}");

                var result = JsonSerializer.Deserialize<OllamaChatResponse>(responseJson);
                string full = result?.Message?.Content?.Trim() ?? "No response";

                if (onToken != null)
                    await SimulateStreaming(full, onToken, ct);

                return full;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error connecting to Ollama: {ex.Message}");
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request to Ollama timed out.");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Invalid JSON from Ollama: {ex.Message}");
            }
        }

        private object[] ConvertMessages(IReadOnlyList<ChatMessage> messages)
        {
            var list = new List<object>();
            if (!string.IsNullOrWhiteSpace(_options.SystemPrompt))
                list.Add(new { role = "system", content = _options.SystemPrompt });

            foreach (var m in messages.TakeLast(12))
            {
                if (m.Role == ChatRole.Assistant && string.IsNullOrWhiteSpace(m.Content)) continue;
                var role = m.Role switch
                {
                    ChatRole.System => "system",
                    ChatRole.User => "user",
                    ChatRole.Assistant => "assistant",
                    _ => "user"
                };
                list.Add(new { role, content = m.Content });
            }
            return list.ToArray();
        }

        private async Task SimulateStreaming(string full, Action<string> onToken, CancellationToken ct)
        {
            const int chunkSize = 4;
            for (int i = 0; i < full.Length; i += chunkSize)
            {
                ct.ThrowIfCancellationRequested();
                onToken(full.Substring(i, Math.Min(chunkSize, full.Length - i)));
                await Task.Delay(25, ct);
            }
        }

        #endregion Methods

        #region DTOs

        private sealed class ChatCompletionResponse
        {
            #region Properties

            [JsonPropertyName("choices")] public Choice[]? Choices { get; set; }

            #endregion Properties
        }

        private sealed class Choice
        {
            #region Properties

            [JsonPropertyName("message")] public Message? Message { get; set; }

            #endregion Properties
        }

        private sealed class Message
        {
            #region Properties

            [JsonPropertyName("content")] public string? Content { get; set; }

            #endregion Properties
        }

        private sealed class OllamaChatMessage
        {
            #region Properties

            [JsonPropertyName("content")] public string? Content { get; set; }
            [JsonPropertyName("role")] public string? Role { get; set; }

            #endregion Properties
        }

        // New Ollama /api/chat response structure
        private sealed class OllamaChatResponse
        {
            #region Properties

            [JsonPropertyName("done")] public bool Done { get; set; }
            [JsonPropertyName("message")] public OllamaChatMessage? Message { get; set; }

            #endregion Properties
        }

        // Old Ollama /api/generate response structure (keeping for compatibility)
        private sealed class OllamaResponse
        {
            #region Properties

            [JsonPropertyName("response")] public string? Response { get; set; }

            #endregion Properties
        }

        #endregion DTOs
    }
}