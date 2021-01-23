using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    static partial class Mixin
    {
        #region Public Methods
        public static DataKey<T> CreateKey<T>(Type locatedType, [CallerMemberName] string callerMemberName = null)
        {
            return new DataKey<T>(locatedType, callerMemberName);
        }

        public static InvocationInfo CreateNewInvocationInfo()
        {
            return new InvocationInfo
            {
                Scenarios = new List<ScenarioInfo>()
            };
        }

        public static ScenarioInfo CreateNewScenarioInfo()
        {
            return new ScenarioInfo
            {
                Assertions       = new List<AssertionInfo>(),
                MethodParameters = new List<InvocationMethodParameterInfo>()
            };
        }

        public static bool IsEndOfDayMethod(InvocationInfo invocationInfo)
        {
            return invocationInfo.MethodName == EndOfDay.MethodAccessText;
        }
        #endregion
    }
}