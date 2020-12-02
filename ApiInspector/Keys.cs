using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using ApiInspector.Models;
using ApiInspector.Tracing;
using Mono.Cecil;

namespace ApiInspector
{
    /// <summary>
    ///     The common application keys
    /// </summary>
    static class Keys
    {
        #region Public Properties
        /// <summary>
        ///     Gets the assembly name suggestions.
        /// </summary>
        public static DataKey<IReadOnlyList<string>> AssemblyNameSuggestions => CreateKey<IReadOnlyList<string>>();

        /// <summary>
        ///     The assembly search directories
        /// </summary>
        public static DataKey<string> AssemblyPath => CreateKey<string>();

        /// <summary>
        ///     Gets the assembly search directories.
        /// </summary>
        public static DataKey<IReadOnlyList<string>> AssemblySearchDirectories => CreateKey<IReadOnlyList<string>>();

        /// <summary>
        ///     Gets the class name suggestions.
        /// </summary>
        public static DataKey<IReadOnlyList<string>> ClassNameSuggestions => CreateKey<IReadOnlyList<string>>();

        /// <summary>
        ///     Gets the database connection.
        /// </summary>
        public static DataKey<IDbConnection> DbConnection => CreateKey<IDbConnection>();

        /// <summary>
        ///     Gets the error monitor.
        /// </summary>
        public static DataKey<ErrorMonitor> ErrorMonitor => CreateKey<ErrorMonitor>();

        /// <summary>
        ///     Gets the name of the get assembly file.
        /// </summary>
        public static DataKey<Func<string>> GetAssemblyFileName => CreateKey<Func<string>>();

        /// <summary>
        ///     Gets the get assembly search directory.
        /// </summary>
        public static DataKey<Func<string>> GetAssemblySearchDirectory => CreateKey<Func<string>>();

        /// <summary>
        ///     Gets the name of the get class.
        /// </summary>
        public static DataKey<Func<string>> GetClassName => CreateKey<Func<string>>();

        /// <summary>
        ///     Gets the history items.
        /// </summary>
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryItems => CreateKey<IReadOnlyList<InvocationInfo>>();

        /// <summary>
        ///     Gets or sets the invocation search directory.
        /// </summary>
        public static DataKey<string> InvocationSearchDirectory => CreateKey<string>();

        /// <summary>
        ///     Gets the method name suggestions.
        /// </summary>
        public static DataKey<IReadOnlyList<string>> MethodNameSuggestions => CreateKey<IReadOnlyList<string>>();

        /// <summary>
        ///     Gets the selected invocation information.
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfo => CreateKey<InvocationInfo>();

        /// <summary>
        ///     Gets the selected method definition.
        /// </summary>
        public static DataKey<MethodDefinition> SelectedMethodDefinition => CreateKey<MethodDefinition>();

        /// <summary>
        ///     Gets the selected type definition.
        /// </summary>
        public static DataKey<TypeDefinition> SelectedTypeDefinition => CreateKey<TypeDefinition>();

        /// <summary>
        ///     Gets the serialize history for database insert.
        /// </summary>
        public static DataKey<Func<object, string>> SerializeHistoryForDatabaseInsert => CreateKey<Func<object, string>>();

        /// <summary>
        ///     The trace
        /// </summary>
        public static DataKey<Action<string>> Trace => CreateKey<Action<string>>();

        /// <summary>
        ///     Gets the trace queue.
        /// </summary>
        public static DataKey<TraceQueue> TraceQueue => CreateKey<TraceQueue>();

        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        public static DataKey<string> UserName => CreateKey<string>();

        /// <summary>
        ///     Gets the user visible trace.
        /// </summary>
        public static DataKey<Action<string>> UserVisibleTrace => CreateKey<Action<string>>();
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