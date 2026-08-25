using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiInspector;

static partial class Program
{
    public static Result<string> GetEnvironment(ExternalInput input)
    {
        return "NetVersion: " + Environment.Version.Major;
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
               from instance in Result.From(() => Activator.CreateInstance(declaringType))
               select instance is null ? null : Json.SerializeIncludeDefaultValues(instance);
    }

    public static Result<string> GetParametersEditorJsonText(ExternalInput input)
    {
        return from methodInfo in LoadMethodInfo(input)
               let map = NewDictionaryFrom
               (
                   from parameterInfo in methodInfo.GetParameters()
                   where parameterInfo.Name is not null
                   select (parameterInfo.Name, Activator.CreateInstance(parameterInfo.ParameterType))
               )
               select Json.SerializeIncludeDefaultValues(map);
    }

    public static Result<object> InvokeMethod(ExternalInput input)
    {
        ReflectionHelper.AttachToAssemblyResolveSameDirectory(input.AssemblyFileFullPath);

        return
            // F in d   M e t h o d
            from methodInfo in LoadMethodInfo(input)

            // C r e a t e   T a r g e t   I n s t a n c e
            from instance in CreateDeclaringType(input, methodInfo)

            // I n it i a l i z e   M e t h o d   P a r a m e t e r s
            from methodParameters in CreateParameters(input, methodInfo)

            // I n v o k e
            from output in Invoke(methodInfo, instance, [..methodParameters])

            // O u t p u t
            select output;

        static Result<object> CreateDeclaringType(ExternalInput input, MethodInfo methodInfo)
        {
            if (methodInfo.IsStatic)
            {
                return Result.Success<object>(null);
            }

            if (!string.IsNullOrWhiteSpace(input.JsonForInstance))
            {
                return Json.Deserialize(input.JsonForInstance, methodInfo.DeclaringType!);
            }

            return Activator.CreateInstance(methodInfo.DeclaringType!);
        }

        static Result<IReadOnlyList<object>> CreateParameters(ExternalInput input, MethodInfo methodInfo)
        {
            var parameterInfoList = methodInfo.GetParameters();

            var map = new JObject();

            try
            {
                if (!string.IsNullOrWhiteSpace(input.JsonForParameters))
                {
                    map = JsonConvert.DeserializeObject<JObject>(input.JsonForParameters);
                }
            }
            catch (Exception exception)
            {
                // Is Direct String Input
                {
                    var isOneStringParameter = parameterInfoList.Length == 1 &&
                                               parameterInfoList[0].ParameterType.FullName == "System.String" &&
                                               parameterInfoList[0].Name is not null;

                    if (isOneStringParameter && !string.IsNullOrWhiteSpace(input.JsonForParameters))
                    {
                        return Result.Success<IReadOnlyList<object>>([input.JsonForParameters]);
                    }
                }

                return exception;
            }

            return Result.From(from p in parameterInfoList select Result.From(() => calculateParameterValue(map, p)));

            static object calculateParameterValue(JObject map, ParameterInfo parameterInfo)
            {
                if (parameterInfo.Name is null)
                {
                    // Default Value By Reflection
                    if (parameterInfo.ParameterType.IsValueType)
                    {
                        return Activator.CreateInstance(parameterInfo.ParameterType);
                    }

                    return null;
                }

                var jProperty = map.Property(parameterInfo.Name, StringComparison.Ordinal);
                if (jProperty is not null)
                {
                    return jProperty.Value.ToObject(parameterInfo.ParameterType);
                }

                // Default Value By Reflection
                if (parameterInfo.ParameterType.IsValueType)
                {
                    return Activator.CreateInstance(parameterInfo.ParameterType);
                }

                return null;
            }
        }

        static Result<object> Invoke(MethodInfo methodInfo, object instance, object[] methodParameters)
        {
            object response = null;

            Exception invocationException = null;

            WriteLog("Invocation started");

            try
            {
                WriteLog("Trying to invoke by default reflection");

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
                WriteLog($"Exception occurred: {exception}");

                invocationException = exception.InnerException ?? exception;
            }

            if (invocationException != null)
            {
                return Result.Error<object>(invocationException);
            }

            WriteLog("Invocation is finished successfully");

            return Result.Success(response);
        }
    }

    public static Result<string> IsYourAssembly(ExternalInput input)
    {
        string content = File.ReadAllText(input.AssemblyFileFullPath);

        if (content.IndexOf(".NETFramework,Version=", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return Convert.ToString(true);
        }
        
        return Convert.ToString(false);
    }

    internal static Result<MethodInfo> LoadMethodInfo(ExternalInput input)
    {
        return from assembly in Result.From(() => Assembly.LoadFrom(input.AssemblyFileFullPath))
               from methodInfo in assembly.TryLoadMethod(input.MethodReference).Tap(_ => WriteLog("Target method found."))
               select methodInfo;
    }
}

class Json
{
    internal static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    internal static object Deserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type);
    }

    internal static string Serialize(object instance)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling       = DefaultValueHandling.Ignore,
            Formatting                 = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling      = ReferenceLoopHandling.Ignore
        };
        return JsonConvert.SerializeObject(instance, jsonSerializerSettings);
    }

    internal static string SerializeIncludeDefaultValues(object instance)
    {
        return JsonConvert.SerializeObject(instance, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting           = Formatting.Indented
        });
    }
}