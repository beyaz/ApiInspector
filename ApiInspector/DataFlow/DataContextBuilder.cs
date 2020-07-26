using System.Collections.Generic;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;

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
                
            };

            ItemSourceListKey[context] = itemSourceList;

            
            return context;
        }
        #endregion
    }
}