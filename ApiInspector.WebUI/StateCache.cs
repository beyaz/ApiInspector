using System.IO;
using System.Text.Json;

namespace ApiInspector.WebUI;

static class StateCache
{
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
        var jsonContent = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented    = true,
            IgnoreNullValues = true
        });

        FileHelper.WriteAllText(StateFilePath, jsonContent);
    }

    public static void Save(MethodReference methodReference, MainWindowModel state)
    {
        var jsonContent = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented    = true,
            IgnoreNullValues = true
        });

        FileHelper.WriteAllText(methodReference.GetCachedFullFilePath(), jsonContent);
    }

    public static MainWindowModel TryRead(MethodReference methodReference)
    {
        var filePath = methodReference.GetCachedFullFilePath();
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

    static string GetCachedFullFilePath(this MethodReference methodReference)
    {
        var cachedAssemblyFolderPath = Path.Combine(CacheDirectory.CacheDirectoryPath, methodReference.DeclaringType.Assembly.Name);

        var fileName = $"{methodReference.ToString().GetHashCode()}.json";

        return Path.Combine(cachedAssemblyFolderPath + Path.DirectorySeparatorChar + fileName);
    }
}