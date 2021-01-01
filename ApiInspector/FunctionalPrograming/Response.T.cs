using System;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The response
    /// </summary>
    [Serializable]
    public sealed class Response<TValue> : Response
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public TValue Value { get; set; }
        #endregion
    }
}