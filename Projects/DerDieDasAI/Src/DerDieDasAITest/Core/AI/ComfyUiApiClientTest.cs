// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.Core.AI
{
    using DerDieDasAICore.AI;
    using FluentAssertions;

    [TestClass]
    public class ComfyUiApiClientTest
    {
        #region Methods

        [TestMethod]
        public void PostJsonFile()
        {
            // Arrange
            var comfyUiApiClient = new ComfyUiApiClient();
            var jsonFilePath = @"TestFiles\default_workflow_api.json";
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine("File not found: " + jsonFilePath);
            }

            // Act
            var result = comfyUiApiClient.ConnectWebSocketAndFetchImages(jsonFilePath).GetAwaiter().GetResult();

            // Assert
            result.Should().NotBeNull();
        }

        #endregion Methods
    }
}