using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

class ConfigInfo
{
    public string BrowserExeArguments { get; set; }
    public string BrowserExePath { get; set; }
    public bool HideConsoleWindow { get; set; }
    public int NextAvailablePortFrom { get; set; }
    public bool UseUrls { get; set; }

    public FileStorageInfo FileStorage { get; set; }
    
    internal class FileStorageInfo
    {
        public bool IsActive { get; set; }
        public string CacheDirectoryFormat { get; set; }
    }
    
}

partial class Extensions
{
    static readonly bool IsRunningInVS = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioEdition"));

    public static readonly ConfigInfo Config = ReadConfig();

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

    public static string DotNetCoreInvokerExePath
    {
        get
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {

                var app = AppFolder.Replace(@"\ApiInspector.WebUI\", @"\ApiInspector.NetCore\");
                return Path.Combine(app, "ApiInspector.exe");
            }
            
            return Path.Combine(AppFolder, "ApiInspector.NetCore", "ApiInspector.exe");
        }
    }

    public static string DotNetFrameworkInvokerExePath => Path.Combine(AppFolder, "ApiInspector.NetFramework", "ApiInspector.exe");

    static ConfigInfo ReadConfig()
    {
        var config = JsonConvert.DeserializeObject<ConfigInfo>(File.ReadAllText(Path.Combine(AppFolder, "ApiInspector.WebUI.Config.json")));

        if (IsRunningInVS)
        {
            config.UseUrls = false;
        }

        return config;
    }
}