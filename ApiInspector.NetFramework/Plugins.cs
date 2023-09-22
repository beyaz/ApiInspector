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
            if (!File.Exists(plugin.FullFilePathOfAssembly))
            {
                continue;
            }

            var assembly = Assembly.LoadFrom(plugin.FullFilePathOfAssembly);

            var helperType = assembly.GetType(plugin.FullClassName);
            if (helperType is null)
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(AfterInvokeMethod), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var invocationResponse = ((bool isProcessed, object invocationResponse, Exception invocationException))methodInfo.Invoke(null, new[]
            {
                targetMethodInfo,
                instance,
                methodParameters,
                targetMethodResponse,
                invocationException
            });
            if (invocationResponse.isProcessed)
            {
                return invocationResponse;
            }
        }

        return (isProcessed: false, null, null);
    }

    static (bool isInvoked, TMethodOutput output) TryInvokeStaticMethodFromPlugin<TMethodOutput>(PluginInfo plugin, string methodName, object[] methodArguments)
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
    
    public static (bool isSuccessfullyCreated, object instance) GetDefaultValueForJson(Type type)
    {
        foreach (var plugin in ListOfPlugins)
        {
            var (isInvoked, response) =  TryInvokeStaticMethodFromPlugin<(bool isSuccessfullyCreated, object instance)>(plugin,nameof(GetDefaultValueForJson),new object[]
            {
                type
            });
            if (!isInvoked)
            {
                continue;
            }
            if (response.isSuccessfullyCreated)
            {
                return response;
            }
        }

        return (false, null);
    }

    public static (Exception exception, bool isInvoked, object invocationOutput) InvokeMethod(MethodInfo targetMethodInfo, object instance, object[] methodParameters)
    {
        foreach (var plugin in ListOfPlugins)
        {
            if (!File.Exists(plugin.FullFilePathOfAssembly))
            {
                continue;
            }

            var assembly = Assembly.LoadFrom(plugin.FullFilePathOfAssembly);

            var helperType = assembly.GetType(plugin.FullClassName);
            if (helperType is null)
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(InvokeMethod), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var response = ((Exception exception, bool isInvoked, object invocationOutput))methodInfo.Invoke(null, new[]
            {
                targetMethodInfo,
                instance,
                methodParameters
            });
            if (response.exception is not null || response.isInvoked)
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
            if (!File.Exists(plugin.FullFilePathOfAssembly))
            {
                continue;
            }

            var assembly = Assembly.LoadFile(plugin.FullFilePathOfAssembly);

            var helperType = assembly.GetType(plugin.FullClassName);
            if (helperType is null)
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(TryCreateInstance), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var response = ((Exception occurredErrorWhenCreatingInstance, bool isSuccessfullyCreated, object instance))methodInfo.Invoke(null, new object[] { type, json });
            if (response.isSuccessfullyCreated)
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
            if (!File.Exists(plugin.FullFilePathOfAssembly))
            {
                continue;
            }

            var assembly = Assembly.LoadFrom(plugin.FullFilePathOfAssembly);

            var helperType = assembly.GetType(plugin.FullClassName);
            if (helperType is null)
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(TryFindFullFilePathOfAssembly), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var fullFilePathOfAssembly = (string)methodInfo.Invoke(null, new object[] { assemblyFileName });
            if (fullFilePathOfAssembly != null)
            {
                return fullFilePathOfAssembly;
            }
        }

        return null;
    }

    public static string GetEnvironment(string assemblyFileName)
    {
        foreach (var plugin in ListOfPlugins)
        {
            if (!File.Exists(plugin.FullFilePathOfAssembly))
            {
                continue;
            }

            var assembly = Assembly.LoadFrom(plugin.FullFilePathOfAssembly);

            var helperType = assembly.GetType(plugin.FullClassName);
            if (helperType is null)
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(GetEnvironment), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var environmentInfo = (string)methodInfo.Invoke(null, new object[] { assemblyFileName });
            if (environmentInfo != null)
            {
                return environmentInfo;
            }
        }

        return null;
    }
    sealed class PluginInfo
    {
        public string FullClassName { get; set; }

        public string FullFilePathOfAssembly { get; set; }
    }
}