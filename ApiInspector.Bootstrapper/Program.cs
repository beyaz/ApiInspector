using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace ApiInspector.Bootstrapper;

static class Program
{
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

    static Config CalculateConfig()
    {
        var config = new Config
        {
            VersionUrl           = ReadConfig(nameof(Config.VersionUrl)),
            NewVersionZipFileUrl = ReadConfig(nameof(Config.NewVersionZipFileUrl)),
            InstallationFolder   = ReadConfig(nameof(Config.InstallationFolder))
        };

        return config;

        static string ReadConfig(string key)
        {
            var value = (string)AppContext.GetData(key);

            if (value is null)
            {
                throw new($"Value @{key} cannot be empty.");
            }

            value = value.Replace("{MyDocuments}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), StringComparison.OrdinalIgnoreCase);

            return value;
        }
    }

   

    class Network
    {
        internal static async Task DownloadFileAsync(string url, string localFilePath)
        {
            using var httpClient = new HttpClient();

            await using var fs = new FileStream(localFilePath, FileMode.CreateNew);

            var response = await httpClient.GetAsync(url);

            await response.Content.CopyToAsync(fs);
        }

        internal static async Task<string> DownloadStringAsync(string url)
        {
            using var httpClient = new HttpClient();

            return await httpClient.GetStringAsync(url);
        }
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

    static async Task Run(Config config)
    {
        Action<string> trace = Console.WriteLine;

        KillAllNamedProcess("ApiInspector.WebUI");
        KillAllNamedProcess("ApiInspector");

        trace("Checking version...");

        var versionUrl = config.VersionUrl;
        
        var newVersionZipFileUrl = config.NewVersionZipFileUrl;
        
        var installationFolder = config.InstallationFolder;

        var shouldUpdate = true;

        trace("Installation Folder: " + installationFolder);

        var localVersionFilePath = Path.Combine(installationFolder, "Version.txt");

        if (File.Exists(localVersionFilePath))
        {
            var remoteVersion = await Network.DownloadStringAsync(versionUrl).Then(int.Parse);

            var localVersion = await File.ReadAllTextAsync(localVersionFilePath).Then(int.Parse);

            trace($"Remote Version: {remoteVersion}");
            trace($"Local Version : {localVersion}");

            if (remoteVersion <= localVersion)
            {
                shouldUpdate = false;
            }
        }

        if (shouldUpdate)
        {
            trace("Updating to new version...");

            if (Directory.Exists(installationFolder))
            {
                trace($"Deleting previous version...{installationFolder}");
                var isDeleted = await TryDeleteDirectory(installationFolder);
                if (!isDeleted)
                {
                    trace($"Please remove folder manually. {installationFolder}");
                    Console.Read();
                    return;
                }

                trace("Previous version successfully removed.");
            }

            Directory.CreateDirectory(installationFolder);

            var localZipFilePath = Path.Combine(installationFolder, "Remote.zip");
            if (File.Exists(localZipFilePath))
            {
                trace("Clearing zip file...");
                File.Delete(localZipFilePath);
            }

            trace($"Downloading... {newVersionZipFileUrl}");

            await Network.DownloadFileAsync(newVersionZipFileUrl, localZipFilePath);

            trace("Extracting...");

            ZipFile.ExtractToDirectory(localZipFilePath, installationFolder, true);

            File.Delete(localZipFilePath);

            var remoteVersion = await Network.DownloadStringAsync(versionUrl).Then(int.Parse);

            await File.WriteAllTextAsync(localVersionFilePath, remoteVersion.ToString());
        }

        // S t a r t   a p p l i c a t i o n
        {
            trace("Starting...");

            var filePath = Path.Combine(installationFolder, "ApiInspector.WebUI.exe");

            var processStartInfo = new ProcessStartInfo(filePath)
            {
                WorkingDirectory = installationFolder,

                CreateNoWindow = true,

                UseShellExecute = false
            };

            Process.Start(processStartInfo);

            trace("Started.");
        }
    }

    static async Task<B> Then<A, B>(this Task<A> task, Func<A, B> nextFunc)
    {
        var a = await task;

        return nextFunc(a);
    }

    static async Task<bool> TryDeleteDirectory(string directoryPath, int maxRetries = 10, int millisecondsDelay = 30)
    {
        if (directoryPath == null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        if (maxRetries < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries));
        }

        if (millisecondsDelay < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
        }

        for (var i = 0; i < maxRetries; ++i)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }

                return true;
            }
            catch (IOException)
            {
                await Task.Delay(millisecondsDelay);
            }
            catch (UnauthorizedAccessException)
            {
                await Task.Delay(millisecondsDelay);
            }
        }

        return false;
    }

    sealed record Config
    {
        public string InstallationFolder { get; init; }
        public string NewVersionZipFileUrl { get; init; }
        public string VersionUrl { get; init; }
    }
}