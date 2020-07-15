using ApiInspector.Models;
using BOA.Base.Data;
using BOA.DataFlow;

namespace ApiInspector.Domain
{
    class Data
    {
        #region Static Fields
        public static DataKey<ExecutionDataContext> BOAExecutionContext = new DataKey<ExecutionDataContext>(nameof(BOAExecutionContext));
        public static DataKey<object>               ExecutionResponse    = new DataKey<object>(nameof(ExecutionResponse));
        public static DataKey<InvocationInfo>       InvocationInfo       = new DataKey<InvocationInfo>(nameof(InvocationInfo));
        #endregion
    }
}