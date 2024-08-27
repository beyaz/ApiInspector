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
            return (default, new FileNotFoundException(assemblyFileFullPath));
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return (default, RuntimeNotDetectedException(assemblyFileFullPath));
        }

        var parameter = assemblyFileFullPath;

        return Execute<string>(runtime.IsDotNetCore, nameof(GetEnvironment), parameter);
    }
    
    public static (string value, Exception exception) GetInstanceEditorJsonText(string assemblyFileFullPath, MethodReference methodReference, string jsonForInstance)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return (default, new FileNotFoundException(assemblyFileFullPath));
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return (default, RuntimeNotDetectedException(assemblyFileFullPath)); 
        }

        var parameter = (assemblyFileFullPath, methodReference, jsonForInstance);

        return Execute<string>(runtime.IsDotNetCore, nameof(GetInstanceEditorJsonText), parameter);
    }

    public static (string value, Exception exception) GetParametersEditorJsonText(string assemblyFileFullPath, MethodReference methodReference, string jsonForParameters)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return (default, new FileNotFoundException(assemblyFileFullPath));
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return (default, RuntimeNotDetectedException(assemblyFileFullPath)); 
        }

        var parameter = (assemblyFileFullPath, methodReference, jsonForParameters);

        return Execute<string>(runtime.IsDotNetCore, nameof(GetParametersEditorJsonText), parameter);
    }

    static Exception RuntimeNotDetectedException(string assemblyFileFullPath)
    {
        return new Exception($"Runtime not detected. @{assemblyFileFullPath}");
    }

    public static Result<IEnumerable<MetadataNode>> GetMetadataNodes(string assemblyFileFullPath, string classFilter, string methodFilter)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return new MetadataNode[]{};
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsDotNetCore is false && runtime.IsDotNetFramework is false)
        {
            return new MetadataNode[] { };
        }

        var parameter = (assemblyFileFullPath, classFilter, methodFilter);

        return Execute<IEnumerable<MetadataNode>>(runtime.IsDotNetCore, nameof(GetMetadataNodes), parameter);
    }

    public static Result<string> InvokeMethod(string assemblyFileFullPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters, bool waitForDebugger)
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

        return Execute<string>(runtime.IsDotNetCore, nameof(InvokeMethod), parameter, waitForDebugger);
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

    
}


