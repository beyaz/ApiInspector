using System;
using System.IO;
using ApiInspector.Serialization;
using ApiInspector.Tracing;
using BOA.Common.Helpers;

namespace ApiInspector.Invoking.BoaSystem
{
    /// <summary>
    ///     The environment variable
    /// </summary>
    class EnvironmentVariable
    {
        #region Fields
        /// <summary>
        ///     The configuration directory
        /// </summary>
        readonly BoaConfigurationDirectory configurationDirectory;

        /// <summary>
        ///     The tracer
        /// </summary>
        readonly TraceQueue tracer;

        /// <summary>
        ///     The user name
        /// </summary>
        string userName;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnvironmentVariable" /> class.
        /// </summary>
        public EnvironmentVariable(BoaConfigurationDirectory configurationDirectory, TraceQueue tracer)
        {
            this.configurationDirectory = configurationDirectory ?? throw new ArgumentNullException(nameof(configurationDirectory));
            this.tracer                 = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the output file path.
        /// </summary>
        string OutputFilePath => Path.Combine(configurationDirectory.GetDirectoryPath(), "EnvironmentVariables.json");
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        public string GetUserName()
        {
            if (userName != null)
            {
                return userName;
            }

            if (!File.Exists(OutputFilePath))
            {
                CreateOutputFile();
            }

            var model = SerializeHelper.JsonToObject<EnvironmentVariableFileModel>(File.ReadAllText(OutputFilePath));

            if (!string.IsNullOrWhiteSpace(model.UserName))
            {
                userName = model.UserName.Trim();

                tracer.Trace($"UserName:{userName}");

                return userName;
            }

            userName = Environment.UserName;

            tracer.Trace($"UserName:{userName}");

            return userName;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the output file.
        /// </summary>
        void CreateOutputFile()
        {
            var environmentVariableFileModel = new EnvironmentVariableFileModel
            {
                UserName = Environment.UserName
            };

            var serializer = new Serializer();

            var fileContent = serializer.SerializeToJsonDoNotIgnoreDefaultValues(environmentVariableFileModel);

            Utility.WriteAllText(OutputFilePath, fileContent);
        }
        #endregion
    }

    /// <summary>
    ///     The environment variable file model
    /// </summary>
    [Serializable]
    public class EnvironmentVariableFileModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }
        #endregion
    }
}