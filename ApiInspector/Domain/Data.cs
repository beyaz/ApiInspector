using ApiInspector.Models;
using BOA.Base.Data;
using BOA.DataFlow;

namespace ApiInspector.Domain
{
    class Data
    {
        public static  DataKey<InvocationInfo>       InvocationInfo       = new DataKey<InvocationInfo>(nameof(InvocationInfo));
        public static  DataKey<ExecutionDataContext> ExecutionDataContext = new DataKey<ExecutionDataContext>(nameof(ExecutionDataContext));
        public static  DataKey<object>               ExecutionResponse    = new DataKey<object>(nameof(ExecutionResponse));
        public static  DataKey<string>               TargetEnvironment    = new DataKey<string>(nameof(TargetEnvironment));
    }
}