using System.IO;

namespace ApiInspector.WebUI;

static class CacheDirectory
{
    public static readonly string CacheDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(ApiInspector)) +
                                                       Path.DirectorySeparatorChar +
                                                       "Cache" +
                                                       Path.DirectorySeparatorChar;

    public static void WriteAllText(string path, string contents)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        File.WriteAllText(path, contents);
    }
}