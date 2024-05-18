using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

sealed record ConfigInfo
{
    public string BrowserExeArguments { get; init; }
    public string BrowserExePath { get; init; }
    public bool HideConsoleWindow { get; init; }
    public int NextAvailablePortFrom { get; init; }
    public bool UseUrls { get; init; }

    public FileStorageInfo FileStorage { get; init; }

    internal class FileStorageInfo
    {
        public string CacheDirectoryFormat { get; init; }
        public bool IsActive { get; init; }
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
            if (Debugger.IsAttached)
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
            config = config with { UseUrls = false };
        }

        return config;
    }
}