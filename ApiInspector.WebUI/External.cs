using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace ApiInspector.WebUI;

static class External
{
    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath)
    {
        return Execute<IEnumerable<MetadataNode>>(nameof(GetMetadataNodes), assemblyFilePath).Unwrap();
    }
    public static string GetAssemblyModel()
    {
        return Execute<string>(nameof(GetAssemblyModel), "abc").Unwrap();
    }

    static TResponse Unwrap<TResponse>(this (TResponse response, Exception exception) tuple)
    {
        if (tuple.exception != null)
        {
            throw tuple.exception;
        }

        return tuple.response;
    }
    
    public static (TResponse response, Exception exception) Execute<TResponse>(string methodName, string parameter)
    {
        var exitCode = RunProcess(methodName, parameter);
        if (exitCode == 1) 
        {
            return (JsonConvert.DeserializeObject<TResponse>(FileHelper.ReadResponse()), null);
        }

        const string error = @"c:\ApiInspector.Response.Error.json";
        if (File.Exists(error))
        {
            var errorMessage = File.ReadAllText(error);

            File.Delete(error);

            return (default, new Exception(errorMessage));
        }

        return (default, new Exception($"Unexpected exitCode: {exitCode}"));
    }


    
    

    static int RunProcess(string methodName, string parameter)
    {
        Process process = new Process();
        process.StartInfo.FileName  = @"D:\work\git\ApiInspector\ApiInspector\bin\Debug\ApiInspector.exe";
        process.StartInfo.Arguments = $"{methodName}|{parameter}";
        process.Start();
        process.WaitForExit();
        
        return process.ExitCode;
        
        
    }
}


