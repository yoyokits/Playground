namespace FileShareApp.Helpers
{
    internal static class FileHelper
    {
        internal static string DefaultDirectory { get; set; } = string.Empty;

        internal static IList<string> GetFiles(string path)
        {
            var files = new List<string>();
            files.AddRange(Directory.GetDirectories(path));
            files.AddRange(Directory.GetFiles(path));
            return files;
        }
    }
}