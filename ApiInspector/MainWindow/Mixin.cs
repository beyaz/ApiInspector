using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public static DataKey<T> CreateKey<T>(Type locatedType,[CallerMemberName] string callerMemberName = null)
        {
            return new DataKey<T>(locatedType, callerMemberName);
        }
    }

    sealed class AssertionExecuteResponseInfo
    {
        public readonly AssertionInfo AssertionInfo;

        public AssertionExecuteResponseInfo(AssertionInfo assertionInfo)
        {
            AssertionInfo = assertionInfo;
        }

        public string ErrorMessage { get; set; }

        public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);
    }

    static class AssertionExecuteResponseInfoExtension
    {
        public static string OnAssertionResponseUpdated = nameof(OnAssertionResponseUpdated);

        static DataKey<List<AssertionExecuteResponseInfo>> AssertionExecuteResponseList => Mixin.CreateKey<List<AssertionExecuteResponseInfo>>(typeof(AssertionExecuteResponseInfo));

        public static void UpdateAssertionExecuteResponse(this Scope scope, AssertionExecuteResponseInfo value)
        {
            var items = scope.GetItems();
            if (items.All(x => x.AssertionInfo != value.AssertionInfo))
            {
                items.Add(value);
            }

            var record = items.First(x => x.AssertionInfo == value.AssertionInfo);

            record.ErrorMessage = value.ErrorMessage;

            scope.PublishEvent(OnAssertionResponseUpdated);
        }

        static List<AssertionExecuteResponseInfo> GetItems(this Scope scope)
        {
            if (!scope.Contains(AssertionExecuteResponseList))
            {
                scope.Add(AssertionExecuteResponseList,new List<AssertionExecuteResponseInfo>());
            }

                return scope.Get(AssertionExecuteResponseList);
            
        }

        public static AssertionExecuteResponseInfo TryGetAssertionExecuteResponse(this Scope scope, AssertionInfo value)
        {
            return scope.GetItems().FirstOrDefault(x=>x.AssertionInfo == value);
        }
    }


    
}