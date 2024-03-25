// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.AI
{
    using DerDieDasAICore.AI.Model;

    public class StableDiffusionClient
    {
        #region Constructors

        public StableDiffusionClient(string root = @"http://127.0.0.1:7860/")
        {
            this.DefaultApi = new Api.DefaultApi(root);
        }

        #endregion Constructors

        #region Properties

        public Api.DefaultApi DefaultApi { get; }

        public StableDiffusionProcessingTxt2Img StableDiffusionProcessingTxt2Img { get; } = new StableDiffusionProcessingTxt2Img();

        #endregion Properties
    }
}