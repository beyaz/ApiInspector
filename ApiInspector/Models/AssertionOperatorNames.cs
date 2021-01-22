using System.Collections.Generic;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The assertion operator names
    /// </summary>
    class AssertionOperatorNames
    {


        public  const string IsEquals = "=";
        public  const string IsNotEquals = "!=";
        public  const string GreaterThan = ">";
        public  const string LessThan = "<";
        public  const string LessThanOrEquals = "<=";
        public  const string GreaterThanOrEquals = ">=";

        #region Public Methods
        /// <summary>
        ///     Gets the descriptions.
        /// </summary>
        public static IReadOnlyList<string> GetDescriptions()
        {
            return new[]
            {
                IsEquals,
                IsNotEquals,
                GreaterThan,
                GreaterThanOrEquals,
                LessThan,
                LessThanOrEquals
            };
        }
        #endregion
    }
}