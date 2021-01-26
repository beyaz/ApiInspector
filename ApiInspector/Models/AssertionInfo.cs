using System;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The assertion information
    /// </summary>
    [Serializable]
    public sealed class AssertionInfo
    {
        #region Constructors
        // ReSharper disable once EmptyConstructor
        public AssertionInfo()
        {
        }
        #endregion

        #region Public Properties
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