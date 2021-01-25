using System.Collections.Generic;
using System.Linq;

namespace ApiInspector.Models
{
    static class Fix
    {
        static InvocationInfo ReOrderAssertions(InvocationInfo invocationInfo)
        {
            foreach (var scenario in invocationInfo.Scenarios)
            {
                if (scenario.Assertions.Any(x=>x.OperatorName == AssertionOperatorNames.AssignTo))
                {
                    scenario.Assertions = scenario.Assertions.OrderByDescending(x => x.OperatorName == AssertionOperatorNames.AssignTo).ToList();    
                }
                
            }

            return invocationInfo;
        }
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
                if (invocationInfo.Scenarios == null)
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

            

            return ReOrderAssertions(invocationInfo);
        }
    }
}