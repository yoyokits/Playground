// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.Core.ChatGPT
{
    using DerDieDasAICore.ChatGPT;
    using DerDieDasAICore.Properties;
    using FluentAssertions;
    using System.Diagnostics;

    [TestClass]
    public class ChatGPTClientTest
    {
        #region Methods

        [TestMethod]
        public async Task Request()
        {
            var key = Settings.Default.ChatGPTKey;
            var chatGPT = ChatGPTClient.CreateInstance(key);
            var response = string.Empty;
            try
            {
                response = await chatGPT.Ask("Was bedeutet das Haus auf English?");
                response.Should().NotBeEmpty();
            }
            catch (Exception e)
            {
                Console.WriteLine(response);
                Console.WriteLine(e);
            }

            Settings.Default.Save();
        }

        #endregion Methods
    }
}