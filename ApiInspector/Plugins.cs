using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace ApiInspector;

static class Plugins
{
    static readonly IReadOnlyList<PluginInfo> ListOfPlugins = new[]
    {
        new PluginInfo
        {
            FullFilePathOfAssembly = @"d:\boa\server\bin\BOA.Orchestration.Card.CCO.dll",
            FullClassName          = "BOA.Orchestration.Card.CCO.TestHelper",
            SupportedMethods       = new[] {  "GetDefaultValueForJson","TryCreateInstance", "TryDisposeInstance" }
        }
    };

    public static (bool isSuccessfullyCreated, object instance) GetDefaultValueForJson(Type type)
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

            if (!plugin.SupportedMethods.Contains(nameof(GetDefaultValueForJson)))
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(GetDefaultValueForJson), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var response = ((bool isSuccessfullyCreated, object instance))methodInfo.Invoke(null, new object[]
            {
                type
            });
            if (response.isSuccessfullyCreated)
            {
                return response;
            }
        }

        return (false, null);
    }

    public static (bool isSuccessfullyCreated, object instance) TryCreateInstance(Type type, JToken jToken)
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

            if (!plugin.SupportedMethods.Contains(nameof(TryCreateInstance)))
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(TryCreateInstance), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var response = ((bool isSuccessfullyCreated, object instance))methodInfo.Invoke(null, new object[] { type, jToken });
            if (response.isSuccessfullyCreated)
            {
                return response;
            }
        }

        return (false, null);
    }

    public static bool TryDisposeInstance(bool hasNoError, object instance)
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

            if (!plugin.SupportedMethods.Contains(nameof(TryDisposeInstance)))
            {
                continue;
            }

            var methodInfo = helperType.GetMethod(nameof(TryDisposeInstance), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var successfullyDisposed = (bool)methodInfo.Invoke(null, new[]
            {
                hasNoError,
                instance
            });
            if (successfullyDisposed)
            {
                return true;
            }
        }

        return false;
    }

    class PluginInfo
    {
        public string FullClassName { get; set; }
        public string FullFilePathOfAssembly { get; set; }
        public IReadOnlyList<string> SupportedMethods { get; set; }
    }
}