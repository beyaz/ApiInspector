using System.IO;
using System.Threading;
using Newtonsoft.Json;

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

    public static T ReadInput<T>()
    {
        var inputAsJson = File.ReadAllText(FilePath.Input);

        File.Delete(FilePath.Input);

        return JsonConvert.DeserializeObject<T>(inputAsJson);
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

        static bool IsFileLocked(string path)
        {
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
    }

    public static void WriteFail(Exception exception)
    {
        File.WriteAllText(FilePath.ResponseFail, exception.ToString());
    }

    public static void WriteInput(string inputAsJson)
    {
        File.WriteAllText(FilePath.Input, inputAsJson);
    }

    public static void WriteSuccessResponse(string responseAsJson)
    {
        File.WriteAllText(FilePath.ResponseSuccess, responseAsJson);
    }

    static class FilePath
    {
        public static string ResponseFail => WorkingDirectory + @"ApiInspector.ResponseFail.json";
        
        public static string Input => WorkingDirectory + @"ApiInspector.Input.json";

        public static string ResponseSuccess => WorkingDirectory + @"ApiInspector.ResponseSuccess.json";
    }
}