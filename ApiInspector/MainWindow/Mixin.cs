using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    static class Mixin
    {
        public static ScenarioInfo CreateNewScenarioInfo()
        {
            return new ScenarioInfo
            {
                Assertions       = new List<AssertionInfo>(),
                MethodParameters = new List<InvocationMethodParameterInfo>()
            };
        }

        public static InvocationInfo CreateNewInvocationInfo()
        {
            return new InvocationInfo
            {
                Scenarios = new List<ScenarioInfo>()
            };
        }

        public static bool IsEndOfDayMethod(InvocationInfo invocationInfo)
        {
            return invocationInfo.MethodName == EndOfDay.MethodAccessText;
        }

        // TODO: check usage
        public static void UpdateAssertionResponseAsSuccess(Scope scope, AssertionInfo assertionInfo)
        {
            assertionInfo.lastExecutionErrorMessage = null;
            assertionInfo.lastExecutionIsSuccess    = true;
        }
        public static void UpdateAssertionResponseAsFail(Scope scope, AssertionInfo assertionInfo,string errorMessage)
        {
            assertionInfo.lastExecutionErrorMessage = errorMessage;
            assertionInfo.lastExecutionIsSuccess    = false;
        }
        public static void ClearAssertionResponse(Scope scope, AssertionInfo assertionInfo)
        {
            assertionInfo.lastExecutionErrorMessage = null;
            assertionInfo.lastExecutionIsSuccess    = null;
        }
    }
}