using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class External
{
    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath)
    {
        return Execute<IEnumerable<MetadataNode>>(nameof(GetMetadataNodes), assemblyFilePath).Unwrap();
    }
  
    

    static TResponse Unwrap<TResponse>(this (TResponse response, Exception exception) tuple)
    {
        if (tuple.exception != null)
        {
            throw tuple.exception;
        }

        return tuple.response;
    }
    
    public static (TResponse response, Exception exception) Execute<TResponse>(string methodName, object parameter)
    {
        FileHelper.WriteInput(JsonConvert.SerializeObject(parameter));
        
        var exitCode = RunProcess(methodName);
        if (exitCode == 1) 
        {
            return (JsonConvert.DeserializeObject<TResponse>(FileHelper.ReadResponse()), null);
        }
        
        if (exitCode == 0)
        {
            return (default, new Exception(FileHelper.TakeResponseAsFail()));
        }

        return (default, new Exception($"Unexpected exitCode: {exitCode}"));
    }


    
    

    static int RunProcess(string methodName)
    {
        Process process = new Process();
        process.StartInfo.FileName  = @"D:\work\git\ApiInspector\ApiInspector\bin\Debug\ApiInspector.exe";
        process.StartInfo.Arguments = $"{methodName}";
        process.Start();
        process.WaitForExit();
        
        return process.ExitCode;
        
        
    }
}


