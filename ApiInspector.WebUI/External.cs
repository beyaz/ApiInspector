using System.Diagnostics;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

sealed record ExternalInvokeInput
{
    // @formatter:off
    
    public required  bool WaitForDebugger { get;init; } 
    
    public required  Action<Process> OnProcessStarted { get; init; }
    
    public required  string InvokerExeFilePath { get; init; }
    
    public required  ExternalInput Input { get; init; }
    
    // @formatter:on
}

static class External
{
    public static Result<string> GetEnvironment(string assemblyFileFullPath)
    {
        return from invokerExeFilePath in TargetRuntimeIndentifier.GetInvokerExePath(assemblyFileFullPath)
               let executeInput = new ExecuteInput
               {
                   AssemblyFileFullPath = assemblyFileFullPath,
                   MethodName           = nameof(GetEnvironment),
                   Parameter = new()
                   {
                       AssemblyFileFullPath = assemblyFileFullPath
                   },
                   InvokerExeFilePath = invokerExeFilePath
               }
               from output in Execute<string>(executeInput)
               select output;
    }

    public static Result<string> GetInstanceEditorJsonText(ExternalInput input)
    {
        return from invokerExeFilePath in TargetRuntimeIndentifier.GetInvokerExePath(input.AssemblyFileFullPath)
               let executeInput = new ExecuteInput
               {
                   AssemblyFileFullPath = input.AssemblyFileFullPath,
                   MethodName           = nameof(GetInstanceEditorJsonText),
                   Parameter            = input,
                   InvokerExeFilePath   = invokerExeFilePath
               }
               from output in Execute<string>(executeInput)
               select output;
    }

    public static Result<string> GetParametersEditorJsonText(ExternalInput input)
    {
        return from invokerExeFilePath in TargetRuntimeIndentifier.GetInvokerExePath(input.AssemblyFileFullPath)
               let executeInput = new ExecuteInput
               {
                   AssemblyFileFullPath = input.AssemblyFileFullPath,
                   MethodName           = nameof(GetParametersEditorJsonText),
                   Parameter            = input,
                   InvokerExeFilePath   = invokerExeFilePath
               }
               from output in Execute<string>(executeInput)
               select output;
    }

    public static Result<string> InvokeMethod(ExternalInvokeInput input)
    {
        var executeInput = new ExecuteInput
        {
            AssemblyFileFullPath = input.Input.AssemblyFileFullPath,
            MethodName           = nameof(InvokeMethod),
            Parameter            = input.Input,
            WaitForDebugger      = input.WaitForDebugger,
            OnProcessStarted     = input.OnProcessStarted,
            InvokerExeFilePath   = input.InvokerExeFilePath
        };

        return Execute<string>(executeInput);
    }

    static Result<string> Execute<TResponse>(ExecuteInput input)
    {
        if (input.InvokerExeFilePath is null)
        {
            return new ArgumentException(nameof(input.InvokerExeFilePath));
        }

        if (!File.Exists(input.AssemblyFileFullPath))
        {
            return new FileNotFoundException(input.AssemblyFileFullPath);
        }

        var inputAsJson = JsonConvert.SerializeObject(input.Parameter, new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.Ignore });

        var runProcessInput = new RunProcessInput
        {
            InputAsJson        = inputAsJson,
            MethodName         = input.MethodName,
            WaitForDebugger    = input.WaitForDebugger,
            OnProcessStarted   = input.OnProcessStarted,
            InvokerExeFilePath = input.InvokerExeFilePath
        };

        var (exitCode, outputAsJson) = RunProcess(runProcessInput);
        if (exitCode == 1)
        {
            return outputAsJson;
        }

        if (exitCode == 0)
        {
            return new Exception(outputAsJson);
        }

        return new Exception($"Unexpected exitCode: {exitCode}");
    }

    static (int exitCode, string outputAsJson) RunProcess(RunProcessInput input)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = input.InvokerExeFilePath,

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

    sealed record ExecuteInput
    {
        // @formatter:off
        
        public string AssemblyFileFullPath { get; init; }
        
        public string MethodName { get; init; }
        
        public ExternalInput Parameter { get; init; }
        
        public bool WaitForDebugger { get; init; }
        
        public Action<Process> OnProcessStarted { get; init; }
        
        public string InvokerExeFilePath { get; init; }
        
        // @formatter:on
    }

    sealed record RunProcessInput
    {
        // @formatter:off
        
        public string InputAsJson { get; init; }
        
        public string MethodName { get; init; }
        
        public bool WaitForDebugger { get; init; }
        
        public Action<Process> OnProcessStarted { get; init; }
        
        public string InvokerExeFilePath { get; init; }
        
        // @formatter:on
    }
}