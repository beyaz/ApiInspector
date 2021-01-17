using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FunctionalPrograming
{
    /// <summary>
    ///     The extensions
    /// </summary>
    public static partial class FPExtensions
    {
        #region Public Methods
        /// <summary>
        ///     Adds the specified list.
        /// </summary>
        public static void Add<T>(List<T> list, params T[] items)
        {
            list.AddRange(items);
        }

        /// <summary>
        ///     Adds the or update.
        /// </summary>
        public static void AddOrUpdate<K, V>(this Dictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }

            dictionary.Add(key, value);
        }

        /// <summary>
        ///     Maps the specified input.
        /// </summary>
        [Pure]
        public static TOut Map<Tin, TOut>(Tin input, Func<Tin, TOut> func) => func(input);

        

        /// <summary>
        ///     Runs the specified action.
        /// </summary>
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
        #endregion
    }
}