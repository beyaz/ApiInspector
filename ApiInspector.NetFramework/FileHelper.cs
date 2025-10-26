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
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }

    public static void ClearLog()
    {
        if (File.Exists(FilePath.Log))
        {
            File.Delete(FilePath.Log);
        }
    }

    public static void WriteLog(string message)
    {
        File.AppendAllText(FilePath.Log, $"{Environment.NewLine}[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}{message}{Environment.NewLine}");
    }

    static class FilePath
    {
        public static string Log => WorkingDirectory + @"ApiInspector.log.txt";
    }
}