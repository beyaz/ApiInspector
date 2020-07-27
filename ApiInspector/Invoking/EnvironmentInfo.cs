using System;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The environment information
    /// </summary>
    class EnvironmentInfo
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnvironmentInfo" /> class.
        /// </summary>
        public EnvironmentInfo(bool isDev, bool isTest)
        {
            IsDev  = isDev;
            IsTest = isTest;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets a value indicating whether this instance is dev.
        /// </summary>
        public bool IsDev { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is test.
        /// </summary>
        public bool IsTest { get; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Parses the specified target environment.
        /// </summary>
        public static EnvironmentInfo Parse(string targetEnvironment)
        {
            if (targetEnvironment.IndexOf("dev", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new EnvironmentInfo(true, false);
            }

            if (targetEnvironment.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new EnvironmentInfo(false, true);
            }

            throw new NotImplementedException(nameof(targetEnvironment));
        }
        #endregion
    }
}