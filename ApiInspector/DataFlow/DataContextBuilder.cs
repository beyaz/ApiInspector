using System.Collections.Generic;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;
using static ApiInspector.DataFlow.ServiceKeys;

namespace ApiInspector.DataFlow
{
    class DataContextBuilder
    {
        #region Public Methods
        public DataContext Build()
        {
            var context    = new DataContext();
            var traceQueue = new TraceQueue();

            var itemSourceList = new ItemSourceList
            {
                AssemblySearchDirectoryList = new List<string> {AssemblySearchDirectories.serverBin, AssemblySearchDirectories.clientBin},
                EnvironmentNameList         = new List<string> {"dev", "test"}
            };

            HistoryServiceKey[context] = new DataSource();
            ItemSourceListKey[context] = itemSourceList;

            TraceQueueKey[context] = traceQueue;
            TraceKey[context]      = traceQueue.AddMessage;

            {
                ViewControllerKeys.TraceKey                  = TraceKey;
                ViewControllerKeys.ItemSourceListKey         = ItemSourceListKey;
                ViewControllerKeys.SelectedInvocationInfoKey = SelectedInvocationInfoKey;
                ViewControllerKeys.MethodDefinitionKey       = MethodDefinitionKey;
                ViewControllerKeys.TypeDefinitionKey         = TypeDefinitionKey;
            }

            return context;
        }
        #endregion
    }
}