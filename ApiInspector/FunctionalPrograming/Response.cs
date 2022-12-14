using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionalPrograming
{



    /// <summary>
    ///     The error
    /// </summary>
    [Serializable]
    public sealed class Error
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Performs an implicit conversion from <see cref="Exception" /> to <see cref="Error" />.
        /// </summary>
        public static implicit operator Error(Exception exception)
        {
            return new Error
            {
                Message = exception.ToString()
            };
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="Error" />.
        /// </summary>
        public static implicit operator Error(string errorMessage)
        {
            return new Error
            {
                Message = errorMessage
            };
        }
        #endregion
    }

    /// <summary>
    ///     The response
    /// </summary>
    [Serializable]
    public class Response
    {
        #region Fields
        /// <summary>
        ///     The errors
        /// </summary>
        protected readonly List<Error> errors = new List<Error>();
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the results.
        /// </summary>
        public IReadOnlyList<Error> Errors => errors;

        /// <summary>
        ///     Gets the fail message.
        /// </summary>
        public string FailMessage => string.Join(Environment.NewLine, errors.Select(e => e.Message));

        /// <summary>
        ///     Gets a value indicating whether this instance is fail.
        /// </summary>
        public bool IsFail => errors.Count > 0;

        /// <summary>
        ///     Gets a value indicating whether this instance is success.
        /// </summary>
        public bool IsSuccess => errors.Count == 0;

        public static readonly Response Success = new Response();
        #endregion

        #region Public Methods
        /// <summary>
        ///     Performs an implicit conversion from <see cref="Exception" /> to <see cref="Response" />.
        /// </summary>
        public static implicit operator Response(Exception exception)
        {
            var response = new Response();

            response.errors.Add(exception);

            return response;
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Error" /> to <see cref="Response" />.
        /// </summary>
        public static implicit operator Response(Error error)
        {
            var response = new Response();

            response.errors.Add(error);

            return response;
        }
        #endregion

        public static Response Fail(string errorMessage)
        {
            return new Response
            {
                errors = {errorMessage}
            };
        }
    }

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

            response.errors.Add(exception);

            return response;
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Error" /> to <see cref="Response{TValue}" />.
        /// </summary>
        public static implicit operator Response<TValue>(Error error)
        {
            var response = new Response<TValue>();

            response.errors.Add(error);

            return response;
        }

        public static implicit operator Response<TValue>(Error[] errors)
        {
            var response = new Response<TValue>();

            response.errors.AddRange(errors);

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