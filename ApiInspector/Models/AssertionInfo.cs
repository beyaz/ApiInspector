using System;

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
        // ReSharper disable once EmptyConstructor
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
}