using System;
using System.Diagnostics.Contracts;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The extensions
    /// </summary>
    partial class FPExtensions
    {
        #region fun
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
        #endregion

        #region fun actions
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

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action<T1, T2> fun<T1, T2>(Action<T1, T2> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action<T1, T2, T3> fun<T1, T2, T3>(Action<T1, T2, T3> f) => f;
        #endregion
    }
}