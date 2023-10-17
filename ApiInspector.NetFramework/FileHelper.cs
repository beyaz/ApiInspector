using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

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

    public static string ReadInputAsJsonString()
    {
        var inputAsJson = File.ReadAllText(FilePath.Input);

        File.Delete(FilePath.Input);

        return inputAsJson;
    }

    public static string ReadResponse()
    {
        var filePath = FilePath.ResponseSuccess;

        while (true)
        {
            if (File.Exists(filePath))
            {
                if (!IsFileLocked(filePath))
                {
                    var response = File.ReadAllText(filePath);

                    File.Delete(filePath);

                    return response;
                }
            }

            Thread.Sleep(100);
        }
    }

    public static string TakeResponseAsFail()
    {
        var filePath = FilePath.ResponseFail;

        while (true)
        {
            if (File.Exists(filePath))
            {
                if (!IsFileLocked(filePath))
                {
                    var response = File.ReadAllText(filePath);

                    File.Delete(filePath);

                    return response;
                }
            }

            Thread.Sleep(100);
        }
    }

    public static void WriteFail(Exception exception)
    {
        File.WriteAllText(FilePath.ResponseFail, calculateFailMessage());

        string calculateFailMessage()
        {
            if ("true".Equals(exception.Data["ExportOnlyExceptionMessage"]?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return exception.Message;
            }

            var sb = new StringBuilder();

            foreach (DictionaryEntry entry in exception.Data)
            {
                sb.Append(entry.Key);
                sb.Append(": ");
                sb.Append(entry.Value);
                sb.AppendLine();
            }
            
            sb.AppendLine(exception.ToString());

            return sb.ToString();
        }
    }

    public static void WriteInput(string inputAsJson)
    {
        File.WriteAllText(FilePath.Input, inputAsJson);
    }

    public static void WriteLog(string message)
    {
        File.WriteAllText(FilePath.Log, message);
    }

    public static void WriteSuccessResponse(string responseAsJson)
    {
        File.WriteAllText(FilePath.ResponseSuccess, responseAsJson);
    }

    static bool IsFileLocked(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        FileStream stream = null;
        try
        {
            var file = new FileInfo(path);
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            return true;
        }
        finally
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        return false;
    }

    static class FilePath
    {
        public static string Input => WorkingDirectory + @"ApiInspector.Input.json";

        public static string Log => WorkingDirectory + @"ApiInspector.log.txt";

        public static string ResponseFail => WorkingDirectory + @"ApiInspector.ResponseFail.json";

        public static string ResponseSuccess => WorkingDirectory + @"ApiInspector.ResponseSuccess.json";
    }
}