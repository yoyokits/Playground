// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAITest.Core.AI
{
    using DerDieDasAICore.AI;
    using FluentAssertions;

    [TestClass]
    public class StableDiffusionClientTest
    {
        #region Methods

        [TestMethod]
        public void Text2imgapiSdapiV1Txt2imgPost()
        {
            var root = @"http://127.0.0.1:7860/";
            var client = new StableDiffusionClient(root);
            var img = client.StableDiffusionProcessingTxt2Img;
            img.Prompt = "Cat";
            var response = client.DefaultApi.Text2imgapiSdapiV1Txt2imgPost(img);
            response.Should().NotBeNull();
            client.Save(@"C:\Temp\Test.png", response);
        }

        #endregion Methods
    }
}