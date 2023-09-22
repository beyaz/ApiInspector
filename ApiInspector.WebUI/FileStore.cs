using System.IO;

namespace ApiInspector.WebUI;

static class FileStore
{
    public static string ReadFromStorage(string path)
    {
        return File.ReadAllText(path);
    }

    public static void SaveToStorage(string path, string content)
    {
        FileHelper.WriteAllText(path, content);
    }
    
    public static bool ExistInStorage(string path)
    {
        return File.Exists(path);
    }
}