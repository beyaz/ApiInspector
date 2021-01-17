using System.Collections.Generic;

namespace ApiInspector.Models
{
    static class Fix
    {
        public static InvocationInfo FixAsScenarioModel(InvocationInfo invocationInfo)
        {
            if (invocationInfo.MethodName == EndOfDay.MethodAccessText)
            {
                invocationInfo.Parameters             = null;
                invocationInfo.ResponseOutputFilePath = null;
                invocationInfo.Scenarios              = new List<ScenarioInfo>();

                return invocationInfo;
            }

            if (invocationInfo.Parameters==null || invocationInfo.Parameters.Count == 0)
            {
                invocationInfo.Scenarios = new List<ScenarioInfo>
                {
                    new ScenarioInfo
                    {
                        MethodParameters       = new List<InvocationMethodParameterInfo>(),
                        ResponseOutputFilePath = invocationInfo.ResponseOutputFilePath,
                        Assertions             = new List<AssertionInfo>()
                    }
                };
            }
            else
            {
                invocationInfo.Scenarios = new List<ScenarioInfo>
                {
                    new ScenarioInfo
                    {
                        MethodParameters       = invocationInfo.Parameters,
                        ResponseOutputFilePath = invocationInfo.ResponseOutputFilePath,
                        Assertions             = new List<AssertionInfo>()
                    }
                };
            }

            invocationInfo.Parameters             = null;
            invocationInfo.ResponseOutputFilePath = null;

            

            return invocationInfo;
        }
    }
}