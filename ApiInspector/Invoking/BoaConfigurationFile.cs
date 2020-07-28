using System;
using System.IO;
using ApiInspector.Serialization;
using ApiInspector.Tracing;
using BOA.Common.Configuration;
using BOA.Common.Helpers;
using BOA.Common.Types;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The boa configuration file
    /// </summary>
    class BoaConfigurationFile
    {
        #region Constants
        const string path = @"d:\boa\server\bin\ApiInspectorConfiguration\";
        #endregion

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
        public AuthenticationRequest GetAuthenticationRequest()
        {
            var jsonFilePath = $"{path}{environmentInfo}.AuthenticationRequest.json";

            if (!File.Exists(jsonFilePath))
            {
                var request = new AuthenticationRequest
                {
                    AuthenticationContext = new AuthenticationContext
                    {
                        UserName = Environment.UserName,
                        Channel  = ChannelContract.Branch,
                        ApplicationEnvironment = ConfigurationManager.ApplicationEnvironmentSection.ApplicationEnvironment.Environment
                    }
                };
                File.WriteAllText(jsonFilePath, new Serializer().SerializeToJsonDoNotIgnoreDefaultValues(request));
            }

            return SerializeHelper.JsonToObject<AuthenticationRequest>(File.ReadAllText(jsonFilePath));
        }

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            if (isLoaded)
            {
                return;
            }

            var configFilePath = $"{path}{environmentInfo}.config";

            tracer.Trace("Loading configuration.");

            ConfigurationManager.LoadConfiguration(configFilePath);

            isLoaded = true;
        }
        #endregion
    }
}