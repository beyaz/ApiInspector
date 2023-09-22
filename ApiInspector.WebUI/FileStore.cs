using System.IO;

namespace ApiInspector.WebUI;

static class FileStore
{
    static readonly string CacheDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(ApiInspector)) +
                                                Path.DirectorySeparatorChar +
                                                "Cache" +
                                                Path.DirectorySeparatorChar;

    public static bool ExistInStorage(string storageKey)
    {
        return File.Exists(ToFilePath(storageKey));
    }

    public static string ReadFromStorage(string storageKey)
    {
        return File.ReadAllText(ToFilePath(storageKey));
    }
    
    public static void DeleteFromStorage(string storageKey)
    {
        var filePath = ToFilePath(storageKey);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void SaveToStorage(string storageKey, string content)
    {
        WriteAllText(ToFilePath(storageKey), content);
    }

    public static IReadOnlyList<(string storageKey, string fileContent)> SearchInStoreage(string filter, int topN)
    {
        return SearchInStoreage(filter).Take(topN).ToList();
    }

    static IEnumerable<(string storageKey, string fileContent)> SearchInStoreage(string filter)
    {
        var cacheDirectoryPath = CacheDirectoryPath;

        if (!Directory.Exists(cacheDirectoryPath))
        {
            yield break;
        }

        foreach (var directory in Directory.GetDirectories(cacheDirectoryPath).OrderByDescending(x => new DirectoryInfo(x).LastWriteTime))
        {
            foreach (var filePath in Directory.GetFiles(directory).OrderByDescending(x => new FileInfo(x).LastWriteTime))
            {
                var fileContent = File.ReadAllText(filePath);

                if (fileContent.IndexOf(filter + string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    yield return (ToStorageKey(filePath), fileContent);
                }
            }
        }
    }

    static string ToFilePath(string storageKey)
    {
        return Path.Combine(CacheDirectoryPath, storageKey);
    }
    static string ToStorageKey(string filePath)
    {
        return filePath.RemoveFromStart(CacheDirectoryPath);
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