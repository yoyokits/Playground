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

    public sealed class LLMAccessor : IAsyncDisposable
    {
        #region Fields

        private static readonly HttpClient SharedClient = new() { Timeout = TimeSpan.FromMinutes(5) };
        private readonly LLMOptions _options;
        private bool _disposed;

        #endregion Fields

        #region Constructors

        private LLMAccessor(LLMOptions options) => _options = options;

        #endregion Constructors

        #region Methods

        public static async Task<LLMAccessor> CreateAsync(LLMOptions options)
        {
            try
            {
                using var testRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:4891/v1/chat/completions")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
                _ = await SharedClient.SendAsync(testRequest);
            }
            catch (HttpRequestException)
            {
                throw new Exception("Cannot connect to GPT4All server at http://localhost:4891.\nStart GPT4All, enable local server, and load a model.");
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Connection to GPT4All timed out.");
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

        private async Task<string> CallChatAsync(
            object[] messages,
            Action<string>? onToken,
            CancellationToken ct)
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
            using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:4891/v1/chat/completions")
            {
                Content = content
            };

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
                throw new Exception($"Network error: {ex.Message}");
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timed out.");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Invalid JSON from GPT4All: {ex.Message}");
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

        #region Classes

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

        #endregion Classes
    }
}