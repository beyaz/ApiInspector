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
        public static T SafeRun<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                return default(T);
            }
        }
        
        public static T IfNull<T>(this T instance, Func<T> create)
        {
            if (instance != null)
            {
                return instance;
            }

            return create();
        }
        

        
        [Pure]
        public static IReadOnlyList<T> AddNewOneItemIfListIsEmpty<T>(this IReadOnlyList<T> items, Func<T> newItem)
        {
            if (items.Count > 0)
            {
                return items;
            }

            return new List<T> {newItem()};
        }

        #endregion
    }
}