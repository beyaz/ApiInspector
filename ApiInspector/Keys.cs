using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using ApiInspector.Models;

namespace ApiInspector
{
    /// <summary>
    ///     The common application keys
    /// </summary>
    static class Keys
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
        ///     Gets the database connection.
        /// </summary>
        public static DataKey<IDbConnection> DbConnection => CreateKey<IDbConnection>();

        /// <summary>
        ///     Gets or sets the invocation search directory.
        /// </summary>
        public static DataKey<string> InvocationSearchDirectory => CreateKey<string>();

        /// <summary>
        ///     Gets the selected invocation information.
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfo => CreateKey<InvocationInfo>();

        /// <summary>
        ///     The trace
        /// </summary>
        public static DataKey<Action<string>> Trace => CreateKey<Action<string>>();

        public static DataKey<Action<string>> UserVisibleTrace => CreateKey<Action<string>>();

        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        public static DataKey<string> UserName => CreateKey<string>();


        public static DataKey<Func<object,string>> SerializeHistoryForDatabaseInsert => CreateKey<Func<object,string>>();

        #endregion

        #region Methods
        /// <summary>
        ///     Creates the key.
        /// </summary>
        static DataKey<T> CreateKey<T>([CallerMemberName] string callerMemberName = null)
        {
            return new DataKey<T>(typeof(Keys), callerMemberName);
        }
        #endregion
    }
}