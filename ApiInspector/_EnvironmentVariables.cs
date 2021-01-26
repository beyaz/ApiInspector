using System;
using System.IO;
using static ApiInspector.Utility;
using static System.IO.File;

namespace ApiInspector
{
    static partial class _
    {
        public static string ConfigurationDirectoryPath
        {
            get { return @"d:\boa\server\bin\ApiInspectorConfiguration\"; }
        }

        public static string AuthenticationUserName
        {
            get
            {
                // remove old file
                File.Delete(Path.Combine(ConfigurationDirectoryPath, "EnvironmentVariables.json"));

                var filePath = Path.Combine(ConfigurationDirectoryPath, "BOA.UserName.txt");
                
                IfFileNotExistsThen(filePath, ()=>WriteToFile(filePath, Environment.UserName));

                var userName = ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(userName))
                {
                    return Environment.UserName;
                }

                return userName.Trim();

            }
        }
    }
}