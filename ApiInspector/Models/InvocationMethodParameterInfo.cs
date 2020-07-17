using System;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The invocation method parameter information
    /// </summary>
    [Serializable]
    public class InvocationMethodParameterInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public object Value { get; set; }
        #endregion
    }
}