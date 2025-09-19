// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.StableDiffusion
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Lightweight diagnostics / health-check helper for Stable Diffusion Web UI server.
    /// </summary>
    public sealed class StableDiffusionDiagnostics
    {
        private readonly HttpClient _httpClient;

        public StableDiffusionDiagnostics(string baseUrl, HttpClient? httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("Base url cannot be empty", nameof(baseUrl));
            BaseUrl = baseUrl.TrimEnd('/');
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public string BaseUrl { get; }

        /// <summary>
        /// Perform a server connectivity test by requesting /sdapi/v1/samplers (cheap list endpoint).
        /// </summary>
        public async Task<StableDiffusionDiagnosticsResult> TestConnectionAsync(CancellationToken ct = default)
        {
            var endpoint = "/sdapi/v1/samplers";
            var start = DateTime.UtcNow;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
                var latency = DateTime.UtcNow - start;
                if (!response.IsSuccessStatusCode)
                {
                    return new StableDiffusionDiagnosticsResult(false, latency, (int)response.StatusCode, $"HTTP {(int)response.StatusCode}");
                }

                return new StableDiffusionDiagnosticsResult(true, latency, (int)response.StatusCode, "OK");
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                return new StableDiffusionDiagnosticsResult(false, TimeSpan.Zero, null, "Cancelled");
            }
            catch (TaskCanceledException)
            {
                return new StableDiffusionDiagnosticsResult(false, TimeSpan.Zero, null, "Timeout");
            }
            catch (Exception ex)
            {
                return new StableDiffusionDiagnosticsResult(false, TimeSpan.Zero, null, ex.Message);
            }
        }
    }

    public sealed record StableDiffusionDiagnosticsResult(bool Success, TimeSpan Latency, int? StatusCode, string Message);
}
