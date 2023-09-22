﻿using System.IO;

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

    public static void SaveToStorage(string storageKey, string content)
    {
        WriteAllText(ToFilePath(storageKey), content);
    }

    public static IReadOnlyList<(string filePath, string fileContent)> SearchInStoreage(string filter, int topN)
    {
        return SearchInStoreage(filter).Take(topN).ToList();
    }

    static IEnumerable<(string filePath, string fileContent)> SearchInStoreage(string filter)
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

                if (fileContent.IndexOf(filter + string.Empty, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    yield return (file, fileContent);
                }
            }
        }
    }

    static string ToFilePath(string storageKey)
    {
        return Path.Combine(CacheDirectoryPath, storageKey);
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