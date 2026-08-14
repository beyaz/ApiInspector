using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class HistoryOfSearchDirectories
{
    const string storageKey = $"{nameof(HistoryOfSearchDirectories)}.json";

    static readonly List<string> value;

    static HistoryOfSearchDirectories()
    {
        if (FileStorage.ExistInStorage(storageKey))
        {
            value = [.. (JsonConvert.DeserializeObject<List<string>>(FileStorage.ReadFromStorage(storageKey)) ?? []).Select(ClearPath)];
        }
        else
        {
            value = [];
        }
    }

    public static IReadOnlyList<string> Value => value;

    static string ClearPath(string path)
    {
        return path.RemoveFromEnd(Path.DirectorySeparatorChar.ToString());
    }

    public static void AddIfNotExists(string path)
    {
        if (path.HasNoValue())
        {
            return;
        }

        path = ClearPath(path);

        if (value.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        value.Add(path);

        FileStorage.SaveToStorage(storageKey, JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        }));
    }
}