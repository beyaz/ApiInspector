using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

public class ConfigInfo
{
    public string DotNetFrameworkInvokerExePath { get; set; }
}

partial class Extensions
{
    public static ConfigInfo Config = JsonConvert.DeserializeObject<ConfigInfo>(File.ReadAllText("ApiInspector.WebUI.Config.json"));
}