using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

public class ConfigInfo
{
    public string DotNetFrameworkInvokerExePath { get; set; }
    public string DotNetCoreInvokerExePath { get; set; }

    
    public string BrowserExePath { get; set; }
    public string BrowserExeArguments { get; set; }
    public bool HideConsoleWindow { get; set; }
    public bool UseUrls { get; set; }
    public int NextAvailablePortFrom { get; set; }
}

partial class Extensions
{
    public static ConfigInfo Config = JsonConvert.DeserializeObject<ConfigInfo>(File.ReadAllText("ApiInspector.WebUI.Config.json"));
}