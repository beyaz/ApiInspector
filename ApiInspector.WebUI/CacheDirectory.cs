using System.IO;

namespace ApiInspector.WebUI;

static class CacheDirectory
{
    public static readonly string CacheDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(ApiInspector)) +
                                                       Path.DirectorySeparatorChar +
                                                       "Cache" +
                                                       Path.DirectorySeparatorChar;
}