﻿using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiInspector;

internal class Program
{
    static string Aloha(string a, int b)
    {
        return "Aloha" + a + b;
    }
    
    
    public static void Main(string[] args)
    {
        //WaitForDebuggerAttach();

        KillAllNamedProcess(nameof(ApiInspector));

        //args = new[] { @"GetMetadataNodes|d:\boa\server\bin\BOA.Types.ERP.PersonRelation.dll" };
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

            var parameters = new object[] {  };
            if (methodInfo.GetParameters().Length == 1)
            {
                parameters = new []{ FileHelper.ReadInput(methodInfo.GetParameters()[0].ParameterType) };
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

    internal static IEnumerable<MetadataNode> GetMetadataNodes(string assemblyFilePath)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
        {
            var fileNameWithoutExtension = new AssemblyName(e.Name).Name;

            if (File.Exists(@"d:\boa\server\bin\" + fileNameWithoutExtension + ".dll"))
            {
                return Assembly.LoadFile(@"d:\boa\server\bin\" + fileNameWithoutExtension + ".dll");
            }

            return null;
        };

        return MetadataHelper.GetMetadataNodes(assemblyFilePath);
    }

    public static string InvokeMethod((string fullAssemblyPath, MethodReference methodReference, string stateJsonTextForDotNetInstanceProperties, string stateJsonTextForDotNetMethodParameters) state)
    {
        var (fullAssemblyPath, methodReference, jsonForInstance, jsonForParameters) = state;

        var assembly = MetadataHelper.LoadAssembly(fullAssemblyPath);
        
        var methodInfo   = assembly.TryLoadFrom(methodReference);
        if (methodInfo == null)
        {
            throw new MissingMemberException(methodReference.FullNameWithoutReturnType);
        }

        object instance = null;
        if (!methodInfo.IsStatic)
        {
            var declaringType = assembly.TryLoadFrom(methodReference.DeclaringType);

            instance = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonForInstance, declaringType);
        }

        var invocationParameters = new List<object>();
        
        var map = (JObject)JsonConvert.DeserializeObject(jsonForParameters, typeof(JObject));

        foreach (var parameterInfo in methodInfo.GetParameters())
        {
            if (map.ContainsKey(parameterInfo.Name))
            {
                invocationParameters.Add(map[parameterInfo.Name].ToObject(parameterInfo.ParameterType));
                continue;
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

        var response = methodInfo.Invoke(instance, invocationParameters.ToArray());

        return JsonConvert.SerializeObject(response);


    }

    public static (string jsonForInstance, string jsonForParameters) GetEditorTexts((string fullAssemblyPath, MethodReference methodReference, string jsonForInstance, string jsonForParameters) state)
    {
        var(fullAssemblyPath, methodReference,  jsonForInstance,  jsonForParameters) = state;


        if (canShowInstanceEditor())
        {
            initializeInstanceJson();
        }

        if (canShowParametersEditor())
        {
            initializeParametersJson();
        }

        return (jsonForInstance, jsonForParameters);

        bool canShowInstanceEditor()
        {
            if (methodReference?.IsStatic == true)
            {
                return false;
            }

            return true;
        }

        bool canShowParametersEditor()
        {
            if (methodReference?.Parameters.Count > 0)
            {
                return true;
            }

            return false;
        }
        
        void initializeInstanceJson()
        {
            var typeOfInstance = methodReference.DeclaringType;
          
            if (typeOfInstance == null)
            {
                return;
            }

            var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonForInstance ?? string.Empty);
            if (map == null)
            {
                map = new Dictionary<string, object>();
            }

            foreach (var propertyInfo in MetadataHelper.LoadAssembly(fullAssemblyPath).TryLoadFrom(typeOfInstance)?.GetProperties(BindingFlags.Instance | BindingFlags.Public) ?? new PropertyInfo[] { })
            {
                var name = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;

                if (name is "state")
                {
                    if (propertyType.Name == "EmptyState")
                    {
                        continue;
                    }

                    if (!map.ContainsKey(name))
                    {
                        map.Add(name, ReflectionHelper.CreateDefaultValue(propertyType));
                        continue;
                    }
                }

                if (propertyInfo.DeclaringType?.IsAbstract == true)
                {
                    continue;
                }

                if (propertyType.BaseType == typeof(MulticastDelegate))
                {
                    continue;
                }

                if (!map.ContainsKey(name))
                {
                    map.Add(name, ReflectionHelper.CreateDefaultValue(propertyType));
                }
            }

            jsonForInstance = JsonConvert.SerializeObject(map, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented
            });
        }

        void initializeParametersJson()
        {
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

                map.Add(name, ReflectionHelper.CreateDefaultValue(parameterInfo.ParameterType));
            }

            jsonForParameters = JsonConvert.SerializeObject(map, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented
            });
        }
    }


    static void WaitForDebuggerAttach()
    {
        while (!Debugger.IsAttached)
        {
            Thread.Sleep(100);
        }
    }

    public static void KillAllNamedProcess(string processName)
    {
        foreach (var process in Process.GetProcessesByName(processName))
        {
            if (Process.GetCurrentProcess().Id != process.Id)
            {
                process.Kill();
            }
        }
    }
}