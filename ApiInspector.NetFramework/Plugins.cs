using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ApiInspector;

static class Plugins
{
    static Plugins()
    {
        Plugin = readPluginInfo();
        
        static PluginInfo readPluginInfo()
        {
            var directory = Path.GetDirectoryName(typeof(Plugins).Assembly.Location);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return null;
            }

            foreach (var file in Directory.GetFiles(directory, "*.json"))
            {
                if (Path.GetFileNameWithoutExtension(file).StartsWith("ApiInspector.Plugin.", StringComparison.OrdinalIgnoreCase))
                {
                    return JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(file));
                }
            }

            return null;
        }
    }

    static readonly PluginInfo Plugin;
    

    public static (bool isProcessed, object invocationResponse, Exception invocationException) AfterInvokeMethod(MethodInfo targetMethodInfo, object instance, object[] methodParameters, object targetMethodResponse, Exception invocationException)
    {
        if (Plugin is null)
        {
            return default;
        }
        
        var (isInvoked, invocationResponse) = TryInvokeStaticMethodFromPlugin<(bool isProcessed, object invocationResponse, Exception invocationException)>(Plugin, nameof(AfterInvokeMethod), targetMethodInfo, instance, methodParameters, targetMethodResponse, invocationException);
        if (isInvoked && invocationResponse.isProcessed)
        {
            return invocationResponse;
        }

        return default;
    }

    public static (bool isSuccessfullyCreated, object instance) GetDefaultValueForJson(Type type)
    {
        if (Plugin is null)
        {
            return default;
        }
        
        var (isInvoked, response) = TryInvokeStaticMethodFromPlugin<(bool isSuccessfullyCreated, object instance)>(Plugin, nameof(GetDefaultValueForJson), type);
        if (isInvoked && response.isSuccessfullyCreated)
        {
            return response;
        }

        return default;
    }

    public static string GetEnvironment(string assemblyFileName)
    {
        if (Plugin is null)
        {
            return default;
        }
        
        var (isInvoked, environmentInfo) = TryInvokeStaticMethodFromPlugin<string>(Plugin, nameof(GetEnvironment), assemblyFileName);
        if (isInvoked && environmentInfo != null)
        {
            return environmentInfo;
        }

        return default;
    }

    public static (Exception exception, bool isInvoked, object invocationOutput) InvokeMethod(MethodInfo targetMethodInfo, object instance, object[] methodParameters)
    {
        if (Plugin is null)
        {
            return default;
        }
        
        var (isInvoked, response) = TryInvokeStaticMethodFromPlugin<(Exception exception, bool isInvoked, object invocationOutput)>(Plugin, nameof(InvokeMethod), targetMethodInfo, instance, methodParameters);
        if ((isInvoked && response.exception is not null) || response.isInvoked)
        {
            return response;
        }

        return default;
    }

    public static (Exception occurredErrorWhenCreatingInstance, bool isSuccessfullyCreated, object instance) TryCreateInstance(Type type, string json)
    {
        if (Plugin is null)
        {
            return default;
        }
        
        var (isInvoked, response) = TryInvokeStaticMethodFromPlugin<(Exception occurredErrorWhenCreatingInstance, bool isSuccessfullyCreated, object instance)>(Plugin, nameof(TryCreateInstance), type, json);
        if (isInvoked && response.isSuccessfullyCreated)
        {
            return response;
        }

        return default;
    }

    public static string TryFindFullFilePathOfAssembly(string assemblyFileName)
    {
        if (Plugin is null)
        {
            return default;
        }
        
        var (isInvoked, fullFilePathOfAssembly) = TryInvokeStaticMethodFromPlugin<string>(Plugin, nameof(TryFindFullFilePathOfAssembly), assemblyFileName);
        if (isInvoked && fullFilePathOfAssembly != null)
        {
            return fullFilePathOfAssembly;
        }

        return default;
    }

    static (bool isInvoked, TMethodOutput output) TryInvokeStaticMethodFromPlugin<TMethodOutput>(PluginInfo plugin, string methodName, params object[] methodArguments)
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