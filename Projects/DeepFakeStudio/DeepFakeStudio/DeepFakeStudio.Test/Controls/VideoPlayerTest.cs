namespace DeepFakeStudio.Test.Controls
{
    using DeepFakeStudio.Controls;
    using DeepFakeStudio.Helpers;
    using DeepFakeStudio.Test.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="VideoPlayerTest" />.
    /// </summary>
    [TestClass]
    public class VideoPlayerTest
    {
        #region Methods

        /// <summary>
        /// The ShowVideoPlayer.
        /// </summary>
        [TestMethod]
        public void ShowVideoPlayer()
        {
            WpfTestRunner.Run(_ =>
            {
                var player = new VideoPlayer
                {
                    Path = FileHelper.GetFile()
                };
                player.ShowDialog(nameof(VideoPlayer));
            });
        }

        #endregion Methods
    }
}