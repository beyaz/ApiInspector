using System;
using System.Collections.Generic;
using System.Linq;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    sealed class AssertionExecuteResponseInfo
    {
        #region Fields
        public readonly AssertionInfo AssertionInfo;
        #endregion

        #region Constructors
        public AssertionExecuteResponseInfo(AssertionInfo assertionInfo)
        {
            AssertionInfo = assertionInfo;
        }
        #endregion

        #region Public Properties
        public string ErrorMessage { get; set; }

        public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);
        #endregion
    }

    sealed class ScenarioExecuteResponseInfo
    {
        public List<AssertionExecuteResponseInfo> AssertionExecuteResponses { get; }= new List<AssertionExecuteResponseInfo>();

        public ScenarioInfo Scenario { get; set; }

        public string ErrorMessage { get; set; }
        
        public Exception Exception { get; set; }

    }



    static partial class Mixin
    {
        public static string OnScenarioExecuteResponseUpdated = nameof(OnScenarioExecuteResponseUpdated);

        static DataKey<List<ScenarioExecuteResponseInfo>> ScenarioExecuteResponses => CreateKey<List<ScenarioExecuteResponseInfo>>(typeof(Mixin));
        

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

            record.ErrorMessage = value.ErrorMessage;

            scope.PublishEvent(OnScenarioExecuteResponseUpdated);
        }



        #region Static Fields
        public static string OnAssertionResponseUpdated = nameof(OnAssertionResponseUpdated);
        #endregion

        #region Properties
        static DataKey<List<AssertionExecuteResponseInfo>> AssertionExecuteResponseList => CreateKey<List<AssertionExecuteResponseInfo>>(typeof(AssertionExecuteResponseInfo));
        #endregion

        #region Public Methods
        public static void ClearAssertionExecuteResponses(this Scope scope)
        {
            scope.TryRemove(AssertionExecuteResponseList);
        }

        public static AssertionExecuteResponseInfo TryGetAssertionExecuteResponse(this Scope scope, AssertionInfo value)
        {
            return scope.GetItems().FirstOrDefault(x => x.AssertionInfo == value);
        }

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
        #endregion

        #region Methods
        static List<AssertionExecuteResponseInfo> GetItems(this Scope scope)
        {
            if (!scope.Contains(AssertionExecuteResponseList))
            {
                scope.Add(AssertionExecuteResponseList, new List<AssertionExecuteResponseInfo>());
            }

            return scope.Get(AssertionExecuteResponseList);
        }
        #endregion
    }
}