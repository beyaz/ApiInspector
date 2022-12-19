﻿using System;
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
            try
            {
                Console.WriteLine("Checking version...");

                using var client = new HttpClient();

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

                installationFolder = installationFolder.Replace("{MyDocuments}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

                var appFolder = Path.Combine(installationFolder, "Api Inspector (.net method invoker)");

                var webClient = new WebClient();

                var shouldUpdate = true;

                Console.WriteLine("AppFolder: " + appFolder);

                var localVersionFilePath = Path.Combine(appFolder, "Version.txt");

                if (File.Exists(localVersionFilePath))
                {
                    var remoteVersion = int.Parse(webClient.DownloadString(versionUrl));
                    var localVersion  = int.Parse(File.ReadAllText(localVersionFilePath));

                    Console.WriteLine($"Remote Version: {remoteVersion}");
                    Console.WriteLine($"Local Version: {localVersion}");

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

                    Console.WriteLine($"Downloading... {newVersionZipFileUrl}");

                    webClient.DownloadFile(new Uri(newVersionZipFileUrl), localZipFilePath);

                    Console.WriteLine("Extracting...");

                    ZipFile.ExtractToDirectory(localZipFilePath, installationFolder, true);

                    File.Delete(localZipFilePath);

                    var remoteVersion = int.Parse(webClient.DownloadString(versionUrl));
                    File.WriteAllText(localVersionFilePath, remoteVersion.ToString());
                }

                // copy plugins
                {
                    var directory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
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
                }

                Console.WriteLine("Starting...");
                Process.Start(Path.Combine(appFolder, "ApiInspector.WebUI.exe"));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.Read();
            }
        }
    }
}