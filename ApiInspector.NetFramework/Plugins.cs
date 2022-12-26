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

    public static (bool isSuccessfullyCreated, object instance) TryCreateInstance(Type type, string json)
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

            var response = ((bool isSuccessfullyCreated, object instance))methodInfo.Invoke(null, new object[] { type, json });
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

    public static string TryFindAssembly(string fileName)
    {
        foreach (var plugin in ListOfPlugins)
        {
            if (plugin.AssemblySearchDirectories is not null)
            {
                foreach (var searchDirectory in plugin.AssemblySearchDirectories)
                {
                    var path = Path.Combine(searchDirectory, fileName);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
        }

        return null;
    }

    public static (bool isProcessed, bool? isSuccess, object processedVersionOfInstance) UnwrapResponse(object responseInstance)
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

            var methodInfo = helperType.GetMethod(nameof(UnwrapResponse), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo is null)
            {
                continue;
            }

            var response = ((bool isProcessed, bool? isSuccess, object processedVersionOfInstance))methodInfo.Invoke(null, new[]
            {
                responseInstance
            });
            if (response.isProcessed)
            {
                return response;
            }
        }

        return (false, null, null);
    }

    sealed class PluginInfo
    {
        public IReadOnlyList<string> AssemblySearchDirectories { get; set; }

        public string FullClassName { get; set; }

        public string FullFilePathOfAssembly { get; set; }
    }
}

