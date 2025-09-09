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

    public static class ModelService
    {
        #region Fields

        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        #endregion Fields

        #region Methods

        public static async Task<string[]> GetAvailableModelsAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:4891/v1/models");
                request.Headers.TryAddWithoutValidation("Authorization", "Bearer no_key_needed");
                using var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonSerializer.Deserialize<ModelsResponse>(json);
                return modelsResponse?.Data?
                           .Select(m => m.Id)
                           .Where(id => !string.IsNullOrWhiteSpace(id))
                           .ToArray()
                       ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        #endregion Methods

        #region Classes

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

        #endregion Classes
    }
}