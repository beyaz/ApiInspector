using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    static class Mixin
    {
        public static ScenarioInfo CreateNewScenarioInfo()
        {
            return new ScenarioInfo
            {
                Assertions = new List<AssertionInfo>(), 
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
      
    }
}
