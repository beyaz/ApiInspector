using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiInspector;

static class Program
{
    public static string GetEnvironment(string assemblyFileFullPath)
    {
        ReflectionHelper.AttachToAssemblyResolveSameDirectory(assemblyFileFullPath);
        ReflectionHelper.AttachAssemblyResolver();

        return Plugin.GetEnvironment(assemblyFileFullPath);
    }

    public static string[] GetHelpMessage()
    {
        return
        [
            "Hello from Api Inspector!",
            "This tool that you can execute or debug any .net method.",
            "Select any .net assembly from left side then prepare parameters or instance properties then you can execute selected method.",
            "if you debug any method then press Debug button and attach to process named 'ApiInspector' by visual studio or any other ide."
        ];
    }

    public static string GetInstanceEditorJsonText((string fullAssemblyPath, MethodReference methodReference, string jsonForInstance) state)
    {
        var (fullAssemblyPath, methodReference, jsonForInstance) = state;

        ReflectionHelper.AttachToAssemblyResolveSameDirectory(fullAssemblyPath);
        ReflectionHelper.AttachAssemblyResolver();

        if (methodReference is null || methodReference.IsStatic)
        {
            return jsonForInstance;
        }

        var typeOfInstance = methodReference.DeclaringType;

        if (typeOfInstance == null)
        {
            return jsonForInstance;
        }

        var declaringType = ReflectionHelper.LoadFrom(fullAssemblyPath).TryLoadFrom(typeOfInstance);
        if (declaringType == null)
        {
            return jsonForInstance;
        }

        // try create from plugins
        {
            var (isSuccessfullyCreated, instance) = Plugin.GetDefaultValueForJson(declaringType);
            if (isSuccessfullyCreated)
            {
                return JsonConvert.SerializeObject(instance, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Include,
                    Formatting           = Formatting.Indented
                });
            }
        }

