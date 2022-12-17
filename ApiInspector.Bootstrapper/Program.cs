using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;

namespace ApiInspector.Bootstrapper
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("Checking version...");

            using var client = new HttpClient();

            var versionUrl = (string)AppContext.GetData("VersionUrl");
            if (versionUrl == null)
            {
                Console.WriteLine($"Value @{nameof(versionUrl)} cannot be empty.");
                Console.Read();
                return;
            }

            var newVersionZipFileUrl = (string)AppContext.GetData("NewVersionZipFileUrl");
            if (newVersionZipFileUrl == null)
            {
                Console.WriteLine($"Value @{nameof(newVersionZipFileUrl)} cannot be empty.");
                Console.Read();
                return;
            }

            var installationFolder = (string)AppContext.GetData("InstallationFolder");
            if (installationFolder == null)
            {
                Console.WriteLine($"Value @{nameof(installationFolder)} cannot be empty.");
                Console.Read();
                return;
            }

            var webClient = new WebClient();

            var shouldUpdate = false;

            var remoteVersion = int.Parse(webClient.DownloadString(versionUrl));

            if (!Directory.Exists(installationFolder))
            {
                Console.WriteLine("Downloading first time please wait...");
                shouldUpdate = true;
            }
            else
            {
                var localVersion = int.Parse(File.ReadAllText(Path.Combine(installationFolder, "Version.txt")));

                if (remoteVersion > localVersion)
                {
                    Console.WriteLine("Updating to new version...");
                    shouldUpdate = true;
                }
            }

            if (shouldUpdate)
            {
                var localZipFilePath = Path.Combine(installationFolder, "Remote.zip");

                webClient.DownloadFile(new Uri(newVersionZipFileUrl), localZipFilePath);

                ZipFile.ExtractToDirectory(localZipFilePath, installationFolder);

                File.Delete(localZipFilePath);

                File.WriteAllText(Path.Combine(installationFolder, "Version.txt"), remoteVersion.ToString());
            }

            Process.Start(Path.Combine(installationFolder, "ApiInspector.WebUI.exe"));
        }
    }
}