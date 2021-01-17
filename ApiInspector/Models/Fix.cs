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
                invocationInfo.Scenarios              = new List<Scenario>();

                return invocationInfo;
            }

            if (invocationInfo.Parameters==null || invocationInfo.Parameters.Count == 0)
            {
                invocationInfo.Scenarios = new List<Scenario>
                {
                    new Scenario
                    {
                        MethodParameters       = new List<InvocationMethodParameterInfo>(),
                        ResponseOutputFilePath = invocationInfo.ResponseOutputFilePath,
                        Assertions             = new List<Assertion>()
                    }
                };
            }
            else
            {
                invocationInfo.Scenarios = new List<Scenario>
                {
                    new Scenario
                    {
                        MethodParameters       = invocationInfo.Parameters,
                        ResponseOutputFilePath = invocationInfo.ResponseOutputFilePath,
                        Assertions             = new List<Assertion>()
                    }
                };
            }

            invocationInfo.Parameters             = null;
            invocationInfo.ResponseOutputFilePath = null;

            

            return invocationInfo;
        }
    }
}