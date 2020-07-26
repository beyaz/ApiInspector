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
      

        
        public static DataKey<ItemSourceList> ItemSourceListKey = new DataKey<ItemSourceList>(nameof(ItemSourceList));

        /// <summary>
        ///     The selected invocation information key
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfoKey = new DataKey<InvocationInfo>(nameof(InvocationInfo));



        public static DataKey<MethodDefinition> MethodDefinitionKey = new DataKey<MethodDefinition>(nameof(MethodDefinition));
        public static DataKey<TypeDefinition> TypeDefinitionKey = new DataKey<TypeDefinition>(nameof(TypeDefinition));
        
        #endregion
    }
    
}