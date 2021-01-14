using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using ApiInspector.Invoking;
using ApiInspector.Models;
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
        ///     Gets the database connection.
        /// </summary>
        public static DataKey<IDbConnection> DbConnection => CreateKey<IDbConnection>();

        public static DataKey<MethodDefinition> SelectedMethodDefinition => CreateKey<MethodDefinition>();

        /// <summary>
        ///     Gets the history items.
        /// </summary>
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryItems => CreateKey<IReadOnlyList<InvocationInfo>>();

        /// <summary>
        ///     Gets the selected invocation information.
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfo => CreateKey<InvocationInfo>();

        public static DataKey<Action<Scenario>> AddNewScenario => CreateKey<Action<Scenario>>();
        

        public static DataKey<Scenario> SelectedScenario => CreateKey<Scenario>();

        
        public static DataKey<List<InvokeOutput>> InvokeOutputs => CreateKey<List<InvokeOutput>>();

        /// <summary>
        ///     Gets the serialize history for database insert.
        /// </summary>
        public static DataKey<Func<object, string>> SerializeHistoryForDatabaseInsert => CreateKey<Func<object, string>>();

        /// <summary>
        ///     The trace
        /// </summary>
        public static DataKey<Action<string>> Trace => CreateKey<Action<string>>();

        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        public static DataKey<string> UserName => CreateKey<string>();
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

    enum ScenarioEvent
    {
        
        NewScenarioAdded,

        
        RemoveSelectedScenario,
        ExecutionFinished
    }
}