using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class External
{
    public static (string jsonForInstance, string jsonForParameters) GetEditorTexts(string fullAssemblyPath, MethodReference methodReference, string jsonForInstance, string jsonForParameters)
    {
        return Execute<(string jsonForInstance, string jsonForParameters)>(nameof(GetEditorTexts), (fullAssemblyPath, methodReference, jsonForInstance, jsonForParameters)).Unwrap();
    }

    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath)
    {
        return Execute<IEnumerable<MetadataNode>>(nameof(GetMetadataNodes), assemblyFilePath).Unwrap();
    }

    public static string InvokeMethod(string fullAssemblyPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters, bool waitForDebugger)
    {
        return Execute<string>(nameof(InvokeMethod), (fullAssemblyPath, methodReference, stateJsonTextForDotNetInstanceProperties, stateJsonTextForDotNetMethodParameters), waitForDebugger).Unwrap();
    }

    static (TResponse response, Exception exception) Execute<TResponse>(string methodName, object parameter, bool waitForDebugger = false)
    {
        FileHelper.WriteInput(JsonConvert.SerializeObject(parameter));

        var exitCode = RunProcess(methodName, waitForDebugger);
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

    static int RunProcess(string methodName, bool waitForDebugger)
    {
        var process = new Process();
        process.StartInfo.FileName  = @"D:\work\git\ApiInspector\ApiInspector\bin\Debug\ApiInspector.exe";
        process.StartInfo.Arguments = $"{(waitForDebugger ? "1" : "0")}|{methodName}";
        process.Start();
        process.WaitForExit();

        return process.ExitCode;
    }

    static TResponse Unwrap<TResponse>(this (TResponse response, Exception exception) tuple)
    {
        if (tuple.exception != null)
        {
            throw tuple.exception;
        }

        return tuple.response;
    }
}


