using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class External
{
    public static (string response, Exception exception) GetEnvironment(string assemblyFileFullPath)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return (null, new Exception("File not exist."));
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return (null, new Exception("Environment not correct."));
        }

        var parameter = assemblyFileFullPath;

        return Execute<string>(runtime.IsDotNetCore, nameof(GetEnvironment), parameter);
    }
    
    public static string GetInstanceEditorJsonText(string assemblyFileFullPath, MethodReference methodReference, string jsonForInstance)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return null;
        }

        var parameter = (assemblyFileFullPath, methodReference, jsonForInstance);

        return Execute<string>(runtime.IsDotNetCore, nameof(GetInstanceEditorJsonText), parameter).Unwrap();
    }

    public static string GetParametersEditorJsonText(string assemblyFileFullPath, MethodReference methodReference, string jsonForParameters)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return null;
        }

        var parameter = (assemblyFileFullPath, methodReference, jsonForParameters);

        return Execute<string>(runtime.IsDotNetCore, nameof(GetParametersEditorJsonText), parameter).Unwrap();
    }
    

    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFileFullPath, string classFilter, string methodFilter)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return Enumerable.Empty<MetadataNode>();
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return Enumerable.Empty<MetadataNode>();
        }

        var parameter = (assemblyFileFullPath, classFilter, methodFilter);

        return Execute<IEnumerable<MetadataNode>>(runtime.IsDotNetCore, nameof(GetMetadataNodes), parameter).Unwrap();
    }

    public static string InvokeMethod(string assemblyFileFullPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters, bool waitForDebugger)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return $"Assembly not exists. Assembly File: {assemblyFileFullPath}";
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return $"File is not support .net Core or .net Framework. File: {assemblyFileFullPath}";
        }

        var parameter = (assemblyFileFullPath, methodReference, stateJsonTextForDotNetInstanceProperties, stateJsonTextForDotNetMethodParameters);

        return Execute<string>(runtime.IsDotNetCore, nameof(InvokeMethod), parameter, waitForDebugger).Unwrap();
    }

    static (TResponse response, Exception exception) Execute<TResponse>(bool runCoreApp, string methodName, object parameter, bool waitForDebugger = false)
    {
        FileHelper.WriteInput(JsonConvert.SerializeObject(parameter, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore }));

        var exitCode = RunProcess(runCoreApp, methodName, waitForDebugger);
        if (exitCode == 1)
        {
            return (JsonConvert.DeserializeObject<TResponse>(FileHelper.ReadResponse()), null);
        }

        var messagePrefix = runCoreApp ? "(NetCore)" : "(NetFramework)";
        
        if (exitCode == 0)
        {
            return (default, new Exception(messagePrefix + FileHelper.TakeResponseAsFail()));
        }

        return (default, new Exception($"{messagePrefix} Unexpected exitCode: {exitCode}"));
    }

    static int RunProcess(bool runCoreApp, string methodName, bool waitForDebugger)
    {
        var process = new Process();
        process.StartInfo.FileName  = runCoreApp ? DotNetCoreInvokerExePath : DotNetFrameworkInvokerExePath;
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


