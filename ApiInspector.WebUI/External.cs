using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class External
{
    public static (string jsonForInstance, string jsonForParameters) GetEditorTexts(string assemblyFileFullPath, MethodReference methodReference, string jsonForInstance, string jsonForParameters)
    {
        var isDotNetCoreAssembly = IsDotNetCoreAssembly(assemblyFileFullPath);

        var parameter = (assemblyFileFullPath, methodReference, jsonForInstance, jsonForParameters);

        return Execute<(string jsonForInstance, string jsonForParameters)>(isDotNetCoreAssembly, nameof(GetEditorTexts), parameter).Unwrap();
    }

    public static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFileFullPath, string classFilter, string methodFilter)
    {
        var isDotNetCoreAssembly = IsDotNetCoreAssembly(assemblyFileFullPath);

        var parameter = (assemblyFileFullPath, classFilter, methodFilter);

        return Execute<IEnumerable<MetadataNode>>(isDotNetCoreAssembly, nameof(GetMetadataNodes), parameter).Unwrap();
    }

    public static string InvokeMethod(string assemblyFileFullPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters, bool waitForDebugger)
    {
        var isDotNetCoreAssembly = IsDotNetCoreAssembly(assemblyFileFullPath);

        var parameter = (assemblyFileFullPath, methodReference, stateJsonTextForDotNetInstanceProperties, stateJsonTextForDotNetMethodParameters);

        return Execute<string>(isDotNetCoreAssembly, nameof(InvokeMethod), parameter, waitForDebugger).Unwrap();
    }

    static (TResponse response, Exception exception) Execute<TResponse>(bool runCoreApp, string methodName, object parameter, bool waitForDebugger = false)
    {
        FileHelper.WriteInput(JsonConvert.SerializeObject(parameter, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore }));

        var exitCode = RunProcess(runCoreApp, methodName, waitForDebugger);
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

    static bool IsDotNetCoreAssembly(string assemblyFileFullPath)
    {
        return GetTargetFramework(new FileInfo(assemblyFileFullPath)).isDotNetCore;
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