        var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonForInstance ?? string.Empty);
        if (map == null)
        {
            map = new();
        }

        foreach (var propertyInfo in declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var name = propertyInfo.Name;
            var propertyType = propertyInfo.PropertyType;

            if (map.ContainsKey(name))
            {
                continue;
            }

            if (propertyInfo.DeclaringType?.IsAbstract == true)
            {
                continue;
            }

            if (propertyType.BaseType == typeof(MulticastDelegate))
            {
                continue;
            }

            var (isSuccessfullyCreated, instance) = Plugin.GetDefaultValueForJson(propertyType);
            if (isSuccessfullyCreated)
            {
                map.Add(name, instance);
                continue;
            }

            map.Add(name, ReflectionHelper.CreateDefaultValue(propertyType));
        }

        return JsonConvert.SerializeObject(map, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting           = Formatting.Indented
        });
    }

    public static string GetParametersEditorJsonText((string fullAssemblyPath, MethodReference methodReference, string jsonForParameters) state)
    {
        var (fullAssemblyPath, methodReference, jsonForParameters) = state;

        ReflectionHelper.AttachToAssemblyResolveSameDirectory(fullAssemblyPath);
        ReflectionHelper.AttachAssemblyResolver();

        if (methodReference is null || methodReference.Parameters.Count == 0)
        {
            return jsonForParameters;
        }

        var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(state.jsonForParameters ?? string.Empty);
        if (map == null)
        {
            map = new();
        }

        foreach (var parameterInfo in ReflectionHelper.LoadFrom(fullAssemblyPath).TryLoadFrom(methodReference)?.GetParameters() ?? new ParameterInfo[] { })
        {
            var name = parameterInfo.Name;
            if (name == null || map.ContainsKey(name))
            {
                continue;
            }

            var (isSuccessfullyCreated, instance) = Plugin.GetDefaultValueForJson(parameterInfo.ParameterType);
            if (isSuccessfullyCreated)
            {
                map.Add(name, instance);
                continue;
            }

            map.Add(name, ReflectionHelper.CreateDefaultValue(parameterInfo.ParameterType));
        }

        return JsonConvert.SerializeObject(map, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting           = Formatting.Indented
        });
    }

    public static string InvokeMethod((string fullAssemblyPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters) state)
    {
        WriteLog("InvokeStarted");

        var (fullAssemblyPath, methodReference, jsonForInstance, jsonForParameters) = state;

        WriteLog("Inputs");
        WriteLog($"fullAssemblyPath: {fullAssemblyPath}");
        WriteLog($"methodReference: {methodReference.FullNameWithoutReturnType}");
        WriteLog($"jsonForInstance: {jsonForInstance}");
        WriteLog($"jsonForParameters: {jsonForParameters}");

        ReflectionHelper.AttachToAssemblyResolveSameDirectory(fullAssemblyPath);
        ReflectionHelper.AttachAssemblyResolver();

        WriteLog("ResolversAttached");

        var assembly = ReflectionHelper.LoadFrom(fullAssemblyPath);

        var methodInfo = assembly.TryLoadFrom(methodReference);
        if (methodInfo == null)
        {
            throw new MissingMemberException(methodReference.FullNameWithoutReturnType);
        }

        WriteLog("MethodFound");

        object instance = null;
        if (!methodInfo.IsStatic)
        {
            var declaringType = assembly.TryLoadFrom(methodReference.DeclaringType);

            WriteLog("Plugin.TryCreateInstance");
            var (occurredErrorWhenCreatingInstance, isSuccessfullyCreated, createdInstance) = Plugin.TryCreateInstance(declaringType, jsonForInstance);
            if (occurredErrorWhenCreatingInstance != null)
            {
                WriteLog($"occurredErrorWhenCreatingInstance: {occurredErrorWhenCreatingInstance}");
                throw occurredErrorWhenCreatingInstance;
            }

            if (isSuccessfullyCreated)
            {
                WriteLog("PluginSuccessfullyCreatedInstance");
                instance = createdInstance;
            }

            if (instance == null)
            {
                if (!string.IsNullOrWhiteSpace(jsonForInstance))
                {
                    WriteLog("instance = deserialize from jsonForInstance");

                    instance = JsonConvert.DeserializeObject(jsonForInstance, declaringType);
                }
                else
                {
                    WriteLog($"instance = reflection create from declaringType: {declaringType.FullName}");

                    instance = Activator.CreateInstance(declaringType);
                }
            }
        }

        WriteLog("Started to calculate parameters");

        // calculate parameters
        object[] methodParameters;
        {
            var parameterInfoList = methodInfo.GetParameters();

            var map = new JObject();
            try
            {
                if (!string.IsNullOrWhiteSpace(jsonForParameters))
                {
                    WriteLog("Started to deserialize jsonForParameters");

                    map = JsonConvert.DeserializeObject<JObject>(jsonForParameters);
                }

                if (parameterInfoList.Length == 1 &&
                    parameterInfoList[0].ParameterType.FullName == "System.String" &&
                    parameterInfoList[0].Name is not null)
                {
                    var jProperty = map.Property(parameterInfoList[0].Name, StringComparison.OrdinalIgnoreCase);
                    if (jProperty == null)
                    {
                        map = new()
                        {
                            [parameterInfoList[0].Name] = new JValue(jsonForParameters)
                        };
                    }
                }
            }
            catch (Exception exception)
            {
                WriteLog($"Deserialization_failed:{exception}");

                if (parameterInfoList.Length == 1 && parameterInfoList[0].ParameterType.FullName == "System.String" && parameterInfoList[0].Name is not null)
                {
                    WriteLog("Deserialization_failed_but_recalculating_for_string_only_parameter");

                    map = new()
                    {
                        [parameterInfoList[0].Name] = new JValue(jsonForParameters)
                    };
                }
                else
                {
                    WriteLog("Throwing_exception");
                    throw;
                }
            }

            WriteLog("Preparing_invocationParameters");

            var invocationParameters = new List<object>();

            foreach (var parameterInfo in parameterInfoList)
            {
                WriteLog($"Preparing_invocation_parameter: {parameterInfo.Name}");

                invocationParameters.Add(calculateParameterValue(map, parameterInfo));
            }

            methodParameters = invocationParameters.ToArray();
        }

        object response = null;

        Exception invocationException = null;

        WriteLog("Invocation_started");

        try
        {
            var shouldInvoke = true;

            WriteLog("Try_to_invoke_from_plugin");

            var (exception, isInvoked, invocationOutput) = Plugin.InvokeMethod(methodInfo, instance, methodParameters);
            if (exception is not null)
            {
                WriteLog($"Try_to_invoke_from_plugin_has_error: {exception}");

                invocationException = exception;

                shouldInvoke = false;
            }

            if (isInvoked)
            {
                WriteLog("Successfully_invoked_from_plugin");

                response = invocationOutput;

                shouldInvoke = false;
            }

            if (shouldInvoke)
            {
                WriteLog("Trying_invoke_by_default_reflection");

                response = methodInfo.Invoke(instance, methodParameters);
            }

            if (response is Task task)
            {
                task.GetAwaiter().GetResult();

                var resultProperty = task.GetType().GetProperty("Result");
                if (resultProperty is not null)
                {
                    response = resultProperty.GetValue(task);
                }
            }
        }
        catch (Exception exception)
        {
            WriteLog($"Exception_occurred: {exception}");

            invocationException = exception.InnerException ?? exception;
        }

        WriteLog("Started_to_call_plugin_after_invoke_method");
        var afterInvoke = Plugin.AfterInvokeMethod(methodInfo, instance, methodParameters, response, invocationException);
        if (afterInvoke.isProcessed)
        {
            WriteLog("Plugin_after_invoke_method_successfully_called");

            response = afterInvoke.invocationResponse;

            invocationException = afterInvoke.invocationException;
        }

        if (invocationException != null)
        {
            WriteLog($"Throwing_exception: {invocationException}");

            throw invocationException;
        }

        WriteLog("Invocation_is_success");

        if (response is string responseAsString)
        {
            WriteLog($"Returning_already_string_response: {responseAsString}");

            return responseAsString;
        }

        WriteLog("Serializing_invocation_output_to_json");

        return ResponseToJson(response);

        static object calculateParameterValue(JObject map, ParameterInfo parameterInfo)
        {
            if (parameterInfo.Name is not null)
            {
                var jProperty = map.Property(parameterInfo.Name, StringComparison.OrdinalIgnoreCase);
                if (jProperty != null)
                {
                    var (occurredErrorWhenCreatingInstance, isSuccessfullyCreated, parameterInstance) = Plugin.TryCreateInstance(parameterInfo.ParameterType, jProperty.Value.ToString());

                    if (occurredErrorWhenCreatingInstance != null)
                    {
                        throw occurredErrorWhenCreatingInstance;
                    }

                    if (isSuccessfullyCreated)
                    {
                        return parameterInstance;
                    }

                    return jProperty.Value.ToObject(parameterInfo.ParameterType, new() { TypeNameHandling = TypeNameHandling.Auto });
                }
            }

            if (parameterInfo.ParameterType.IsValueType)
            {
                return Activator.CreateInstance(parameterInfo.ParameterType);
            }

            return null;
        }
    }

    public static void Main(string[] args)
    {
        var originalStdout = Console.Out;

        Console.SetOut(TextWriter.Null);

        WriteLog("Invocation started.");

        try
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

            var waitForDebugger = arr[0];

            var methodName = arr[1];

            var loggerUrl = arr[2];

            Start(loggerUrl);
            
            if (waitForDebugger == "1")
            {
                WriteLog("WaitingForAttachToDebugger");
                WaitForDebuggerAttach();
                WriteLog("DebuggerAttached");
            }

            var methodInfo = typeof(Program).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new($"CommandLine argument invalid.Method not found. @methodName: {methodName}");
            }

            var parameters = new object[] { };
            if (methodInfo.GetParameters().Length == 1)
            {
                var inputAsJsonString = Console.In.ReadToEnd();

                WriteLog($"I N P U T : {inputAsJsonString}");

                parameters = [JsonConvert.DeserializeObject(inputAsJsonString, methodInfo.GetParameters()[0].ParameterType)];
            }

            var response = methodInfo.Invoke(null, parameters);

            var responseAsJson = ResponseToJson(response);

            Console.SetOut(originalStdout);
            Console.Write(responseAsJson);

            WriteLog($"ResponseAsJson: {responseAsJson}");

            WriteLog("SuccessfullyExit");

            Environment.Exit(1);
        }
        catch (Exception exception)
        {
            if (exception is TargetInvocationException targetInvocationException)
            {
                if (targetInvocationException.InnerException is not null)
                {
                    exception = targetInvocationException.InnerException;
                }
            }

            var failInfoAsJson = JsonConvert.SerializeObject(exception, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });

            Console.SetOut(originalStdout);
            Console.Write(failInfoAsJson);

            WriteLog($"Failed: {failInfoAsJson}");

            Environment.Exit(0);
        }
    }

    static string ResponseToJson(object response)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling       = DefaultValueHandling.Ignore,
            Formatting                 = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling      = ReferenceLoopHandling.Ignore,
            Converters                 = new List<JsonConverter> { new JsonConverterForPropertyInfo() }
        };
        return JsonConvert.SerializeObject(response, jsonSerializerSettings);
    }

    static void WaitForDebuggerAttach()
    {
        while (!Debugger.IsAttached)
        {
            Thread.Sleep(100);
        }
    }
}