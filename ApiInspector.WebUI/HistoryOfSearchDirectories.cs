using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class HistoryOfSearchDirectories
{
    static readonly string storageKey = $"{nameof(HistoryOfSearchDirectories)}.json";

    static readonly List<string> value;

    static HistoryOfSearchDirectories()
    {
        if (FileStorage.ExistInStorage(storageKey))
        {
            value = JsonConvert.DeserializeObject<List<string>>(FileStorage.ReadFromStorage(storageKey));
        }
        else
        {
            value = new List<string>();
        }
    }

    public static IReadOnlyList<string> Value => value;

    public static void AddIfNotExists(string path)
    {
        if (value.Contains(path))
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