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

            var appFolder = Path.Combine(installationFolder, "Api Inspector (.net method invoker)");

            var webClient = new WebClient();

            var shouldUpdate = true;

            var localVersionFilePath = Path.Combine(appFolder, "Version.txt");

            if (File.Exists(localVersionFilePath))
            {
                var remoteVersion = int.Parse(webClient.DownloadString(versionUrl));
                var localVersion  = int.Parse(File.ReadAllText(localVersionFilePath));
                if (remoteVersion == localVersion)
                {
                    shouldUpdate = false;
                }
            }

            if (shouldUpdate)
            {
                Console.WriteLine("Updating to new version...");

                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                var localZipFilePath = Path.Combine(installationFolder, "Remote.zip");

                webClient.DownloadFile(new Uri(newVersionZipFileUrl), localZipFilePath);

                ZipFile.ExtractToDirectory(localZipFilePath, installationFolder,true);

                File.Delete(localZipFilePath);

                var remoteVersion = int.Parse(webClient.DownloadString(versionUrl));
                File.WriteAllText(localVersionFilePath, remoteVersion.ToString());
            }

            Process.Start(Path.Combine(appFolder, "ApiInspector.WebUI.exe"));
        }
    }
}