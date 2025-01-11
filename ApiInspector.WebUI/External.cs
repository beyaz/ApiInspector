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

        var communicationId = Guid.NewGuid().ToString("N");
        
        
        if (runtime.IsNetStandard)
        {
            isNetCore = true;

            // default pattern: run standard dlls on net core application
            // But sometimes we need to run standard dlls on .netframework application

            FileHelper.WriteInput(communicationId, JsonConvert.SerializeObject(assemblyFileFullPath, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore }));
            exitCode = RunProcess(communicationId, runCoreApp: false, "ShouldNetStandardAssemblyRunOnNetFramework", waitForDebugger: false);
            if (exitCode == 1)
            {
                var shouldNetStandardAssemblyRunOnNetFramework = JsonConvert.DeserializeObject<bool?>(FileHelper.ReadResponse(communicationId));
                if (shouldNetStandardAssemblyRunOnNetFramework is true)
                {
                    isNetCore = false;
                }
            }
        }

        {
            FileHelper.WriteInput(communicationId, inputAsJson);

            exitCode = RunProcess(communicationId, isNetCore, methodName, waitForDebugger);
            if (exitCode == 1)
            {
                return (JsonConvert.DeserializeObject<TResponse>(FileHelper.ReadResponse(communicationId)), null);
            }

            var messagePrefix = isNetCore ? "(NetCore)" : "(NetFramework)";

            if (exitCode == 0)
            {
                return (default, new Exception(messagePrefix + FileHelper.TakeResponseAsFail(communicationId)));
            }
        }

        return (default, new Exception($"Unexpected exitCode: {exitCode}"));
    }

    static int RunProcess(string communicationId, bool runCoreApp, string methodName, bool waitForDebugger)
    {
        var process = new Process();
        process.StartInfo.FileName  = runCoreApp ? DotNetCoreInvokerExePath : DotNetFrameworkInvokerExePath;
        process.StartInfo.Arguments = $"{(waitForDebugger ? "1" : "0")}|{methodName}|{communicationId}";
        process.Start();
        process.WaitForExit();

        return process.ExitCode;
    }

    static Exception RuntimeNotDetectedException(string assemblyFileFullPath)
    {
        return new Exception($"Runtime not detected. @{assemblyFileFullPath}");
    }
}