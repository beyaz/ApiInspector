using System;
using System.Collections.Generic;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The response
    /// </summary>
    [Serializable]
    public class Response
    {
        #region Fields
        /// <summary>
        ///     The results
        /// </summary>
        readonly List<Result> results = new List<Result>();
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets a value indicating whether this instance is fail.
        /// </summary>
        public bool IsFail => results.Count > 0;

        /// <summary>
        ///     Gets a value indicating whether this instance is success.
        /// </summary>
        public bool IsSuccess => results.Count == 0;

        /// <summary>
        ///     Gets the results.
        /// </summary>
        public IReadOnlyList<Result> Results => results;
        #endregion

        #region Public Methods
        /// <summary>
        ///     Adds the error.
        /// </summary>
        public void AddError(string errorMessage)
        {
            results.Add(errorMessage);
        }

        /// <summary>
        ///     Adds the error.
        /// </summary>
        public void AddError(Exception exception)
        {
            results.Add(exception);
        }
        #endregion
    }
}