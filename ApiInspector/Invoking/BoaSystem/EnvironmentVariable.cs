using System;
using System.IO;
using static System.IO.File;
using static System.String;
using static ApiInspector._;
using static ApiInspector.Serialization.Serializer;
using static ApiInspector.Utility;
using static BOA.Common.Helpers.SerializeHelper;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.Invoking.BoaSystem
{
    static class EnvironmentVariables
    {
        #region Static Fields
        public static readonly Func<string> GetUserName;
        public static readonly Func<bool> UseLocalProxyForCardServices;
        #endregion

        #region Constructors
        static EnvironmentVariables()
        {
            var createOutputFile = fun(() =>
            {
                var environmentVariableFileModel = new EnvironmentVariableFileModel
                {
                    UserName                     = Environment.UserName,
                    UseLocalProxyForCardServices = true
                };

                var fileContent = SerializeToJsonDoNotIgnoreDefaultValues(environmentVariableFileModel);

                WriteToFile(EnvironmentVariablesJsonFilePath, fileContent);
            });

            var ensureOutputFileExists = fun(() => { IfFileNotExistsThen(EnvironmentVariablesJsonFilePath, createOutputFile); });

            var readModelFromFile = fun(() => JsonToObject<EnvironmentVariableFileModel>(ReadAllText(EnvironmentVariablesJsonFilePath)));

            var useLocalProxyForCardServices = fun(() =>
            {
                ensureOutputFileExists();
                return readModelFromFile().UseLocalProxyForCardServices;
            });

            var getUserName = fun(() =>
            {
                ensureOutputFileExists();

                var model = readModelFromFile();

                if (IsNullOrWhiteSpace(model.UserName))
                {
                    return Environment.UserName;
                }

                return model.UserName.Trim();
            });

            GetUserName = getUserName;

            UseLocalProxyForCardServices = useLocalProxyForCardServices;
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