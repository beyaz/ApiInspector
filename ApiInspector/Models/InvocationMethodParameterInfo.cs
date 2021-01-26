using System;

namespace ApiInspector.Models
{
    /// <summary>
    ///     The invocation method parameter information
    /// </summary>
    [Serializable]
    public sealed class InvocationMethodParameterInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
        #endregion
    }
}