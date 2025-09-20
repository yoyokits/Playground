// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PostAuraCore.Services;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PostAuraTest;

[TestClass]
public class GeminiIntegrationTests
{
    #region Fields

    private static string? _apiKey;

    #endregion Fields

    #region Methods

    [AssemblyInitialize]
    public static void Init(TestContext ctx)
    {
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            Console.WriteLine("Enter Gemini API Key (will NOT be stored): ");
            _apiKey = ReadLineHidden();
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task AskWhereIsBali()
    {
        if (string.IsNullOrWhiteSpace(_apiKey)) Assert.Inconclusive("API key not provided");
        var svc = new GeminiLlmService();
        await svc.InitializeAsync(_apiKey);
        var sb = new StringBuilder();
        await svc.GenerateAsync("Where is Bali?", async t => { sb.Append(t); await Task.CompletedTask; });
        var result = sb.ToString();
        Assert.IsTrue(result.Contains("Indonesia", StringComparison.OrdinalIgnoreCase), "Response should mention Indonesia. Actual: " + result);
    }

    private static string? ReadLineHidden()
    {
        var sb = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
            { sb.Length--; continue; }
            if (!char.IsControl(key.KeyChar)) sb.Append(key.KeyChar);
        }
        return sb.ToString();
    }

    #endregion Methods
}