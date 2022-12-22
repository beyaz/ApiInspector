using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

public class ConfigInfo
{
    public string BrowserExeArguments { get; set; }
    public string BrowserExePath { get; set; }
    public bool HideConsoleWindow { get; set; }
    public int NextAvailablePortFrom { get; set; }
    public bool UseUrls { get; set; }
}

partial class Extensions
{
    public static ConfigInfo Config = JsonConvert.DeserializeObject<ConfigInfo>(File.ReadAllText(Path.Combine(AppFolder, "ApiInspector.WebUI.Config.json")));

    public static string AppFolder
    {
        get
        {
            var location = Path.GetDirectoryName(typeof(Extensions).Assembly.Location);
            if (location == null)
            {
                throw new ArgumentException("assembly location not found");
            }

            return location;
        }
    }

    public static string DotNetCoreInvokerExePath => Path.Combine(AppFolder, "ApiInspector.NetCore", "ApiInspector.exe");

    public static string DotNetFrameworkInvokerExePath => Path.Combine(AppFolder, "ApiInspector.NetFramework", "ApiInspector.exe");
}