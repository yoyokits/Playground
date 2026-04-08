// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

using Microsoft.Maui.Controls;

namespace TravelCamApp.Selectors
{
    /// <summary>
    /// Selects appropriate DataTemplate based on media file type (image vs video).
    /// </summary>
    public class MediaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ImageTemplate { get; set; }
        public DataTemplate? VideoTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is string filePath)
            {
                var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                if (extension == ".mp4")
                    return VideoTemplate;
                return ImageTemplate; // default for images
            }

            return ImageTemplate;
        }
    }
}
