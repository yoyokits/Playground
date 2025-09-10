// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Services
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using LLM.Models.Enums;

    public static class ModelService
    {
        #region Fields

        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        #endregion Fields

        #region Methods

        public static async Task<string[]> GetAvailableModelsAsync(LLMProvider provider = LLMProvider.GPT4All)
        {
            var (baseUrl, needsAuth) = GetProviderInfo(provider);

            try
            {
                // Use different endpoints for different providers
                string endpoint = provider switch
                {
                    LLMProvider.Ollama => $"{baseUrl}/api/tags",    // Ollama uses /api/tags
                    _ => $"{baseUrl}/v1/models"                     // GPT4All uses OpenAI-style endpoint
                };

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                if (needsAuth)
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "Bearer no_key_needed");
                }

                using var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                // Debug output to see what we're getting
                System.Diagnostics.Debug.WriteLine($"[ModelService] {provider} response: {json}");

                if (provider == LLMProvider.Ollama)
                {
                    // Ollama /api/tags endpoint returns: {"models": [{"name": "model:tag", ...}, ...]}
                    var ollamaResponse = JsonSerializer.Deserialize<OllamaModelsResponse>(json);
                    var models = ollamaResponse?.Models?
                                   .Select(m => m.Name)
                                   .Where(name => !string.IsNullOrWhiteSpace(name))
                                   .ToArray()
                               ?? Array.Empty<string>();

                    System.Diagnostics.Debug.WriteLine($"[ModelService] Ollama models found: {string.Join(", ", models)}");
                    return models;
                }
                else
                {
                    // GPT4All uses OpenAI-compatible structure
                    var modelsResponse = JsonSerializer.Deserialize<ModelsResponse>(json);
                    var models = modelsResponse?.Data?
                                   .Select(m => m.Id)
                                   .Where(id => !string.IsNullOrWhiteSpace(id))
                                   .ToArray()
                               ?? Array.Empty<string>();

                    System.Diagnostics.Debug.WriteLine($"[ModelService] GPT4All models found: {string.Join(", ", models)}");
                    return models;
                }
            }
            catch (Exception ex)
            {
                // Log the specific error for debugging
                System.Diagnostics.Debug.WriteLine($"[ModelService] Error for {provider}: {ex}");
                return Array.Empty<string>();
            }
        }

        private static (string baseUrl, bool needsAuth) GetProviderInfo(LLMProvider provider)
        {
            return provider switch
            {
                LLMProvider.GPT4All => ("http://localhost:4891", true),
                LLMProvider.Ollama => ("http://localhost:11434", false),
                _ => ("http://localhost:4891", true)
            };
        }

        #endregion Methods

        #region DTOs

        private sealed class ModelInfo
        {
            #region Properties

            [JsonPropertyName("id")] public string? Id { get; set; }

            #endregion Properties
        }

        private sealed class ModelsResponse
        {
            #region Properties

            [JsonPropertyName("data")] public ModelInfo[]? Data { get; set; }

            #endregion Properties
        }

        private sealed class OllamaModelInfo
        {
            #region Properties

            [JsonPropertyName("model")] public string? Model { get; set; }
            [JsonPropertyName("modified_at")] public string? ModifiedAt { get; set; }
            [JsonPropertyName("name")] public string? Name { get; set; }
            [JsonPropertyName("size")] public long? Size { get; set; }

            #endregion Properties
        }

        // Ollama API structures
        private sealed class OllamaModelsResponse
        {
            #region Properties

            [JsonPropertyName("models")] public OllamaModelInfo[]? Models { get; set; }

            #endregion Properties
        }

        #endregion DTOs
    }
}