﻿using System.Diagnostics;
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
        return new[]
        {
            "Hello from Api Inspector!",
            "This tool that you can execute or debug any .net method.",
            "Select any .net assembly from left side then prepare parameters or instance properties then you can execute selected method.",
            "if you debug any method then press Debug button and attach to process named 'ApiInspector' by visual studio or any other ide."
        };
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
            map = new Dictionary<string, object>();
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

    public static string InvokeMethod( (string fullAssemblyPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters) state)
    {
        var (fullAssemblyPath, methodReference, jsonForInstance, jsonForParameters) = state;

        ReflectionHelper.AttachToAssemblyResolveSameDirectory(fullAssemblyPath);
        ReflectionHelper.AttachAssemblyResolver();

        var assembly = ReflectionHelper.LoadFrom(fullAssemblyPath);

        var methodInfo = assembly.TryLoadFrom(methodReference);
        if (methodInfo == null)
        {
            throw new MissingMemberException(methodReference.FullNameWithoutReturnType);
        }

        object instance = null;
        if (!methodInfo.IsStatic)
        {
            var declaringType = assembly.TryLoadFrom(methodReference.DeclaringType);

            var (occurredErrorWhenCreatingInstance, isSuccessfullyCreated, createdInstance) = Plugin.TryCreateInstance(declaringType, jsonForInstance);

            if (occurredErrorWhenCreatingInstance != null)
            {
                throw occurredErrorWhenCreatingInstance;
            }

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

        // calculate parameters
        object[] methodParameters;
        {
            var parameterInfoList = methodInfo.GetParameters();

            var map = new JObject();
            try
            {
                if (!string.IsNullOrWhiteSpace(jsonForParameters))
                {
                    map = JsonConvert.DeserializeObject<JObject>(jsonForParameters);
                }

                if (parameterInfoList.Length == 1 &&
                    parameterInfoList[0].ParameterType.FullName == "System.String" &&
                    parameterInfoList[0].Name is not null)
                {
                    var jProperty = map.Property(parameterInfoList[0].Name, StringComparison.OrdinalIgnoreCase);
                    if (jProperty == null)
                    {
                        map = new JObject
                        {
                            [parameterInfoList[0].Name] = new JValue(jsonForParameters)
                        };
                    }
                }
            }
            catch (Exception)
            {
                if (parameterInfoList.Length == 1 && parameterInfoList[0].ParameterType.FullName == "System.String" && parameterInfoList[0].Name is not null)
                {
                    map = new JObject
                    {
                        [parameterInfoList[0].Name] = new JValue(jsonForParameters)
                    };
                }
                else
                {
                    throw;
                }
            }

            var invocationParameters = new List<object>();

            foreach (var parameterInfo in parameterInfoList)
            {
                invocationParameters.Add(calculateParameterValue(map, parameterInfo));
            }

            methodParameters = invocationParameters.ToArray();
        }

        object response = null;

        Exception invocationException = null;

        try
        {
            var shouldInvoke = true;

            var (exception, isInvoked, invocationOutput) = Plugin.InvokeMethod(methodInfo, instance, methodParameters);
            if (exception is not null)
            {
                invocationException = exception;

                shouldInvoke = false;
            }

            if (isInvoked)
            {
                response = invocationOutput;

                shouldInvoke = false;
            }

            if (shouldInvoke)
            {
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
            invocationException = exception.InnerException ?? exception;
        }

        var afterInvoke = Plugin.AfterInvokeMethod(methodInfo, instance, methodParameters, response, invocationException);
        if (afterInvoke.isProcessed)
        {
            response = afterInvoke.invocationResponse;

            invocationException = afterInvoke.invocationException;
        }

        if (invocationException != null)
        {
            throw invocationException;
        }

        if (response is string responseAsString)
        {
            return responseAsString;
        }

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

                    return jProperty.Value.ToObject(parameterInfo.ParameterType, new JsonSerializer{ TypeNameHandling = TypeNameHandling.Auto});
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
        
        FileHelper.ClearLog();

        WriteLog("ProgramStarted");

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

            WriteLog($"A r g u m e n t s : {args[0]}");
            
            var arr = args[0].Split('|');
            if (arr.Length is not 2)
            {
                throw new Exception($"CommandLine arguments are invalid. @arguments: {args[0]}");
            }

            var waitForDebugger = arr[0];

            var methodName = arr[1];
            
            if (waitForDebugger == "1")
            {
                WriteLog("WaitingForAttachToDebugger");
                WaitForDebuggerAttach();
                WriteLog("DebuggerAttached");
            }

            var methodInfo = typeof(Program).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new Exception($"CommandLine argument invalid.Method not found. @methodName: {methodName}");
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
        return JsonConvert.SerializeObject(response, new JsonSerializerSettings
        {
            DefaultValueHandling       = DefaultValueHandling.Ignore,
            Formatting                 = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Converters                 = new List<JsonConverter> { new JsonConverterForPropertyInfo() }
        });
    }


    

    static void WaitForDebuggerAttach()
    {
        while (!Debugger.IsAttached)
        {
            Thread.Sleep(100);
        }
    }
}