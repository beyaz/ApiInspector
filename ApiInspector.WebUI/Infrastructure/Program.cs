using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ApiInspector.WebUI;

public class Program
{
    static readonly int Port = NetworkHelper.GetAvailablePort(Config.NextAvailablePortFrom);

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            if (Config.UseUrls)
            {
                webBuilder.UseUrls(urls: $"https://*:{Port}");
            }
        });
    }

    public static void Main(string[] args)
    {
        ProcessHelper.KillAllNamedProcess("ApiInspector.WebUI");
        
        if (Config.HideConsoleWindow)
        {
            ConsoleWindowUtility.HideConsoleWindow();
        }

        if (Config.UseUrls)
        {
            Process.Start(Config.BrowserExePath, Config.BrowserExeArguments.Replace("{Port}", Port.ToString()));
        }

        CreateHostBuilder(args).Build().Run();
    }
}