using System;
using System.Collections.Generic;
using System.Linq;
using ApiInspector.Invoking;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
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

    sealed class ScenarioExecuteResponseInfo
    {
        public List<AssertionExecuteResponseInfo> AssertionExecuteResponses { set;get; }= new List<AssertionExecuteResponseInfo>();

        public ScenarioInfo Scenario { get; set; }

        public InvokeOutput InvokeOutput{ get; set; }

        public bool IsSuccess => InvokeOutput.IsSuccess && AssertionExecuteResponses.All(x => x.IsSuccess);

    }



    static partial class Mixin
    {
        public static string OnScenarioExecuteResponseUpdated = nameof(OnScenarioExecuteResponseUpdated);

        static DataKey<List<ScenarioExecuteResponseInfo>> ScenarioExecuteResponses => CreateKey<List<ScenarioExecuteResponseInfo>>(typeof(Mixin));
        
        public static void ClearScenarioExecuteResponse(this Scope scope,ScenarioInfo scenarioInfo)
        {
            var response = scope.TryGetScenarioExecuteResponse(scenarioInfo);
            if (response == null)
            {
                return;
            }

            scope.GetScenarioExecuteResponses().Remove(response);
        }

        public static ScenarioExecuteResponseInfo TryGetScenarioExecuteResponse(this Scope scope, ScenarioInfo value)
        {
            return scope.GetScenarioExecuteResponses().FirstOrDefault(x => x.Scenario == value);
        }
        static List<ScenarioExecuteResponseInfo> GetScenarioExecuteResponses(this Scope scope)
        {
            if (!scope.Contains(ScenarioExecuteResponses))
            {
                scope.Add(ScenarioExecuteResponses, new List<ScenarioExecuteResponseInfo>());
            }

            return scope.Get(ScenarioExecuteResponses);
        }
        
        public static void UpdateScenarioExecuteResponse(this Scope scope, ScenarioExecuteResponseInfo value)
        {
            var items = scope.GetScenarioExecuteResponses();
            if (items.All(x => x.Scenario != value.Scenario))
            {
                items.Add(value);
            }

            var record = items.First(x => x.Scenario == value.Scenario);

            record.InvokeOutput              = value.InvokeOutput;
            record.AssertionExecuteResponses = value.AssertionExecuteResponses;

            scope.PublishEvent(OnScenarioExecuteResponseUpdated);
        }



        public static AssertionExecuteResponseInfo TryGetAssertionExecuteResponse(this Scope scope, AssertionInfo assertionInfo)
        {
            foreach (var scenarioExecuteResponseInfo in scope.GetScenarioExecuteResponses())
            {
                foreach (var assertionExecuteResponseInfo in scenarioExecuteResponseInfo.AssertionExecuteResponses)
                {
                    if (assertionExecuteResponseInfo.AssertionInfo == assertionInfo)
                    {
                        return assertionExecuteResponseInfo;
                    }
                }
            }

            return null;
        }

        
    }
}