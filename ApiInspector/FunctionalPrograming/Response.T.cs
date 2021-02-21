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

        #region Public Methods
        /// <summary>
        ///     Performs an implicit conversion from <see cref="Exception" /> to <see cref="Response{TValue}" />.
        /// </summary>
        public static implicit operator Response<TValue>(Exception exception)
        {
            var response = new Response<TValue>();

            response.AddError(exception);

            return response;
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="TValue" /> to <see cref="Response{TValue}" />.
        /// </summary>
        public static implicit operator Response<TValue>(TValue value)
        {
            return new Response<TValue> {Value = value};
        }
        #endregion
    }
}