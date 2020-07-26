using System;

namespace ApiInspector.TestData
{
    /// <summary>
    ///     Any input
    /// </summary>
    [Serializable]
    public class AnyInput
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the integer property.
        /// </summary>
        public int IntegerProperty { get; set; }

        /// <summary>
        ///     Gets or sets the string property.
        /// </summary>
        public string StringProperty { get; set; }
        #endregion
    }
}