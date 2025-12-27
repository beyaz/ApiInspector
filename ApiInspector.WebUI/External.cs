using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

sealed record ExternalInvokeInput
{
    public string RuntimeName { get; init; }
    public string AssemblyFileFullPath { get; init;} 
    public MethodReference MethodReference { get;init; } 
    public string JsonTextForDotNetInstanceProperties { get; init;} 
    public string JsonTextForDotNetMethodParameters { get; init;} 
    public bool WaitForDebugger { get;init; } 
    public Action<Process> OnProcessStarted { get; init; }
}

static class External
{
    public static Result<string> GetEnvironment(string runtimeName, string assemblyFileFullPath)
    {
        var parameter = assemblyFileFullPath;

        var executeInput = new ExecuteInput
        {
            RuntimeName          = runtimeName,
            AssemblyFileFullPath = assemblyFileFullPath,
            MethodName           = nameof(GetEnvironment),
            Parameter            = parameter
        };
        
        return Execute<string>(executeInput);
    }

    public static Result<string> GetInstanceEditorJsonText(string runtimeName,string assemblyFileFullPath, MethodReference methodReference, string jsonForInstance)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForInstance);

        var executeInput = new ExecuteInput
        {
            RuntimeName          = runtimeName,
            AssemblyFileFullPath = assemblyFileFullPath,
            MethodName           = nameof(GetInstanceEditorJsonText),
            Parameter            = parameter
        };
        
        return Execute<string>(executeInput);
    }
    
    public static Result<string> GetParametersEditorJsonText(string runtimeName, string assemblyFileFullPath, MethodReference methodReference, string jsonForParameters)
    {
        var parameter = (assemblyFileFullPath, methodReference, jsonForParameters);

        var executeInput = new ExecuteInput
        {
            RuntimeName          = runtimeName,
            AssemblyFileFullPath = assemblyFileFullPath,
            MethodName           = nameof(GetParametersEditorJsonText),
            Parameter            = parameter
        };
        
        return Execute<string>(executeInput);
    }

    public static Result<string> InvokeMethod(ExternalInvokeInput input)
    {
        var parameter = (assemblyFileFullPath: input.AssemblyFileFullPath, methodReference: input.MethodReference, stateJsonTextForDotNetInstanceProperties: input.JsonTextForDotNetInstanceProperties, stateJsonTextForDotNetMethodParameters: input.JsonTextForDotNetMethodParameters);

        var executeInput = new ExecuteInput
        {
            RuntimeName          = input.RuntimeName,
            AssemblyFileFullPath = input.AssemblyFileFullPath,
            MethodName           = nameof(InvokeMethod),
            Parameter            = parameter,
            WaitForDebugger      = input.WaitForDebugger,
            OnProcessStarted = input.OnProcessStarted
        };
        
        return Execute<string>(executeInput);
    }

    sealed record ExecuteInput
    {
        public string RuntimeName { get; init; }
        public string AssemblyFileFullPath { get; init; }
        public string MethodName { get; init; }
        public object Parameter { get; init; }
        public bool WaitForDebugger { get; init; }
        
        public Action<Process> OnProcessStarted { get; init; }
    }
    
    static Result<TResponse> Execute<TResponse>(ExecuteInput input)
    {
        if (string.IsNullOrWhiteSpace(input.RuntimeName))
        {
            return  new ArgumentException("Select runtime");
        }
        
        var fileInfo = new FileInfo(input.AssemblyFileFullPath);
        if (!fileInfo.Exists)
        {
            return  new FileNotFoundException(input.AssemblyFileFullPath);
        }

        var runtime = GetTargetFramework(fileInfo);
        if (runtime.IsNetCore is false && runtime.IsNetFramework is false && runtime.IsNetStandard is false)
        {
            return  RuntimeNotDetectedException(input.AssemblyFileFullPath);
        }

        var inputAsJson = JsonConvert.SerializeObject(input.Parameter, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore });

        var isNetCore = input.RuntimeName == RuntimeNames.NetCore;

        var runProcessInput = new RunProcessInput
        {
            InputAsJson = inputAsJson, IsNetCoreApp = isNetCore, MethodName = input.MethodName, WaitForDebugger = input.WaitForDebugger
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
        public string InputAsJson{ get; init; }
        public bool IsNetCoreApp{ get; init; }
        public string MethodName{ get; init; }
        public bool WaitForDebugger{ get; init; }
        
        public Action<Process> OnProcessStarted { get; init; }
    }
    
    static (int exitCode, string outputAsJson) RunProcess(RunProcessInput input)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = input.IsNetCoreApp ? DotNetCoreInvokerExePath : DotNetFrameworkInvokerExePath,
            
            Arguments = $"{(input.WaitForDebugger ? "1" : "0")}|{input.MethodName}|{AsyncLogger.ListennigUrl}",
                
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
            writer.Write(input.InputAsJson);
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