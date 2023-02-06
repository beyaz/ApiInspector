using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ApiInspector.ProcessHelper;

namespace ApiInspector;

static class Program
{

   
    
    public static string GetInstanceEditorJsonText((string fullAssemblyPath, MethodReference methodReference, string jsonForInstance) state)
    {
        var (fullAssemblyPath, methodReference, jsonForInstance) = state;

        if (methodReference is null || methodReference.IsStatic)
        {
            return jsonForInstance;
        }

        var typeOfInstance = methodReference.DeclaringType;

        if (typeOfInstance == null)
        {
            return jsonForInstance;
        }

        var declaringType = MetadataHelper.LoadAssembly(fullAssemblyPath).TryLoadFrom(typeOfInstance);
        if (declaringType == null)
        {
            return jsonForInstance;
        }

        // try create from plugins
        {
            var (isSuccessfullyCreated, instance) = Plugins.GetDefaultValueForJson(declaringType);
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
            map = new Dictionary<string, object>();
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

            var (isSuccessfullyCreated, instance) = Plugins.GetDefaultValueForJson(propertyType);
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
            Formatting = Formatting.Indented
        });
    }

    public static string GetParametersEditorJsonText((string fullAssemblyPath, MethodReference methodReference, string jsonForParameters) state)
    {
        var (fullAssemblyPath, methodReference, jsonForParameters) = state;

        if (methodReference is null || methodReference.Parameters.Count == 0)
        {
            return jsonForParameters;
        }

        var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(state.jsonForParameters ?? string.Empty);
        if (map == null)
        {
            map = new Dictionary<string, object>();
        }

        foreach (var parameterInfo in MetadataHelper.LoadAssembly(fullAssemblyPath).TryLoadFrom(methodReference)?.GetParameters() ?? new ParameterInfo[] { })
        {
            var name = parameterInfo.Name;
            if (name == null || map.ContainsKey(name))
            {
                continue;
            }

            var (isSuccessfullyCreated, instance) = Plugins.GetDefaultValueForJson(parameterInfo.ParameterType);
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

    public static string[] GetHelpMessage()
    {
        return new[]
        {
            "Hello from Api Inspector!",
            "This tool that you can execute or debug any .net method.",
            "Select any .net assembly from left side then prepare parameters or instance properties then you can execute selected method.",
            "if you debug any method then press Debug button and attach to process named 'ApiInspector' by visual studio or any other ide."
        };
    }

    public static string InvokeMethod((string fullAssemblyPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters) state)
    {
        var (fullAssemblyPath, methodReference, jsonForInstance, jsonForParameters) = state;

        var assembly = MetadataHelper.LoadAssembly(fullAssemblyPath);

        var methodInfo = assembly.TryLoadFrom(methodReference);
        if (methodInfo == null)
        {
            throw new MissingMemberException(methodReference.FullNameWithoutReturnType);
        }

        object instance = null;
        if (!methodInfo.IsStatic)
        {
            var declaringType = assembly.TryLoadFrom(methodReference.DeclaringType);

            var (isSuccessfullyCreated, createdInstance) = Plugins.TryCreateInstance(declaringType, jsonForInstance);
            if (isSuccessfullyCreated)
            {
                instance = createdInstance;
            }

            if (instance == null)
            {
                if (!string.IsNullOrWhiteSpace(jsonForInstance))
                {
                    instance = JsonConvert.DeserializeObject(jsonForInstance, declaringType);
                }
                else
                {
                    instance = Activator.CreateInstance(declaringType);
                }
            }
        }

        var invocationParameters = new List<object>();

        var map = (JObject)JsonConvert.DeserializeObject(jsonForParameters, typeof(JObject)) ?? new JObject();

        foreach (var parameterInfo in methodInfo.GetParameters())
        {
            if (map.ContainsKey(parameterInfo.Name))
            {
                var jToken = map[parameterInfo.Name];
                if (jToken != null)
                {
                    var (isSuccessfullyCreated, parameterInstance) = Plugins.TryCreateInstance(parameterInfo.ParameterType, jToken.ToString());
                    if (isSuccessfullyCreated)
                    {
                        invocationParameters.Add(parameterInstance);
                        continue;
                    }

                    invocationParameters.Add(jToken.ToObject(parameterInfo.ParameterType));
                    continue;
                }
            }

            if (parameterInfo.ParameterType.IsValueType)
            {
                invocationParameters.Add(Activator.CreateInstance(parameterInfo.ParameterType));
            }
            else
            {
                invocationParameters.Add(null);
            }
        }

        var hasNoError = false;

        try
        {
            Plugins.BeforeInvokeMethod(methodInfo);
            
            var response = methodInfo.Invoke(instance, invocationParameters.ToArray());

            if (response is Task task)
            {
                task.GetAwaiter().GetResult();
                
                var resultProperty = task.GetType().GetProperty("Result");
                if (resultProperty is not null)
                {
                    response = resultProperty.GetValue(task);
                }
            }

            var (isProcessed, isSuccess, processedVersionOfInstance) = Plugins.UnwrapResponse(response);
            if (isProcessed)
            {
                response = processedVersionOfInstance;

                if (isSuccess == null)
                {
                    throw new Exception("UnwrapResponse 'isSuccess' return value cannot be null");
                }

                if (isSuccess == true)
                {
                    hasNoError = true;
                }
            }
            else
            {
                hasNoError = true;
            }

            if (response is string responseAsString)
            {
                return responseAsString;
            }
            
            return JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                Formatting           = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
        }
        finally
        {
            Plugins.TryDisposeInstance(hasNoError, instance);

            foreach (var invocationParameter in invocationParameters)
            {
                Plugins.TryDisposeInstance(hasNoError, invocationParameter);
            }
        }
    }

    public static void Main(string[] args)
    {
        ReflectionHelper.AttachAssemblyResolver();

        KillAllNamedProcess(nameof(ApiInspector));
        
        FileHelper.ClearLog();

        try
        {
            if (args == null)
            {
                throw new Exception("CommandLine arguments cannot be null.");
            }

            if (args.Length == 0)
            {
                throw new Exception("CommandLine arguments cannot be empty.");
            }

            var arr = args[0].Split('|');
            if (arr.Length is not 2)
            {
                throw new Exception($"CommandLine arguments are invalid. @arguments: {args[0]}");
            }

            var waitForDebugger = arr[0];

            var methodName = arr[1];

            if (waitForDebugger == "1")
            {
                WaitForDebuggerAttach();
            }

            var methodInfo = typeof(Program).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new Exception($"CommandLine argument invalid.Method not found. @methodName: {methodName}");
            }

            var parameters = new object[] { };
            if (methodInfo.GetParameters().Length == 1)
            {
                parameters = new[] { FileHelper.ReadInput(methodInfo.GetParameters()[0].ParameterType) };
            }

            var response = methodInfo.Invoke(null, parameters);

            var responseAsJson = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                Formatting                 = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

            FileHelper.WriteSuccessResponse(responseAsJson);

            Environment.Exit(1);
        }
        catch (Exception exception)
        {
            FileHelper.WriteFail(exception);
            Environment.Exit(0);
        }
    }

    internal static IEnumerable<MetadataNode> GetMetadataNodes((string assemblyFilePath, string classFilter, string methodFilter) prm)
    {
        return MetadataHelper.GetMetadataNodes(prm.assemblyFilePath, prm.classFilter, prm.methodFilter);
    }

    static void WaitForDebuggerAttach()
    {
        while (!Debugger.IsAttached)
        {
            Thread.Sleep(100);
        }
    }
}