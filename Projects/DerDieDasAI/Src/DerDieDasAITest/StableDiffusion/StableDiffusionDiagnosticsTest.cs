// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.StableDiffusion
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using DerDieDasAICore.StableDiffusion;
    using FluentAssertions;

    [TestClass]
    public class StableDiffusionDiagnosticsTest
    {
        [TestMethod]
        public async Task TestConnectionAsync_ReturnsSuccess_On200()
        {
            var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:7860") };
            var diag = new StableDiffusionDiagnostics("http://localhost:7860", httpClient);

            var result = await diag.TestConnectionAsync();

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        [TestMethod]
        public async Task TestConnectionAsync_ReturnsFailure_On500()
        {
            var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:7860") };
            var diag = new StableDiffusionDiagnostics("http://localhost:7860", httpClient);

            var result = await diag.TestConnectionAsync();

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task TestConnectionAsync_ReturnsFailure_OnException()
        {
            var handler = new FakeHandler(_ => throw new HttpRequestException("Network down"));
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:7860") };
            var diag = new StableDiffusionDiagnostics("http://localhost:7860", httpClient);

            var result = await diag.TestConnectionAsync();

            result.Success.Should().BeFalse();
            result.StatusCode.Should().BeNull();
            result.Message.Should().Contain("Network down");
        }

        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _func;
            public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> func) => _func = func;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_func(request));
        }
    }
}
