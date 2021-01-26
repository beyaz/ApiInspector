using System.Collections.Generic;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The assertion operator names
    /// </summary>
    static class AssertionOperatorNames
    {


        public  const string IsEqual = "Is Equal";
        public  const string IsNotEqual = "Is Not Equal";
        public  const string GreaterThan = "Greater Than";
        public  const string LessThan = "Less Than";
        public  const string LessThanOrEqual = "Less Than or Equal";
        public  const string GreaterThanOrEqual = "Greater Than or Equal";
        public  const string AssignTo = "Assign";
        public  const string AssignToVariable = "Assign to Variable";

        #region Public Methods
        /// <summary>
        ///     Gets the descriptions.
        /// </summary>
        public static IReadOnlyList<string> GetDescriptions()
        {
            return new[]
            {
                IsEqual,
                IsNotEqual,
                GreaterThan,
                GreaterThanOrEqual,
                LessThan,
                LessThanOrEqual,
                AssignTo,
                AssignToVariable
            };
        }
        #endregion
    }
}