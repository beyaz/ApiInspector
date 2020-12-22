using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The extensions
    /// </summary>
    public static class Extensions
    {
        #region Public Methods
        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<R> fun<R>(Func<R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, R> fun<T1, R>(Func<T1, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, R> fun<T1, T2, R>(Func<T1, T2, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, T3, R> fun<T1, T2, T3, R>(Func<T1, T2, T3, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, T3, T4, R> fun<T1, T2, T3, T4, R>(Func<T1, T2, T3, T4, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, T3, T4, T5, R> fun<T1, T2, T3, T4, T5, R>(Func<T1, T2, T3, T4, T5, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, T3, T4, T5, T6, R> fun<T1, T2, T3, T4, T5, T6, R>(Func<T1, T2, T3, T4, T5, T6, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, T3, T4, T5, T6, T7, R> fun<T1, T2, T3, T4, T5, T6, T7, R>(Func<T1, T2, T3, T4, T5, T6, T7, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action<T> fun<T>(Action<T> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action fun(Action f) => f;
        #endregion

        [Pure]
        public static TOut Map<Tin, TOut>(Tin input, Func<Tin, TOut> func) => func(input);

        [Pure]
        public static Response<T> Run<T>(Func<T> action)
        {
            var returnObject = new Response<T>();

            try
            {
                returnObject.Value = action();
            }
            catch (Exception exception)
            {
                returnObject.AddError(exception);
            }

            return returnObject;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (T element in source)
            {
                action(element);
            }
        }

        
        public static void Add<T>(List<T> list, params T[] items)
        {
            list.AddRange(items);
        }
    }

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