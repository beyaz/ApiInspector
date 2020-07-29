using System;
using ApiInspector.Tracing;
using BOA.Common.Configuration;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The boa configuration file
    /// </summary>
    class BoaConfigurationFile
    {
        #region Fields
        readonly BoaConfigurationDirectory configurationDirectory;

        /// <summary>
        ///     The environment information
        /// </summary>
        readonly EnvironmentInfo environmentInfo;

        /// <summary>
        ///     The tracer
        /// </summary>
        readonly ITracer tracer;

        /// <summary>
        ///     The is loaded
        /// </summary>
        bool isLoaded;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="BoaConfigurationFile" /> class.
        /// </summary>
        public BoaConfigurationFile(EnvironmentInfo environmentInfo, ITracer tracer, BoaConfigurationDirectory configurationDirectory)
        {
            this.environmentInfo        = environmentInfo ?? throw new ArgumentNullException(nameof(environmentInfo));
            this.tracer                 = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.configurationDirectory = configurationDirectory ?? throw new ArgumentNullException(nameof(configurationDirectory));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            if (isLoaded)
            {
                return;
            }

            var configurationDirectoryPath = configurationDirectory.GetDirectoryPath();

            var configFilePath = $"{configurationDirectoryPath}{environmentInfo}.config";

            tracer.Trace("Loading configuration.");

            ConfigurationManager.LoadConfiguration(configFilePath);

            isLoaded = true;
        }
        #endregion
    }
}