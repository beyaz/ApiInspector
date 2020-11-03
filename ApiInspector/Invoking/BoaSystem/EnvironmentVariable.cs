using System;
using System.IO;
using ApiInspector.Serialization;
using ApiInspector.Tracing;
using BOA.Common.Helpers;
using static ApiInspector.Invoking.BoaSystem.BoaConfigurationDirectory;

namespace ApiInspector.Invoking.BoaSystem
{
    /// <summary>
    ///     The environment variable
    /// </summary>
    class EnvironmentVariable
    {
        #region Fields
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
        public EnvironmentVariable(TraceQueue tracer)
        {
            this.tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets a value indicating whether [use local proxy].
        /// </summary>
        public bool UseLocalProxyForCardServices
        {
            get
            {
                GetUserName();
                return ReadModelFromFile().UseLocalProxyForCardServices;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the output file path.
        /// </summary>
        static string OutputFilePath => Path.Combine(GetConfigurationDirectoryPath(), "EnvironmentVariables.json");
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

            var model = ReadModelFromFile();

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
        static void CreateOutputFile()
        {
            var environmentVariableFileModel = new EnvironmentVariableFileModel
            {
                UserName                     = Environment.UserName,
                UseLocalProxyForCardServices = true
            };

            var serializer = new Serializer();

            var fileContent = serializer.SerializeToJsonDoNotIgnoreDefaultValues(environmentVariableFileModel);

            Utility.WriteToFile(OutputFilePath, fileContent);
        }

        /// <summary>
        ///     Reads the model from file.
        /// </summary>
        static EnvironmentVariableFileModel ReadModelFromFile()
        {
            return SerializeHelper.JsonToObject<EnvironmentVariableFileModel>(File.ReadAllText(OutputFilePath));
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
        ///     Gets or sets a value indicating whether [use local proxy].
        /// </summary>
        public bool UseLocalProxyForCardServices { get; set; }

        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }
        #endregion
    }
}