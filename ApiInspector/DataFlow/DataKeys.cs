using System;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.DataFlow
{
    /// <summary>
    ///     The data keys
    /// </summary>
    class DataKeys
    {
        #region Static Fields
        /// <summary>
        ///     The main window view model key
        /// </summary>
        public static DataKey<MainWindowViewModel> MainWindowViewModelKey = new DataKey<MainWindowViewModel>(nameof(MainWindowViewModel));


        
        public static DataKey<ItemSourceList> ItemSourceListKey = new DataKey<ItemSourceList>(nameof(ItemSourceList));

        /// <summary>
        ///     The selected invocation information key
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfoKey = new DataKey<InvocationInfo>(nameof(InvocationInfo));



        public static DataKey<MethodDefinition> MethodDefinitionKey = new DataKey<MethodDefinition>(nameof(MethodDefinition));
        public static DataKey<TypeDefinition> TypeDefinitionKey = new DataKey<TypeDefinition>(nameof(TypeDefinition));
        
        #endregion
    }

    /// <summary>
    ///     The service keys
    /// </summary>
    static class ServiceKeys
    {
        #region Static Fields
        /// <summary>
        ///     The history service key
        /// </summary>
        public static DataKey<DataSource> HistoryServiceKey = new DataKey<DataSource>(nameof(History));

        /// <summary>
        ///     The trace key
        /// </summary>
        public static DataKey<Action<string>> TraceKey = new DataKey<Action<string>>(nameof(TraceKey));
        #endregion
    }
}