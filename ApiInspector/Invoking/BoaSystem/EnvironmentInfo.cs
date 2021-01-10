using System;

namespace ApiInspector.Invoking.BoaSystem
{
    /// <summary>
    ///     The environment information
    /// </summary>
    [Serializable]
    class EnvironmentInfo
    {
        #region Static Fields
        /// <summary>
        ///     The dev
        /// </summary>
        public static readonly EnvironmentInfo Dev = Parse("Dev");

        /// <summary>
        ///     The test
        /// </summary>
        public static readonly EnvironmentInfo Test = Parse("Test");

        /// <summary>
        ///     The prep
        /// </summary>
        public static readonly EnvironmentInfo Prep = Parse("Prep");
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnvironmentInfo" /> class.
        /// </summary>
        public EnvironmentInfo(bool isDev, bool isTest, bool isPrep)
        {
            IsDev  = isDev;
            IsTest = isTest;
            IsPrep = isPrep;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets a value indicating whether this instance is dev.
        /// </summary>
        public bool IsDev { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is prep.
        /// </summary>
        public bool IsPrep { get; }

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
                return new EnvironmentInfo(true, false, false);
            }

            if (targetEnvironment.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new EnvironmentInfo(false, true, false);
            }

            if (targetEnvironment.IndexOf("prep", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new EnvironmentInfo(false, false, true);
            }

            throw new NotImplementedException(nameof(targetEnvironment));
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            if (IsDev)
            {
                return "Dev";
            }

            if (IsTest)
            {
                return "Test";
            }

            if (IsPrep)
            {
                return "Prep";
            }

            throw new NotImplementedException();
        }
        #endregion
    }
}