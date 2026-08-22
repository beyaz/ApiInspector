using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiInspector;

static partial class Program
{
    internal static Result<MethodInfo> LoadMethodInfo(ExternalInput input)
    {
        return from assembly in Result.From(() => Assembly.LoadFrom(input.AssemblyFileFullPath))
               from methodInfo in assembly.TryLoadMethod(input.MethodReference).Tap(_ => WriteLog("Target method found."))
               select methodInfo;
    }
    
    public static Result<string> IsYourAssembly(ExternalInput input)
    {
        return Convert.ToString(true);
    }
    
    public static Result<string> GetEnvironment(ExternalInput input)
    {
        return  "NetVersion: " + Environment.Version.Major;
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

    public static Result<string> GetInstanceEditorJsonText(ExternalInput input)
    {
        return from methodInfo in LoadMethodInfo(input)
               from declaringType in Result.NotNull(methodInfo.DeclaringType)
              select CreateJson(input.JsonForInstance, declaringType);


        static string CreateJson(string jsonForInstance, Type declaringType)
        {
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

                

                map.Add(name, ReflectionHelper.CreateDefaultValue(propertyType));
            }

            return JsonConvert.SerializeObject(map, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting           = Formatting.Indented
            });
        }
               
    }
    
    public static Result<string> GetParametersEditorJsonText(ExternalInput input)
    {
        return from methodInfo in LoadMethodInfo(input)
               select CreateParametersJson(input.JsonForParameters, methodInfo);


        static string CreateParametersJson(string jsonForParameters, MethodInfo methodInfo)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonForParameters ?? string.Empty);
            if (map == null)
            {
                map = new();
            }

            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                var name = parameterInfo.Name;
                if (name == null || map.ContainsKey(name))
                {
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
    }
    
   
    public static Result<object> InvokeMethod(ExternalInput input)
    {
        WriteLog("InvokeStarted");

        ReflectionHelper.AttachToAssemblyResolveSameDirectory(input.AssemblyFileFullPath);

        var methodInfo = LoadMethodInfo(input).Unwrap();
       
        object instance = CreateDeclaringType(input,methodInfo).Unwrap();
        
        object[] methodParameters = CreateParameters(input, methodInfo).Unwrap();
        
        return Invoke(methodInfo, instance, methodParameters);

        static Result<object> CreateDeclaringType(ExternalInput input, MethodInfo methodInfo)
        {
            if (methodInfo.IsStatic)
            {
                return Result.Success<object>(null);
            }
            
            if (!string.IsNullOrWhiteSpace(input.JsonForInstance))
            {
                return JsonConvert.DeserializeObject(input.JsonForInstance, methodInfo.DeclaringType!);
            }
            
            return Activator.CreateInstance(methodInfo.DeclaringType!);
        }
        

        

        static Result<object[]> CreateParameters(ExternalInput input, MethodInfo methodInfo)
        {
            var parameterInfoList = methodInfo.GetParameters();

            var map = new JObject();
            try
            {
                if (!string.IsNullOrWhiteSpace(input.JsonForParameters))
                {
                    WriteLog("Started to deserialize jsonForParameters");

                    map = JsonConvert.DeserializeObject<JObject>(input.JsonForParameters);
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
                            [parameterInfoList[0].Name] = new JValue(input.JsonForParameters)
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
                        [parameterInfoList[0].Name] = new JValue(input.JsonForParameters)
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

            return invocationParameters.ToArray();
        }

        
       

        static Result<object> Invoke(MethodInfo methodInfo, object instance, object[] methodParameters)
        {
            object response = null;

            Exception invocationException = null;

            WriteLog("Invocation_started");

            try
            {
                WriteLog("Trying_invoke_by_default_reflection");

                response = methodInfo.Invoke(instance, methodParameters);

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

            return Result.Success(response);
        }

       

        static object calculateParameterValue(JObject map, ParameterInfo parameterInfo)
        {
            var jProperty = parameterInfo.Name switch
            {
                null => null,
                _    => map.Property(parameterInfo.Name, StringComparison.Ordinal)
            };

            return ExecUntilNotNull(parameterInfo, jProperty, [
                tryDeserializeTuple,
                tryCreateFromJsonBySerialization,
                createDefaultValueByReflection
            ]);

            
            static object tryCreateFromJsonBySerialization(ParameterInfo parameterInfo, JProperty jProperty)
            {
                return jProperty?.Value.ToObject(parameterInfo.ParameterType, new()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }

            static object createDefaultValueByReflection(ParameterInfo parameterInfo, JProperty jProperty)
            {
                if (parameterInfo.ParameterType.IsValueType)
                {
                    return Activator.CreateInstance(parameterInfo.ParameterType);
                }

                return null;
            }
            
            static object tryDeserializeTuple(ParameterInfo parameterInfo, JProperty jProperty)
            {
                if (jProperty.Value is not JObject jObject)
                {
                    return null;
                }

                IList<string> elementNames;
                {
                    var tupleNamesAttr = parameterInfo.GetCustomAttribute<System.Runtime.CompilerServices.TupleElementNamesAttribute>();

                    elementNames = tupleNamesAttr?.TransformNames;

                    if (elementNames is null || elementNames.Count == 0)
                    {
                        return null;
                    }
                }

                var parameterType = parameterInfo.ParameterType;
                
                var genericArgs = parameterType.GetGenericArguments();

                var values = new object[genericArgs.Length];

                for (var i = 0; i < genericArgs.Length; i++)
                {
                    var name = elementNames[i];
                    if (name is not null && jObject.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var jToken))
                    {
                        values[i] = jToken.ToObject(genericArgs[i]);
                    }
                    else
                    {
                        name = $"Item{i + 1}";
                        if (jObject.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out  jToken))
                        {
                            values[i] = jToken.ToObject(genericArgs[i]);
                        }
                    }
                }

                return Activator.CreateInstance(parameterType, values);
            }

        }
    }



  

   
}

class Json
{
    public static string SerializeDoNotIgnoreDefaultValues(object o)
    {
        throw new NotImplementedException();
    }
    
    internal static T Deserialize<T>(string json)
    {
        throw new NotImplementedException();
    }

    internal static string Serialize(object instance)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling       = DefaultValueHandling.Ignore,
            Formatting                 = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling      = ReferenceLoopHandling.Ignore,
            Converters                 = new List<JsonConverter> { new JsonConverterForPropertyInfo() }
        };
        return JsonConvert.SerializeObject(instance, jsonSerializerSettings);
    }
}