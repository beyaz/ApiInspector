using System;
using System.IO;
using ApiInspector.Invoking.BoaSystem;
using Dapper;
using static ApiInspector.Utility;

namespace ApiInspector
{
    static partial class _
    {
        #region Static Fields
        public static Action<string> UserVisibleTrace = Console.WriteLine;
        public static Action ClearUserVisibleTrace = () => { };

        #endregion

        #region Public Methods
        //public static string FindWebConfigFilePath(string rootDirectory, string projectName)
        //{
        //    var webConfigFilePath = Path.Combine(rootDirectory, projectName, "Web.config");
        //    if (File.Exists(webConfigFilePath))
        //    {
        //        return webConfigFilePath;
        //    }

        //    if (File.Exists(Path.Combine(rootDirectory, "Web.config")))
        //    {
        //        return null;
        //    }

        //    foreach (var directory in Directory.GetDirectories(rootDirectory))
        //    {
        //        var configFilePath = GetWebConfigFilePath(directory, projectName);
        //        if (configFilePath != null)
        //        {
        //            return configFilePath;
        //        }
        //    }

        //    return null;
        //}

        public static string GetWebConfigFilePath(string interfaceTypeAssemblyQualifiedName, string environment)
        {
            interfaceTypeAssemblyQualifiedName = ChangeVersionNumber(interfaceTypeAssemblyQualifiedName, "1.0.0.0");

            var environmentInfo = EnvironmentInfo.Parse(environment);

            var remoteServiceConfigurationUrl = string.Empty;
            {
                if (environmentInfo.IsDev)
                {
                    remoteServiceConfigurationUrl = "http://Srvcardmdlsdev1/Configuration/configuration/management/api";
                    environment                   = "Development";
                }
                else if (environmentInfo.IsTest)
                {
                    remoteServiceConfigurationUrl = "http://Srvcardtest1/Configuration/configuration/management/api";
                    environment                   = "Test";
                }
                else if (environmentInfo.IsPrep)
                {
                    remoteServiceConfigurationUrl = "http://sxlcard1/Configuration/configuration/management/api";
                    environment                   = "Preproduction";
                }
            }

            var sql = $@"
SELECT TOP 1 shd.DOMAIN_ID
  FROM CFG.SERVICE_REGISTRATION sr  WITH(NOLOCK) INNER JOIN
       CFG.SERVICE_HOST_DEF     shd WITH(NOLOCK) ON sr.SERVICE_TYPE_ID = shd.SERVICE_TYPE_ID
 WHERE sr.RESTFUL_CLIENT_INTERFACE = @{nameof(interfaceTypeAssemblyQualifiedName)}
   AND shd.ENVIRONMENT = @{nameof(environment)}

";
            var domainId = GetBoaCardDbConnection(environmentInfo).QuerySingleOrDefault<string>(sql, new
            {
                interfaceTypeAssemblyQualifiedName,
                environment
            });

            if (string.IsNullOrWhiteSpace(domainId))
            {
                throw new InvalidOperationException($"DomainId not found. {nameof(interfaceTypeAssemblyQualifiedName)} : {interfaceTypeAssemblyQualifiedName} | {nameof(environment)} : {environment}");
            }

            UserVisibleTrace($"DomainId: {domainId}");

            var content = $@"
<?xml version='1.0' encoding='utf-8'?>
<configuration>
  <configSections>
    <section name='boa.card.setup' type='BOA.Card.Core.ServiceBus.Configuration.EverestSetupConfigurationSection, BOA.Card.Core, Culture=neutral, PublicKeyToken=null'/>
    <section name='log4net' type='log4net.Config.Log4NetConfigurationSectionHandler, log4net'/>
  </configSections>
  <log4net>
    <root>
      <level value='DEBUG'/>
      <appender-ref ref='LogFileAppender'/>
    </root>
    <appender name='LogFileAppender' type='log4net.Appender.RollingFileAppender'>
      <param name='File' value='d:\BOA\server\bin\CardServiceLogs\ApiInspector.log'/>
      <lockingModel type='log4net.Appender.FileAppender+MinimalLock'/>
      <appendToFile value='true'/>
      <rollingStyle value='Composite'/>
      <datePattern value='.yyyy-MM-dd'/>
      <maxSizeRollBackups value='2'/>
      <maximumFileSize value='1MB'/>
      <staticLogFileName value='true'/>
      <layout type='log4net.Layout.PatternLayout'>
        <conversionPattern value='%date [%level] [%thread] %ndc %message%newline'/>
      </layout>
    </appender>
  </log4net>
  <boa.card.setup>
    <domain id='{domainId}' environment='{environment}'/>
    <remoteServiceConfiguration uri='{remoteServiceConfigurationUrl}'/>
  </boa.card.setup>
  <startup>
    <supportedRuntime version='v4.0' sku='.NETFramework,Version=v4.6.1'/>
  </startup>
</configuration>



";

            var projectName = interfaceTypeAssemblyQualifiedName.Substring(0, interfaceTypeAssemblyQualifiedName.IndexOf(','));

            var filePath = Path.Combine(ConfigurationDirectoryPath, "CardServiceConfigs", projectName, $"{environment}.config");

            WriteToFile(filePath, content.Trim());

            UserVisibleTrace($"CardServiceConfigFileExportedTo: {filePath}");

            return filePath;
        }
        #endregion

        #region Methods
        static string ChangeVersionNumber(string assemblyQualifiedNameOfType, string newVersionNumber)
        {
            var versionIndex = assemblyQualifiedNameOfType.IndexOf("Version=") + "Version=".Length;

            var commaIndex = assemblyQualifiedNameOfType.IndexOf(",", versionIndex);

            var versionNumber = assemblyQualifiedNameOfType.Substring(versionIndex, commaIndex - versionIndex);

            return assemblyQualifiedNameOfType.Replace(versionNumber, newVersionNumber);
        }
        #endregion
    }
}