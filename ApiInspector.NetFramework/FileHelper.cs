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

    public static string ReadInputAsJsonString(string communicationId)
    {
        var inputAsJson = File.ReadAllText(FilePath.Input(communicationId));

        File.Delete(FilePath.Input(communicationId));

        return inputAsJson;
    }

    public static string ReadResponse(string communicationId)
    {
        var filePath = FilePath.ResponseSuccess(communicationId);

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

    public static string TakeResponseAsFail(string communicationId)
    {
        var filePath = FilePath.ResponseFail(communicationId);

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

    public static void WriteFail(string communicationId, Exception exception)
    {
        File.WriteAllText(FilePath.ResponseFail(communicationId), calculateFailMessage());
        return;

        string calculateFailMessage()
        {
            if ("true".Equals(exception.Data["ExportOnlyExceptionMessage"]?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return exception.Message;
            }

            var sb = new StringBuilder();

            sb.AppendLine("Fail");
            
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

    public static void WriteInput(string communicationId, string inputAsJson)
    {
        File.WriteAllText(FilePath.Input(communicationId), inputAsJson);
    }

    public static void WriteLog(string message)
    {
        File.WriteAllText(FilePath.Log, message);
    }

    public static void WriteSuccessResponse(string communicationId, string responseAsJson)
    {
        File.WriteAllText(FilePath.ResponseSuccess(communicationId), responseAsJson);
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
        public static string Input(string communicationId) => WorkingDirectory + $"ApiInspector.Input.{communicationId}.json";

        public static string Log => WorkingDirectory + @"ApiInspector.log.txt";

        public static string ResponseFail(string communicationId) => WorkingDirectory + $"ApiInspector.ResponseFail-{communicationId}.json";

        public static string ResponseSuccess(string communicationId) => WorkingDirectory + $"ApiInspector.ResponseSuccess-{communicationId}.json";
    }
}