using System.IO;

namespace ApiInspector;

static class FileHelper
{
    static string WorkingDirectory
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var directoryPath = Path.Combine(myDocuments, "ApiInspector") + Path.DirectorySeparatorChar;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return field = directoryPath;
        }
    }

    public static void ClearLog()
    {
        if (File.Exists(FilePath.Log))
        {
            File.Delete(FilePath.Log);
        }
    }

    // todo: check and remove
    public static void WriteLog_(string message)
    {
        
        try
        {
            File.AppendAllText(FilePath.Log, $"{Environment.NewLine}[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}{message}{Environment.NewLine}");
        }
        catch (Exception)
        {
            // ignored
        }
    }

    static class FilePath
    {
        public static string Log
        {
            get { return field ??= Path.Combine(WorkingDirectory, "ApiInspector.log.txt"); }
        }
    }
}