using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiInspector.WebUI;

static class StateCache
{
    static string StateFilePath => "LastState.json";

    public static MainWindowModel ReadState()
    {
        if (FileStorage.ExistInStorage(StateFilePath))
        {
            var json = FileStorage.ReadFromStorage(StateFilePath);

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

        SaveToStorage(methodReference.GetStorageKey(), jsonContent);
    }

    public static MainWindowModel TryRead(MethodReference methodReference)
    {
        var storageKey = methodReference.GetStorageKey();
        if (!ExistInStorage(storageKey))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<MainWindowModel>(ReadFromStorage(storageKey));
        }
        catch (Exception)
        {
            return null;
        }
    }

    static string GetStorageKey(this MethodReference methodReference)
    {
        var fileName = $"{methodReference.ToString().GetHashString()}.json";

        return Path.Combine(methodReference.DeclaringType.Assembly.Name + Path.DirectorySeparatorChar + fileName);
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