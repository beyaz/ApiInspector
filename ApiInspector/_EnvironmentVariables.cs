using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using ApiInspector.Invoking.BoaSystem;
using static ApiInspector.Utility;
using static System.IO.File;

namespace ApiInspector
{
    sealed class InitialConfiguration
    {
        public string ConfigurationDirectoryPath { get; set; } = @"d:\boa\server\bin\ApiInspectorConfiguration\";
        public bool UseLocalDirectoryForHistory { get; set; }
    }

    static partial class _
    {
        public static InitialConfiguration InitialConfiguration = new InitialConfiguration();

        public static IDbConnection DbConnection
        {
            get
            {
                return new SqlConnection("server=srvdev\\ATLAS;database =BOA;integrated security=true");
            }
        }


        #region Public Properties
        public static string AuthenticationUserName
        {
            get
            {
                // remove old file
                Delete(Path.Combine(ConfigurationDirectoryPath, "EnvironmentVariables.json"));

                var filePath = Path.Combine(ConfigurationDirectoryPath, "BOA.UserName.txt");

                IfFileNotExistsThen(filePath, () => WriteToFile(filePath, Environment.UserName));

                var userName = ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(userName))
                {
                    return Environment.UserName;
                }

                return userName.Trim();
            }
        }

        public static string ConfigurationDirectoryPath
        {
            get { return InitialConfiguration.ConfigurationDirectoryPath; }
        }

        public static IDbConnection GetBoaCardDbConnection(EnvironmentInfo environmentInfo)
        {
            if (environmentInfo.IsDev)
            {
                return new SqlConnection(@"Data Source=srvxdev\zumrut;Initial Catalog=BOACard;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;");
            }
            if (environmentInfo.IsTest)
            {
                return new SqlConnection(@"Data Source=srvxtest\zumrut;Initial Catalog=BOACard;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;");
            }
            if (environmentInfo.IsPrep)
            {
                return new SqlConnection(@"Data Source=srvxprep\zumrut;Initial Catalog=BOACard;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;");
            }

            throw new NotImplementedException(environmentInfo.ToString());
        }


        #endregion
    }
}