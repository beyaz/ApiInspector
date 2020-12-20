using System;
using System.IO;
using ApiInspector.Serialization;
using ApiInspector.Tracing;
using BOA.Common.Helpers;
using static System.IO.File;
using static System.String;
using static ApiInspector.Invoking.BoaSystem.BoaConfigurationDirectory;
using static ApiInspector.Utility;

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
        public static bool UseLocalProxyForCardServices
        {
            get
            {
                EnsureOutputFileExists();
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
            userName = GetUserName(userName);

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


            var fileContent = Serializer.SerializeToJsonDoNotIgnoreDefaultValues(environmentVariableFileModel);

            WriteToFile(OutputFilePath, fileContent);
        }

        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        public static string GetUserName(string userName)
        {
            if (userName != null)
            {
                return userName;
            }

            EnsureOutputFileExists();

            var model = ReadModelFromFile();

            if (IsNullOrWhiteSpace(model.UserName))
            {
                return Environment.UserName;
            }

            return model.UserName.Trim();
        }

        static void EnsureOutputFileExists()
        {
            IfFileNotExistsThen(OutputFilePath, CreateOutputFile);
        }

        /// <summary>
        ///     Reads the model from file.
        /// </summary>
        static EnvironmentVariableFileModel ReadModelFromFile()
        {
            return SerializeHelper.JsonToObject<EnvironmentVariableFileModel>(ReadAllText(OutputFilePath));
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