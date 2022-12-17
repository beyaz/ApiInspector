using System;
using System.IO;
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


            var versionUrl = System.AppContext.GetData("VersionUrl") as string;

            var newVersionZipFileUrl = System.AppContext.GetData("NewVersionZipFileUrl") as string;

            var installationFolder = System.AppContext.GetData("InstallationFolder") as string;

            var webClient = new WebClient();
            
            if (!Directory.Exists(installationFolder))
            {
                
                webClient.DownloadFile(new Uri(newVersionZipFileUrl), "d:\\r.zip");
                
                return;
            }
            var remoteVersion = webClient.DownloadString(versionUrl);
            

            
        }
    }
}
