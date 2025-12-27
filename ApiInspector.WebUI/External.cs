using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class External
{
    public static Result<string> GetEnvironment(string runtimeName, string assemblyFileFullPath)
    {
        var parameter = assemblyFileFullPath;

        var executeInput = new ExecuteInput
        {
            runtimeName          = runtimeName,
            assemblyFileFullPath = assemblyFileFullPath,
            methodName           = nameof(GetEnvironment),
            parameter            = parameter
        };
        
        return Execute<string>(executeInput);
    }

    public static Result<string> GetInstanceEditorJsonText(string runtimeName,string assemblyFileFullPath, MethodReference methodReference, string jsonForInstance)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForInstance);

        var executeInput = new ExecuteInput
        {
            runtimeName          = runtimeName,
            assemblyFileFullPath = assemblyFileFullPath,
            methodName           = nameof(GetInstanceEditorJsonText),
            parameter            = parameter
        };
        
        return Execute<string>(executeInput);
    }
    
    public static Result<string> GetParametersEditorJsonText(string runtimeName, string assemblyFileFullPath, MethodReference methodReference, string jsonForParameters)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForParameters);

        var executeInput = new ExecuteInput
        {
            runtimeName          = runtimeName,
            assemblyFileFullPath = assemblyFileFullPath,
            methodName           = nameof(GetParametersEditorJsonText),
            parameter            = parameter
        };
        
        return Execute<string>(executeInput);
    }

    public static Result<string> InvokeMethod(string runtimeName, string assemblyFileFullPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters, bool waitForDebugger)
    {
        var parameter = (assemblyFileFullPath, methodReference, stateJsonTextForDotNetInstanceProperties, stateJsonTextForDotNetMethodParameters);

        var executeInput = new ExecuteInput
        {
            runtimeName          = runtimeName,
            assemblyFileFullPath = assemblyFileFullPath,
            methodName           = nameof(InvokeMethod),
            parameter            = parameter,
            waitForDebugger      = waitForDebugger
        };
        
        return Execute<string>(executeInput);
    }

    sealed record ExecuteInput
    {
        public string runtimeName;
        public string assemblyFileFullPath;
        public string methodName { get; set; }
        public object parameter;
        public bool waitForDebugger;
        
        public Action<Process> OnProcessStarted { get; init; }
    }
    
    static Result<TResponse> Execute<TResponse>(ExecuteInput input)
    {
        if (string.IsNullOrWhiteSpace(input.runtimeName))
        {
            return  new ArgumentException("Select runtime");
        }
        
        var fileInfo = new FileInfo(input.assemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return  new FileNotFoundException(input.assemblyFileFullPath);
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsNetCore is false && runtime.IsNetFramework is false && runtime.IsNetStandard is false)
        {
            return  RuntimeNotDetectedException(input.assemblyFileFullPath);
        }

        var inputAsJson = JsonConvert.SerializeObject(input.parameter, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore });

        var isNetCore = input.runtimeName == RuntimeNames.NetCore;

        var runProcessInput = new RunProcessInput
        {
            inputAsJson = inputAsJson, runCoreApp = isNetCore, methodName = input.methodName, waitForDebugger = input.waitForDebugger
        };
        
        var (exitCode, outputAsJson) = RunProcess(runProcessInput);
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

    sealed record RunProcessInput
    {
        public string inputAsJson;
        public bool runCoreApp;
        public string methodName;
        public bool waitForDebugger;
        
        public Action<Process> OnProcessStarted { get; init; }
    }
    
    static (int exitCode, string outputAsJson) RunProcess(RunProcessInput input)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = input.runCoreApp ? DotNetCoreInvokerExePath : DotNetFrameworkInvokerExePath,
            
            Arguments = $"{(input.waitForDebugger ? "1" : "0")}|{input.methodName}|{AsyncLogger.ListennigUrl}",
                
            RedirectStandardInput  = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        using var process = new Process();
        
        process.StartInfo = processStartInfo;

        if (input.OnProcessStarted is not null)
        {
            input.OnProcessStarted(process);
        }
        
        process.Start();
        
        using (var writer = process.StandardInput)
        {
            writer.Write(input.inputAsJson);
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