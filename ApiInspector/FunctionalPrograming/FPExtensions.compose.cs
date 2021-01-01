using System;
using System.Diagnostics.Contracts;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The extensions
    /// </summary>
    partial class FPExtensions
    {
        #region composition
        /// <summary>
        ///     Function composition
        /// </summary>
        [Pure]
        public static Func<A, C> Compose<A, B, C>(this Func<A, B> f, Func<B, C> g) => v => g(f(v));

        /// <summary>
        ///     Function composition
        /// </summary>
        [Pure]
        public static Func<B> Compose<A, B>(this Func<A> f, Func<A, B> g) => () => g(f());

        /// <summary>
        ///     Action composition
        /// </summary>
        [Pure]
        public static Action<A> Compose<A, B>(this Func<A, B> f, Action<B> g) => v => g(f(v));

        /// <summary>
        ///     Action composition
        /// </summary>
        [Pure]
        public static Action Compose<A>(this Func<A> f, Action<A> g) => () => g(f());
        #endregion
    }
}