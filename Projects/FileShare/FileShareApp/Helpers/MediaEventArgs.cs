namespace FileShareApp.Helpers
{
    public class MediaEventArgs
    {
        public MediaAssest Media { get; }

        public MediaEventArgs(MediaAssest media)
        {
            Media = media;
        }
    }
}