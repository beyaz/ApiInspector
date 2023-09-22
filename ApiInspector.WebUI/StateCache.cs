using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiInspector.WebUI;

static class StateCache
{
    static string StateFilePath => Path.Combine(CacheDirectory.CacheDirectoryPath, "LastState.json");

    public static MainWindowModel ReadState()
    {
        if (File.Exists(StateFilePath))
        {
            var json = ReadFromStorage(StateFilePath);

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
            WriteIndented          = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        SaveToStorage(StateFilePath, jsonContent);
    }

    public static void Save(MethodReference methodReference, MainWindowModel state)
    {
        var jsonContent = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented          = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        SaveToStorage(methodReference.GetCachedFullFilePath(), jsonContent);
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
            return JsonSerializer.Deserialize<MainWindowModel>(ReadFromStorage(filePath));
        }
        catch (Exception)
        {
            return null;
        }
    }

    static string GetCachedFullFilePath(this MethodReference methodReference)
    {
        var cachedAssemblyFolderPath = Path.Combine(CacheDirectory.CacheDirectoryPath, methodReference.DeclaringType.Assembly.Name);

        var fileName = $"{methodReference.ToString().GetHashString()}.json";

        return Path.Combine(cachedAssemblyFolderPath + Path.DirectorySeparatorChar + fileName);
    }

    static string GetHashString(this string text, string salt = "")
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Uses SHA256 to create the hash
        using (var sha = SHA256.Create())
        {
            // Convert the string to a byte array first, to be processed
            var textBytes = Encoding.UTF8.GetBytes(text + salt);
            var hashBytes = sha.ComputeHash(textBytes);

            // Convert back to a string, removing the '-' that BitConverter adds
            var hash = BitConverter
                .ToString(hashBytes)
                .Replace("-", string.Empty);

            return hash;
        }
    }
}