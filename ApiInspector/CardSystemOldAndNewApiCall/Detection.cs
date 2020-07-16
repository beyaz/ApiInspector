using System.Linq;
using ApiInspector.Models;
using BOA.DataFlow;
using Mono.Cecil;

namespace ApiInspector.CardSystemOldAndNewApiCall
{
    class Detection
    {
        #region Static Fields
        public static DataKey<InvocationInfo>   InvocationInfo   = new DataKey<InvocationInfo>(nameof(InvocationInfo));
        public static DataKey<MethodDefinition> MethodDefinition = new DataKey<MethodDefinition>(nameof(MethodDefinition));
        #endregion

        #region Public Methods
        public static bool CanInvokeAsCardSystemOldAndNewApiCall(DataContext context)
        {
            var invocationInfo = context.Get(InvocationInfo);

            if (invocationInfo.AssemblyName == "BOA.Process.Kernel.Card.dll")
            {
                if (invocationInfo.MethodName == "ExecuteInOldCardSystem")
                {
                    return context.Get(MethodDefinition).DeclaringType.Methods.Any(m => m.Name == "ExecuteInNewCardSystem");
                }

                if (invocationInfo.MethodName == "ExecuteInNewCardSystem")
                {
                    return context.Get(MethodDefinition).DeclaringType.Methods.Any(m => m.Name == "ExecuteInOldCardSystem");
                }
            }

            return false;
        }
        #endregion
    }
}