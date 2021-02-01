using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiInspector
{
    static partial class _
    {
        #region Public Methods
        /// <summary>
        ///     Use your own App.Config file instead of the default.
        /// </summary>
        public static void ChangeAppConfig(string newAppConfigFileFullPath)
        {
            if (!File.Exists(newAppConfigFileFullPath))
            {
                throw new IOException(newAppConfigFileFullPath);
            }

            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", newAppConfigFileFullPath);
            ResetConfigMechanism();
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Remove cached values from ClientConfigPaths.
        ///     Call this after changing path to App.Config.
        /// </summary>
        static void ResetConfigMechanism()
        {
            var Flags = BindingFlags.NonPublic | BindingFlags.Static;
            /* s_initState holds one of the four internal configuration state.
                0 - Not Started, 1 - Started, 2 - Usable, 3- Complete

               Setting to 0 indicates the configuration is not started, this will 
               hint the AppDomain to reaload the most recent config file set thru 
               .SetData call
               More [here][1]

            */
            typeof(ConfigurationManager)
                .GetField("s_initState", Flags)
                .SetValue(null, 0);

            /*s_configSystem holds the configuration section, this needs to be set 
                as null to enable reload*/
            typeof(ConfigurationManager)
                .GetField("s_configSystem", Flags)
                .SetValue(null, null);

            /*s_current holds the cached configuration file path, this needs to be 
               made null to fetch the latest file from the path provided 
              */
            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName == "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", Flags)
                .SetValue(null, null);
        }
        #endregion
    }
}