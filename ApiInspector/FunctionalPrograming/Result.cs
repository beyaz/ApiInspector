using System;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The result
    /// </summary>
    [Serializable]
    public sealed class Result
    {
        #region Public Properties
        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Performs an implicit conversion from <see cref="Exception" /> to <see cref="Result" />.
        /// </summary>
        public static implicit operator Result(Exception exception)
        {
            return new Result
            {
                ErrorMessage = exception.ToString()
            };
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="Result" />.
        /// </summary>
        public static implicit operator Result(string errorMessage)
        {
            return new Result
            {
                ErrorMessage = errorMessage
            };
        }
        #endregion
    }
}