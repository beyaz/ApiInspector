﻿namespace ApiInspector.WebUI;

static class FileStorage
{
    static string CacheDirectoryPath
    {
        get
        {
            var folderNames = Config.FileStorage.CacheDirectoryFormat.Split('>', StringSplitOptions.RemoveEmptyEntries);

            return string.Join(Path.DirectorySeparatorChar.ToString(), folderNames.Select(processFolderName)) + Path.DirectorySeparatorChar;

            static string processFolderName(string folderName)
            {
                return folderName.Replace("{MyDocuments}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)).Trim();
            }
        }
    }

    public static void DeleteFromStorage(string storageKey)
    {
        var filePath = ToFilePath(storageKey);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static bool ExistInStorage(string storageKey)
    {
        return File.Exists(ToFilePath(storageKey));
    }

    public static string ReadFromStorage(string storageKey)
    {
        return File.ReadAllText(ToFilePath(storageKey));
    }

    public static void SaveToStorage(string storageKey, string storageValue)
    {
        WriteAllText(ToFilePath(storageKey), storageValue);
    }

    public static IReadOnlyList<(string StorageKey, string StorageValue)> SearchInStoreage(string filter, int topN)
    {
        return SearchInStoreage(filter).Take(topN).ToList();
    }

    static IEnumerable<(string StorageKey, string StorageValue)> SearchInStoreage(string filter)
    {
        var cacheDirectoryPath = CacheDirectoryPath;

        if (!Directory.Exists(cacheDirectoryPath))
        {
            yield break;
        }

        var searchTexts = (filter + string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var directory in Directory.GetDirectories(cacheDirectoryPath).OrderByDescending(x => new DirectoryInfo(x).LastWriteTime))
        {
            foreach (var filePath in Directory.GetFiles(directory).OrderByDescending(x => new FileInfo(x).LastWriteTime))
            {
                var fileContent = File.ReadAllText(filePath);

                var hasAnyMatch = searchTexts.Any(x => fileContent.IndexOf(x + string.Empty, StringComparison.OrdinalIgnoreCase) >= 0);
                
                if (hasAnyMatch || string.IsNullOrWhiteSpace(filter))
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