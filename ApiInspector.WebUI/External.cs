using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class External
{
    public static (string response, Exception exception) GetEnvironment(string assemblyFileFullPath)
    {
        var parameter = assemblyFileFullPath;

        return Execute<string>(assemblyFileFullPath, nameof(GetEnvironment), parameter);
    }

    public static (string value, Exception exception) GetInstanceEditorJsonText(string assemblyFileFullPath, MethodReference methodReference, string jsonForInstance)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForInstance);

        return Execute<string>(assemblyFileFullPath, nameof(GetInstanceEditorJsonText), parameter);
    }

    public static Result<IEnumerable<MetadataNode>> GetMetadataNodes(string assemblyFileFullPath, string classFilter, string methodFilter)
    {
        var parameter = (assemblyFileFullPath, classFilter, methodFilter);

        return Execute<IEnumerable<MetadataNode>>(assemblyFileFullPath, nameof(GetMetadataNodes), parameter);
    }

    public static (string value, Exception exception) GetParametersEditorJsonText(string assemblyFileFullPath, MethodReference methodReference, string jsonForParameters)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForParameters);

        return Execute<string>(assemblyFileFullPath, nameof(GetParametersEditorJsonText), parameter);
    }

    public static Result<string> InvokeMethod(string assemblyFileFullPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters, bool waitForDebugger)
    {
        var parameter = (assemblyFileFullPath, methodReference, stateJsonTextForDotNetInstanceProperties, stateJsonTextForDotNetMethodParameters);

        return Execute<string>(assemblyFileFullPath, nameof(InvokeMethod), parameter, waitForDebugger);
    }

    static (TResponse response, Exception exception) Execute<TResponse>(string assemblyFileFullPath, string methodName, object parameter, bool waitForDebugger = false)
    {
        var fileInfo = new FileInfo(assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return (default, new FileNotFoundException(assemblyFileFullPath));
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsNetCore is false && runtime.IsNetFramework is false && runtime.IsNetStandard is false)
        {
            return (default, RuntimeNotDetectedException(assemblyFileFullPath));
        }

        var inputAsJson = JsonConvert.SerializeObject(parameter, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore });

        var isNetCore = runtime.IsNetCore;

        int exitCode;

        {
            FileHelper.WriteInput(inputAsJson);

            exitCode = RunProcess(isNetCore, methodName, waitForDebugger);
            if (exitCode == 1)
            {
                return (JsonConvert.DeserializeObject<TResponse>(FileHelper.ReadResponse()), null);
            }

            var messagePrefix = isNetCore ? "(NetCore)" : "(NetFramework)";

            if (exitCode == 0)
            {
                return (default, new Exception(messagePrefix + FileHelper.TakeResponseAsFail()));
            }
        }

        return (default, new Exception($"Unexpected exitCode: {exitCode}"));
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

    static Exception RuntimeNotDetectedException(string assemblyFileFullPath)
    {
        return new Exception($"Runtime not detected. @{assemblyFileFullPath}");
    }
}