using System.IO;

namespace ApiInspector.WebUI;

static class FileStore
{
    public static string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }

    public static void SaveFile(string path, string content)
    {
        FileHelper.WriteAllText(path, content);
    }
    
    public static bool ExistInStore(string path)
    {
        return File.Exists(path);
    }
}