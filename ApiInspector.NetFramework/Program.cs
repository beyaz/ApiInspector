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
               select instance is null
                   ? null
                   : JsonConvert.SerializeObject(instance, new JsonSerializerSettings
                   {
                       DefaultValueHandling = DefaultValueHandling.Include,
                       Formatting           = Formatting.Indented
                   });
    }

    public static Result<string> GetParametersEditorJsonText(ExternalInput input)
    {
        return from methodInfo in LoadMethodInfo(input)
               let map = CreateNewDictionary
               (
                   from parameterInfo in methodInfo.GetParameters()
                   where parameterInfo.Name is not null
                   select (parameterInfo.Name, Activator.CreateInstance(parameterInfo.ParameterType))
               )
               select JsonConvert.SerializeObject(map, new JsonSerializerSettings
               {
                   DefaultValueHandling = DefaultValueHandling.Include,
                   Formatting           = Formatting.Indented
               });

        static Dictionary<string, object> CreateNewDictionary(IEnumerable<(string name, object value)> items)
        {
            var map = new Dictionary<string, object>();
            foreach (var (name, value) in items)
            {
                map[name] = value;
            }

            return map;
        }
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
            from output in Invoke(methodInfo, instance, methodParameters)

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
                        return Result.Success<object[]>([input.JsonForParameters]);
                    }
                }

                return exception;
            }

            return Result.Success<object[]>([from p in parameterInfoList select calculateParameterValue(map, p)]);

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
        return Convert.ToString(true);
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
}