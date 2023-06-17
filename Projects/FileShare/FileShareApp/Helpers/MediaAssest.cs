namespace FileShareApp.Helpers
{
    public class MediaAssest
    {
        //Image,Video
        public enum MediaAssetType
        {
            Image, Video
        }

        public Frame Frame { get; set; }

        public string Id { get; set; }

        public bool IsSelectable { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string PreviewPath { get; set; }

        public MediaAssetType Type { get; set; }
    }
}
