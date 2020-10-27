using System;
using ApiInspector.Tracing;
using BOA.Common.Configuration;
using static ApiInspector.Invoking.BoaSystem.BoaConfigurationDirectory;

namespace ApiInspector.Invoking.BoaSystem
{
    /// <summary>
    ///     The boa configuration file
    /// </summary>
    class BoaConfigurationFile
    {
        #region Fields
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
        public BoaConfigurationFile(EnvironmentInfo environmentInfo, ITracer tracer)
        {
            this.environmentInfo = environmentInfo ?? throw new ArgumentNullException(nameof(environmentInfo));
            this.tracer          = tracer ?? throw new ArgumentNullException(nameof(tracer));
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

            LoadBOAConfigurationFile(() => environmentInfo.ToString(), tracer.Trace);

            isLoaded = true;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Loads the boa configuration file.
        /// </summary>
        static void LoadBOAConfigurationFile(Func<string> environmentInfo, Action<string> trace)
        {
            var configurationDirectoryPath = GetConfigurationDirectoryPath();

            var configFilePath = $"{configurationDirectoryPath}{environmentInfo()}.config";

            trace($"Loading configuration. File: {configFilePath}");

            ConfigurationManager.LoadConfiguration(configFilePath);
        }
        #endregion
    }
}