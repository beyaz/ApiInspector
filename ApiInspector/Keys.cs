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
        

        public static DataKey<MethodDefinition> SelectedMethodDefinition => CreateKey<MethodDefinition>();

        /// <summary>
        ///     Gets the history items.
        /// </summary>
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryItems => CreateKey<IReadOnlyList<InvocationInfo>>();

        /// <summary>
        ///     Gets the selected invocation information.
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfo => CreateKey<InvocationInfo>();

        
        

        public static DataKey<ScenarioInfo> SelectedScenario => CreateKey<ScenarioInfo>();

        public static DataKey<AssertionInfo> SelectedAssertion => CreateKey<AssertionInfo>();


        
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
            return new DataKey<T>(typeof(Keys), callerMemberName);
        }
        #endregion
    }

   

    
    
}