using System.IO;

namespace ApiInspector.WebUI;

static class FileStore
{
    static readonly string CacheDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(ApiInspector)) +
                                                Path.DirectorySeparatorChar +
                                                "Cache" +
                                                Path.DirectorySeparatorChar;

    public static IEnumerable<(string filePath, string fileContent)> EnumerateAllInStoreage()
    {
        var cacheDirectoryPath = CacheDirectoryPath;

        if (!Directory.Exists(cacheDirectoryPath))
        {
            yield break;
        }

        foreach (var directory in Directory.GetDirectories(cacheDirectoryPath).OrderByDescending(x => new DirectoryInfo(x).LastWriteTime))
        {
            foreach (var file in Directory.GetFiles(directory).OrderByDescending(x => new FileInfo(x).LastWriteTime))
            {
                var fileContent = File.ReadAllText(file);

                yield return (file, fileContent);
            }
        }
    }

    public static bool ExistInStorage(string path)
    {
        return File.Exists(path);
    }

    public static string GetUniqueKeyForStorage(string fileName)
    {
        return Path.Combine(CacheDirectoryPath, fileName);
    }

    public static string ReadFromStorage(string path)
    {
        return File.ReadAllText(path);
    }

    public static void SaveToStorage(string path, string content)
    {
        WriteAllText(path, content);
    }

    static void WriteAllText(string path, string contents)
    {
        var directoryName = Path.GetDirectoryName(path);
        if (directoryName != null)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        File.WriteAllText(path, contents);
    }
}