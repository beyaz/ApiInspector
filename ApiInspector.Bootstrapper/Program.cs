using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiInspector.Bootstrapper;

static class Program
{

    class Config
    {
        public string VersionUrl { get; init; }
        public string NewVersionZipFileUrl { get; init; }
        public string InstallationFolder { get; init; }
    }

    static Config CalculateConfig()
    {
        var versionUrl = (string)AppContext.GetData("VersionUrl");
        if (versionUrl == null)
        {
            throw new Exception($"Value @{nameof(versionUrl)} cannot be empty.");
        }

        var newVersionZipFileUrl = (string)AppContext.GetData("NewVersionZipFileUrl");
        if (newVersionZipFileUrl == null)
        {
            throw new Exception($"Value @{nameof(newVersionZipFileUrl)} cannot be empty.");
        }

        var installationFolder = (string)AppContext.GetData("InstallationFolder");
        if (installationFolder == null)
        {
            throw new Exception($"Value @{nameof(installationFolder)} cannot be empty.");
        }

        return new Config
        {
            VersionUrl           = versionUrl,
            NewVersionZipFileUrl = newVersionZipFileUrl,
            InstallationFolder   = installationFolder
        };
    }
    
    public static async Task Main()
    {
        try
        {
            await Run(CalculateConfig());
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            Console.Read();
        }
    }

    static async Task Run(Config config)
    {
        KillAllNamedProcess("ApiInspector.WebUI");
        KillAllNamedProcess("ApiInspector");

        Console.WriteLine("Checking version...");

        using var client = new HttpClient();

        var versionUrl = config.VersionUrl;
        var newVersionZipFileUrl = config.NewVersionZipFileUrl;
        var installationFolder = config.InstallationFolder;

        installationFolder = installationFolder.Replace("{MyDocuments}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        var appFolder = Path.Combine(installationFolder, "Api Inspector (.net method invoker)");

        var shouldUpdate = true;

        Console.WriteLine("AppFolder: " + appFolder);

        var localVersionFilePath = Path.Combine(appFolder, "Version.txt");

        if (File.Exists(localVersionFilePath))
        {
            var remoteVersion = await DownloadStringAsync(versionUrl).Then(int.Parse);

            var localVersion = await File.ReadAllTextAsync(localVersionFilePath).Then(int.Parse);

            Console.WriteLine($"Remote Version: {remoteVersion}");
            Console.WriteLine($"Local Version : {localVersion}");

            if (remoteVersion <= localVersion)
            {
                shouldUpdate = false;
            }
        }

        if (shouldUpdate)
        {
            Console.WriteLine("Updating to new version...");

            if (Directory.Exists(appFolder))
            {
                Directory.Delete(appFolder);
            }

            Directory.CreateDirectory(appFolder);

            var localZipFilePath = Path.Combine(installationFolder, "Remote.zip");
            if (File.Exists(localZipFilePath))
            {
                Console.WriteLine("Clearing zip file...");
                File.Delete(localZipFilePath);
            }

            Console.WriteLine($"Downloading... {newVersionZipFileUrl}");

            await DownloadFileAsync(newVersionZipFileUrl, localZipFilePath);

            Console.WriteLine("Extracting...");

            ZipFile.ExtractToDirectory(localZipFilePath, installationFolder, true);

            File.Delete(localZipFilePath);

            var remoteVersion = await DownloadStringAsync(versionUrl).Then(int.Parse);

            await File.WriteAllTextAsync(localVersionFilePath, remoteVersion.ToString());
        }

        CopyPlugins(appFolder);

        StartWebApplication(appFolder);
    }

    static void CopyPlugins(string appFolder)
    {
        var directory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(directory, "*.json"))
        {
            if (Path.GetFileNameWithoutExtension(file).StartsWith("ApiInspector.Plugin.", StringComparison.OrdinalIgnoreCase))
            {
                if (file.IndexOf("NetFramework", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    File.Copy(file, Path.Combine(appFolder, "ApiInspector.NetFramework", Path.GetFileName(file)), true);
                }
                else
                {
                    File.Copy(file, Path.Combine(appFolder, "ApiInspector.NetCore", Path.GetFileName(file)), true);
                }
            }
        }
    }

    static async Task DownloadFileAsync(string url, string localFilePath)
    {
        using var httpClient = new HttpClient();

        File.Delete(localFilePath);

        var directory = Path.GetDirectoryName(localFilePath);
        if (Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fs = new FileStream(localFilePath, FileMode.CreateNew);

        var response = await httpClient.GetAsync(url);

        await response.Content.CopyToAsync(fs);
    }

    static async Task<string> DownloadStringAsync(string url)
    {
        using var httpClient = new HttpClient();

        var str = await httpClient.GetStringAsync(url);

        return str;
    }

    static void KillAllNamedProcess(string processName)
    {
        foreach (var process in Process.GetProcessesByName(processName))
        {
            if (Process.GetCurrentProcess().Id != process.Id)
            {
                process.Kill();
            }
        }
    }

    static void StartWebApplication(string appFolder)
    {
        Console.WriteLine("Starting...");
        
        var process = Process.Start(Path.Combine(appFolder, "ApiInspector.WebUI.exe"));
        
        if (process?.HasExited == true)
        {
            Console.WriteLine($"Exieted with exitCode: {process.ExitCode}"); 
        }
        else
        {
            Console.WriteLine("Started.");
        }
    }

    static async Task<B> Then<A, B>(this Task<A> task, Func<A, B> nextFunc)
    {
        var a = await task;

        return nextFunc(a);
    }
}