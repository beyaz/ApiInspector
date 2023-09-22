namespace ApiInspector.WebUI;

static class Storage
{
    public static void DeleteFromStorage(string storageKey)
    {
        FileStorage.DeleteFromStorage(storageKey);
    }

    public static bool ExistInStorage(string storageKey)
    {
        return FileStorage.ExistInStorage(storageKey);
    }

    public static string ReadFromStorage(string storageKey)
    {
        return FileStorage.ReadFromStorage(storageKey);
    }

    public static void SaveToStorage(string storageKey, string storageValue)
    {
        FileStorage.SaveToStorage(storageKey, storageValue);
    }

    public static IReadOnlyList<(string StorageKey, string StorageValue)> SearchInStoreage(string filter, int topN)
    {
        return FileStorage.SearchInStoreage(filter, topN);
    }
}