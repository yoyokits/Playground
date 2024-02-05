namespace DerDieDasAICore.File
{
    internal static class FolderHelper
    {
        internal static string NameToPath(string fileName)
        {
            fileName = fileName.ToUpper();
            if (fileName.Length == 1)
            {
                return "_";
            }
            var folder0 = fileName[0];
            var folder1 = fileName.Length > 1 ? fileName[1] : '_';
            var path = Path.Combine(folder0.ToString(), folder1.ToString());
            return path;
        }
    }
}
