using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ApiInspector;

class ConfigInfo
{
    public static ConfigInfo Instance
    {
        get
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty, "ApiInspector.Config.json");
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<ConfigInfo>(File.ReadAllText(path));
            }

            return null;
        }
    }

    public IReadOnlyList<PluginInfo> ListOfPlugins { get; set; }
}