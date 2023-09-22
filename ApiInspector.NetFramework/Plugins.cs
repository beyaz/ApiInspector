using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ApiInspector;

static class Plugins
{
    static IEnumerable<PluginInfo> ListOfPlugins
    {
        get
        {
            var plugins = new List<PluginInfo>();

            var directory = Path.GetDirectoryName(typeof(Plugins).Assembly.Location);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return plugins;
            }

            foreach (var file in Directory.GetFiles(directory, "*.json"))
            {
                if (Path.GetFileNameWithoutExtension(file).StartsWith("ApiInspector.Plugin.", StringComparison.OrdinalIgnoreCase))
                {
                    plugins.Add(JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(file)));
                }
            }

            return plugins;
        }
    }

    public static (bool isProcessed, object invocationResponse, Exception invocationException) AfterInvokeMethod(MethodInfo targetMethodInfo, object instance, object[] methodParameters, object targetMethodResponse, Exception invocationException)
    {
        foreach (var plugin in ListOfPlugins)
        {
            var (isInvoked, invocationResponse) = TryInvokeStaticMethodFromPlugin<(bool isProcessed, object invocationResponse, Exception invocationException)>(plugin, nameof(AfterInvokeMethod), new[]
            {
                targetMethodInfo,
                instance,
                methodParameters,
                targetMethodResponse,
                invocationException
            });
            if (isInvoked && invocationResponse.isProcessed)
            {
                return invocationResponse;
            }
        }

        return (isProcessed: false, null, null);
    }

    public static (bool isSuccessfullyCreated, object instance) GetDefaultValueForJson(Type type)
    {
        foreach (var plugin in ListOfPlugins)
        {
            var (isInvoked, response) = TryInvokeStaticMethodFromPlugin<(bool isSuccessfullyCreated, object instance)>(plugin, nameof(GetDefaultValueForJson), new object[]
            {
                type
            });
            if (isInvoked && response.isSuccessfullyCreated)
            {
                return response;
            }
        }

        return (false, null);
    }

    public static string GetEnvironment(string assemblyFileName)
    {
        foreach (var plugin in ListOfPlugins)
        {
            var (isInvoked, environmentInfo) = TryInvokeStaticMethodFromPlugin<string>(plugin, nameof(GetEnvironment), assemblyFileName);
            if (isInvoked && environmentInfo != null)
            {
                return environmentInfo;
            }
        }

        return null;
    }

    public static (Exception exception, bool isInvoked, object invocationOutput) InvokeMethod(MethodInfo targetMethodInfo, object instance, object[] methodParameters)
    {
        foreach (var plugin in ListOfPlugins)
        {
            var (isInvoked, response) = TryInvokeStaticMethodFromPlugin<(Exception exception, bool isInvoked, object invocationOutput)>(plugin, nameof(InvokeMethod), new[]
            {
                targetMethodInfo,
                instance,
                methodParameters
            });
            if (isInvoked && response.exception is not null || response.isInvoked)
            {
                return response;
            }
        }

        return (null, false, null);
    }

    public static (Exception occurredErrorWhenCreatingInstance, bool isSuccessfullyCreated, object instance) TryCreateInstance(Type type, string json)
    {
        foreach (var plugin in ListOfPlugins)
        {
            var (isInvoked, response) = TryInvokeStaticMethodFromPlugin<(Exception occurredErrorWhenCreatingInstance, bool isSuccessfullyCreated, object instance)>(plugin, nameof(TryCreateInstance), type, json);
            if (isInvoked && response.isSuccessfullyCreated)
            {
                return response;
            }
        }

        return (null, false, null);
    }

    public static string TryFindFullFilePathOfAssembly(string assemblyFileName)
    {
        foreach (var plugin in ListOfPlugins)
        {
            var (isInvoked,fullFilePathOfAssembly) = TryInvokeStaticMethodFromPlugin<string>(plugin, nameof(TryFindFullFilePathOfAssembly), assemblyFileName);
            if (isInvoked && fullFilePathOfAssembly != null)
            {
                return fullFilePathOfAssembly;
            }
        }

        return null;
    }

    static (bool isInvoked, TMethodOutput output) TryInvokeStaticMethodFromPlugin<TMethodOutput>(PluginInfo plugin, string methodName,params object[] methodArguments)
    {
        if (!File.Exists(plugin.FullFilePathOfAssembly))
        {
            return default;
        }

        var assembly = Assembly.LoadFrom(plugin.FullFilePathOfAssembly);

        var helperType = assembly.GetType(plugin.FullClassName);
        if (helperType is null)
        {
            return default;
        }

        var methodInfo = helperType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (methodInfo is null)
        {
            return default;
        }

        return (isInvoked: true, (TMethodOutput)methodInfo.Invoke(null, methodArguments));
    }

    sealed class PluginInfo
    {
        public string FullClassName { get; set; }

        public string FullFilePathOfAssembly { get; set; }
    }
}