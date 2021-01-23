using ApiInspector.Invoking;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    static partial class Mixin
    {
        #region Static Fields
        public static string OnInvokeOutputChanged = nameof(OnInvokeOutputChanged);
        #endregion

        #region Properties
        static DataKey<InvokeOutput[]> InvokeOutputs => CreateKey<InvokeOutput[]>(typeof(Mixin));
        #endregion

        #region Public Methods
        public static void ClearScenarioOutputs(this Scope scope)
        {
            scope.TryRemove(InvokeOutputs);
            scope.PublishEvent(OnInvokeOutputChanged);
        }

        public static InvokeOutput FindScenarioOutput(this Scope scope, ScenarioInfo scenarioInfo)
        {
            var invokeOutputs = scope.TryGet(InvokeOutputs);

            if (invokeOutputs == null || invokeOutputs.Length == 0)
            {
                return null;
            }

            var scenarios = scope.Get(Keys.SelectedInvocationInfo).Scenarios;

            var scenarioIndex = scenarios.IndexOf(scenarioInfo);
            if (scenarioIndex >= 0 && invokeOutputs.Length > scenarioIndex)
            {
                return invokeOutputs[scenarioIndex];
            }

            return null;
        }

        public static void UpdateScenarioOutput(this Scope scope, ScenarioInfo scenarioInfo, InvokeOutput invokeOutput)
        {
            var scenarios = scope.Get(Keys.SelectedInvocationInfo).Scenarios;

            if (!scope.Contains(InvokeOutputs))
            {
                scope.Add(InvokeOutputs, new InvokeOutput[scenarios.Count]);
            }

            var scenarioIndex = scenarios.IndexOf(scenarioInfo);

            scope.Get(InvokeOutputs)[scenarioIndex] = invokeOutput;

            scope.PublishEvent(OnInvokeOutputChanged);
        }
        #endregion
    }
}