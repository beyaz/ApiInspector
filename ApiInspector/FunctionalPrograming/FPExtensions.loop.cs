using System;
using System.Collections.Generic;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The extensions
    /// </summary>
    public static partial class FPExtensions
    {
        #region Public Methods
        /// <summary>
        ///     Fors the each.
        /// </summary>
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

            foreach (var element in source)
            {
                action(element);
            }
        }

        /// <summary>
        ///     Fors the each.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<int, T> action)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var i = 0;
            foreach (var item in sequence)
            {
                action(i, item);
                i++;
            }
        }
        #endregion
    }
}