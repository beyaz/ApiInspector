using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The common application keys
    /// </summary>
    static class CommonApplicationKeys
    {
        #region Public Properties
        /// <summary>
        ///     The assembly search directories
        /// </summary>
        public static DataKey<string> AssemblyPath => CreateKey<string>();

        /// <summary>
        ///     Gets the assembly search directories.
        /// </summary>
        public static DataKey<IReadOnlyList<string>> AssemblySearchDirectories => CreateKey<IReadOnlyList<string>>();

        /// <summary>
        ///     Gets or sets the invocation search directory.
        /// </summary>
        public static DataKey<string> InvocationSearchDirectory => CreateKey<string>();

        /// <summary>
        ///     The trace
        /// </summary>
        public static DataKey<Action<string>> Trace => CreateKey<Action<string>>();
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the key.
        /// </summary>
        static DataKey<T> CreateKey<T>([CallerMemberName] string callerMemberName = null)
        {
            return new DataKey<T>(typeof(CommonApplicationKeys), callerMemberName);
        }
        #endregion
    }
}