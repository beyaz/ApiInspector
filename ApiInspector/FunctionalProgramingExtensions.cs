using System;
using System.Diagnostics.Contracts;

namespace FunctionalPrograming
{
    public static class Extensions
    {
        [Pure]
        public static Func<R> fun<R>(Func<R> f) => f;

        [Pure]
        public static Func<T1, R> fun<T1, R>(Func<T1, R> f) => f;

        [Pure]
        public static Func<T1, T2, R> fun<T1, T2, R>(Func<T1, T2, R> f) => f;

        [Pure]
        public static Func<T1, T2, T3, R> fun<T1, T2, T3, R>(Func<T1, T2, T3, R> f) => f;

        [Pure]
        public static Func<T1, T2, T3, T4, R> fun<T1, T2, T3, T4, R>(Func<T1, T2, T3, T4, R> f) => f;

        [Pure]
        public static Func<T1, T2, T3, T4, T5, R> fun<T1, T2, T3, T4, T5, R>(Func<T1, T2, T3, T4, T5, R> f) => f;

        [Pure]
        public static Func<T1, T2, T3, T4, T5, T6, R> fun<T1, T2, T3, T4, T5, T6, R>(Func<T1, T2, T3, T4, T5, T6, R> f) => f;

        [Pure]
        public static Func<T1, T2, T3, T4, T5, T6, T7, R> fun<T1, T2, T3, T4, T5, T6, T7, R>(Func<T1, T2, T3, T4, T5, T6, T7, R> f) => f;
    }
}