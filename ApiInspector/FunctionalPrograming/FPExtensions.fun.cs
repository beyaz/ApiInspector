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
        public static Func<R> Fun<R>(Func<R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, R> Fun<T1, R>(Func<T1, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, R> Fun<T1, T2, R>(Func<T1, T2, R> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Func<T1, T2, T3, R> Fun<T1, T2, T3, R>(Func<T1, T2, T3, R> f) => f;
        #endregion

        #region fun actions
        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action<T> Fun<T>(Action<T> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action Fun(Action f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action<T1, T2> Fun<T1, T2>(Action<T1, T2> f) => f;

        /// <summary>
        ///     Funs the specified f.
        /// </summary>
        [Pure]
        public static Action<T1, T2, T3> Fun<T1, T2, T3>(Action<T1, T2, T3> f) => f;
        #endregion
    }
}