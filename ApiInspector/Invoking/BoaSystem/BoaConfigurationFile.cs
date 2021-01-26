using System;
using BOA.Common.Configuration;
using static ApiInspector._;

namespace ApiInspector.Invoking.BoaSystem
{
    /// <summary>
    ///     The boa configuration file
    /// </summary>
    static class BoaConfigurationFile
    {
        #region Public Methods
        /// <summary>
        ///     Loads the specified environment information.
        /// </summary>
        public static void Load(Func<string> environmentInfo, Action<string> trace)
        {
            var configurationDirectoryPath = ConfigurationDirectoryPath;

            var configFilePath = $"{configurationDirectoryPath}{environmentInfo()}.config";

            trace($"Loading configuration. File: {configFilePath}");

            ConfigurationManager.LoadConfiguration(configFilePath);
        }
        #endregion
    }
}