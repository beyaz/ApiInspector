using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiInspector.Application;

namespace ApiInspector
{
    static partial class _
    {
        public static string GetConfigurationDirectoryPath()
        {
            return @"d:\boa\server\bin\ApiInspectorConfiguration\";
        }

        public static string EnvironmentVariablesJsonFilePath=>Path.Combine(GetConfigurationDirectoryPath(), "EnvironmentVariables.json");

    }
}