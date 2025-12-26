using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class External
{
    public static Result<string> GetEnvironment(string runtimeName, string assemblyFileFullPath)
    {
        var parameter = assemblyFileFullPath;

        return Execute<string>(runtimeName,assemblyFileFullPath, nameof(GetEnvironment), parameter);
    }

    public static Result<string> GetInstanceEditorJsonText(string runtimeName,string assemblyFileFullPath, MethodReference methodReference, string jsonForInstance)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForInstance);

        return Execute<string>(runtimeName, assemblyFileFullPath, nameof(GetInstanceEditorJsonText), parameter);
    }
    
    public static Result<string> GetParametersEditorJsonText(string runtimeName, string assemblyFileFullPath, MethodReference methodReference, string jsonForParameters)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForParameters);

        return Execute<string>(runtimeName,assemblyFileFullPath, nameof(GetParametersEditorJsonText), parameter);
    }

    public static Result<string> InvokeMethod(string runtimeName, string assemblyFileFullPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters, bool waitForDebugger)
    {
        var parameter = (assemblyFileFullPath, methodReference, stateJsonTextForDotNetInstanceProperties, stateJsonTextForDotNetMethodParameters);

        return Execute<string>(runtimeName, assemblyFileFullPath, nameof(InvokeMethod), parameter, waitForDebugger);
    }

    static Result<TResponse> Execute<TResponse>(string runtimeName, string assemblyFileFullPath, string methodName, object parameter, bool waitForDebugger = false)
    {
        if (string.IsNullOrWhiteSpace(runtimeName))
        {
            return  new ArgumentException("Select runtime");
        }
        
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return  new FileNotFoundException(assemblyFileFullPath);
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsNetCore is false && runtime.IsNetFramework is false && runtime.IsNetStandard is false)
        {
            return  RuntimeNotDetectedException(assemblyFileFullPath);
        }

        var inputAsJson = JsonConvert.SerializeObject(parameter, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore });

        var isNetCore = runtimeName == RuntimeNames.NetCore;

        var (exitCode, outputAsJson) = RunProcess(inputAsJson, isNetCore, methodName, waitForDebugger);
        if (exitCode == 1)
        {
            return JsonConvert.DeserializeObject<TResponse>(outputAsJson);
        }

        if (exitCode == 0)
        {
            return  new Exception(outputAsJson);
        }

        return  new Exception($"Unexpected exitCode: {exitCode}");
    }
    
    static (int exitCode, string outputAsJson) RunProcess(string inputAsJson, bool runCoreApp, string methodName, bool waitForDebugger)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = runCoreApp ? DotNetCoreInvokerExePath : DotNetFrameworkInvokerExePath,
            
            Arguments = $"{(waitForDebugger ? "1" : "0")}|{methodName}|{AsyncLogger.ListennigUrl}",
                
            RedirectStandardInput  = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        using var process = new Process();
        
        process.StartInfo = processStartInfo;

        process.Start();
        
        using (var writer = process.StandardInput)
        {
            writer.Write(inputAsJson);
        }
        
        
        var outputAsJson = process.StandardOutput.ReadToEnd();
        
        process.WaitForExit();

        return (process.ExitCode, outputAsJson);
    }

    static Exception RuntimeNotDetectedException(string assemblyFileFullPath)
    {
        return new Exception($"Runtime not detected. @{assemblyFileFullPath}");
    }
}