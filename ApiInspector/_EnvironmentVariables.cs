using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiInspector.Application;
using static ApiInspector.Utility;
using static System.IO.File;

namespace ApiInspector
{
    static partial class _
    {
        public static string GetConfigurationDirectoryPath()
        {
            return @"d:\boa\server\bin\ApiInspectorConfiguration\";
        }

        public static string EnvironmentVariablesJsonFilePath=>Path.Combine(GetConfigurationDirectoryPath(), "EnvironmentVariables.json");


        public static string AuthenticationUserName
        {
            get
            {

                IfFileNotExistsThen(EnvironmentVariablesJsonFilePath, ()=>WriteToFile(EnvironmentVariablesJsonFilePath, Environment.UserName));

                var userName = ReadAllText(EnvironmentVariablesJsonFilePath);

                if (string.IsNullOrWhiteSpace(userName))
                {
                    return Environment.UserName;
                }

                return userName.Trim();

            }
        }
    }
}