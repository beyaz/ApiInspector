using System.IO;

namespace ApiInspector;

static class FileHelper
{
    static string WorkingDirectory
    {
        get
        {
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var directoryPath = Path.Combine(myDocuments, "ApiInspector") + Path.DirectorySeparatorChar;
            if (Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }

    public static void WriteFail(Exception exception)
    {
        File.WriteAllText(WorkingDirectory + @"ApiInspector.Response.Error.json", exception.ToString());
    }

    public static void WriteSuccess(string responseAsJson)
    {
        File.WriteAllText(WorkingDirectory + @"ApiInspector.Response.json", responseAsJson);
    }
}