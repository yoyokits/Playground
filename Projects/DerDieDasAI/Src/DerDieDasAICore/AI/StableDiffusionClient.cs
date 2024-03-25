// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.AI
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Text;
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

        #region Methods

        public void Save(string filename, TextToImageResponse txt2imgResponse)
        {
            byte[] data = Convert.FromBase64String(txt2imgResponse.Images[0]);
            var metaData = txt2imgResponse.Info;
            using (var stream = new MemoryStream(data, 0, data.Length))
            {
                var image = Image.FromStream(stream);
                this.AddMetadata(image, metaData);
                image.Save(filename, ImageFormat.Jpeg);
            }
        }

        private void AddMetadata(Image image, string metaData)
        {
            var propItem = image.PropertyItems[0];
            propItem.Id = 0x9286; // PropertyTagExifUserComment
            propItem.Type = 2; // ASCII
            propItem.Value = Encoding.UTF8.GetBytes(metaData + '\0');
            propItem.Len = propItem.Value.Length;
            image.SetPropertyItem(propItem);
        }

        #endregion Methods
    }
}