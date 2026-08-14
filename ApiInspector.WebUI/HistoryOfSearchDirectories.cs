using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class HistoryOfSearchDirectories
{
    const string storageKey = $"{nameof(HistoryOfSearchDirectories)}.json";

    static readonly List<string> value;

    static HistoryOfSearchDirectories()
    {
        value = [];

        if (FileStorage.ExistInStorage(storageKey))
        {
            foreach (var path in JsonConvert.DeserializeObject<List<string>>(FileStorage.ReadFromStorage(storageKey)) ?? [])
            {
                Add(path);
            }
        }
    }

    public static IReadOnlyList<string> Value => value;

    static bool Add(string path)
    {
        if (path.HasNoValue())
        {
            return false;
        }

        path = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

        if (value.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        value.Add(path);

        return true;
    }

    public static void AddIfNotExists(string path)
    {
        if (!Add(path))
        {
            return;
        }

        FileStorage.SaveToStorage(storageKey, JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        }));
    }
}