using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using SystemEnvironment = System.Environment;

namespace ApiInspector;

partial class Program
{
    public static void Main(string[] args)
    {
        ProcessArguments(args, Console.In.ReadToEnd);
    }

    static void ProcessArguments(string[] args, Func<string> readInputJson)
    {
        var originalStdout = Console.Out;

        Console.SetOut(new LogTextWriter());

        WriteLog("Invocation started.");

        try
        {
            string waitForDebugger, methodName, loggerUrl;
            {
                if (args == null)
                {
                    throw new("CommandLine arguments cannot be null.");
                }

                if (args.Length == 0)
                {
                    throw new("CommandLine arguments cannot be empty.");
                }

                var arr = args[0].Split('|');
                if (arr.Length is not 3)
                {
                    throw new($"CommandLine arguments are invalid. @arguments: {args[0]}");
                }

                waitForDebugger = arr[0];

                methodName = arr[1];

                loggerUrl = arr[2];
            }

            Start(loggerUrl);

            if (waitForDebugger == "1")
            {
                WriteLog("WaitingForAttachToDebugger");
                WaitForDebuggerAttach();
                WriteLog("DebuggerAttached");
            }

            string responseAsString = null;
            {
                var inputJson = readInputJson();

                // Attach Same Directory Assembly Resolver
                {
                    var assemblyPath = Regex.Match(inputJson, @"""AssemblyFileFullPath""\s*:\s*""(?<value>[^""]*)""", RegexOptions.IgnoreCase).Groups["value"].Value;
                
                    var baseDirectory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
                
                    AttachAssemblyResolverForSameDirectory(baseDirectory);
                }
                
                var input = Json.Deserialize<ExternalInput>(inputJson);

                switch (methodName)
                {
                    case nameof(GetEnvironment):
                    {
                        responseAsString = GetEnvironment(input).Unwrap();
                        break;
                    }
                    case nameof(GetInstanceEditorJsonText):
                    {
                        responseAsString = GetInstanceEditorJsonText(input).Unwrap();
                        break;
                    }
                    case nameof(GetParametersEditorJsonText):
                    {
                        responseAsString = GetParametersEditorJsonText(input).Unwrap();
                        break;
                    }
                    default:
                    {
                        var output = InvokeMethod(input).Unwrap();
                        if (output is string stringValue)
                        {
                            responseAsString = stringValue;
                        }
                        else if (output is not null)
                        {
                            responseAsString = Json.Serialize(output);
                        }

                        break;
                    }
                }
            }

            Console.SetOut(originalStdout);

            Console.Write(responseAsString);
            

            WriteLog("S U C C E S S");

            WaitAsyncLogsForFinish();

            SystemEnvironment.Exit(1);
        }
        catch (Exception exception)
        {
            if (exception is TargetInvocationException { InnerException: not null } targetInvocationException)
            {
                exception = targetInvocationException.InnerException;
            }

            var failInfoAsJson = Json.Serialize(new
            {
                Type = exception.GetType().FullName,
                exception.Message,
                exception.StackTrace,
                InnerException = exception.InnerException?.ToString()
            });

            Console.SetOut(originalStdout);
            Console.Write(failInfoAsJson);

            WriteLog("F A I L");

            WaitAsyncLogsForFinish();

            SystemEnvironment.Exit(0);
        }
    }
}