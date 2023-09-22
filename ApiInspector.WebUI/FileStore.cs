using System.IO;

namespace ApiInspector.WebUI;

static class FileStore
{
    public static IEnumerable<(string filePath, string fileContent)> EnumerateAllInStoreage()
    {
        var cacheDirectoryPath = CacheDirectory.CacheDirectoryPath;

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
        return Path.Combine(CacheDirectory.CacheDirectoryPath, fileName);
    }

    public static string ReadFromStorage(string path)
    {
        return File.ReadAllText(path);
    }

    public static void SaveToStorage(string path, string content)
    {
        FileHelper.WriteAllText(path, content);
    }
}