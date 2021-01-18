using System;
using System.Collections.Generic;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The assertion information
    /// </summary>
    [Serializable]
    public class AssertionInfo
    {
        #region Constructors
        #endregion

        #region Public Properties
        public AssertionInfo()
        {
            
        }
        /// <summary>
        ///     Gets or sets the actual.
        /// </summary>
        public ValueAccessInfo Actual { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the expected.
        /// </summary>
        public ValueAccessInfo Expected { get; set; }

        /// <summary>
        ///     Gets or sets the name of the operator.
        /// </summary>
        public string OperatorName { get; set; }
        #endregion
    }

    /// <summary>
    ///     The assertion operator names
    /// </summary>
    class AssertionOperatorNames
    {


        public  const string IsEquals = "=";

        #region Public Methods
        /// <summary>
        ///     Gets the descriptions.
        /// </summary>
        public static IReadOnlyList<string> GetDescriptions()
        {
            return new[]
            {
                IsEquals,
                "!=",
                ">",
                ">=",
                "<",
                "<=",
                "Contains",
                "StartsWith",
                "EndsWith"
            };
        }
        #endregion
    }
}