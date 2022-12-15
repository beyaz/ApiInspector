using System.IO;
using System.Text.Json;

namespace ApiInspector.WebUI;

class StateCache
{
    static readonly object fileLock = new();

    static string StateFilePath => Path.Combine(CacheDirectory.CacheDirectoryPath, @"LastState.json");

    public static MainWindowModel ReadState()
    {
        if (File.Exists(StateFilePath))
        {
            var json = File.ReadAllText(StateFilePath);

            try
            {
                var state = JsonSerializer.Deserialize<MainWindowModel>(json);
                if (state is not null)
                {
                    if (state.SelectedMethod is not null)
                    {
                        return TryRead(state.SelectedMethod) ?? state;
                    }
                }

                return state;
            }
            catch (Exception)
            {
                return new MainWindowModel();
            }
        }

        return null;
    }

    public static void Save(MainWindowModel state)
    {
        lock (fileLock)
        {
            var jsonContent = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented    = true,
                IgnoreNullValues = true
            });

            CacheDirectory.WriteAllText(StateFilePath, jsonContent);
        }
    }

    public static void Save(MethodReference methodReference, MainWindowModel state)
    {
        var jsonContent = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented    = true,
            IgnoreNullValues = true
        });

        SaveToFile(GetFileName(methodReference), jsonContent);
    }

    public static void SaveToFile(string fileNameWithoutExtension, string jsonContent)
    {
        lock (fileLock)
        {
            CacheDirectory.WriteAllText(GetCacheFilePath(fileNameWithoutExtension), jsonContent);
        }
    }

    public static MainWindowModel TryRead(MethodReference methodReference)
    {
        var filePath = GetCacheFilePath(GetFileName(methodReference));
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<MainWindowModel>(File.ReadAllText(filePath));
        }
        catch (Exception)
        {
            return null;
        }
    }

    static string GetCacheFilePath(string type) => $@"{CacheDirectory.CacheDirectoryPath}{type}.json";

    static string GetFileName(MethodReference methodReference) => methodReference.ToString().GetHashCode().ToString();
}