using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using static System.String;
using static ApiInspector.Keys;
using static ApiInspector.Utility;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    partial class ScenarioEditor
    {
        public Action<string> ShowErrorNotification;

        #region Public Methods
        public void ExecuteAllScenarioList()
        {
            foreach (var scenario in scenarios)
            {
                scope.Update(SelectedScenario, scenario);

                ExecuteSelectedScenario();
            }
        }
        #endregion

        #region Methods
        void ExecuteSelectedScenario()
        {
            var scenario = scope.Get(SelectedScenario);

            var invocationInfo  = InvocationInfo;
            var environmentInfo = EnvironmentInfo.Parse(invocationInfo.Environment);

            void UpdateUI(Action action)
            {
                Dispatcher.InvokeAsync(action);
            }

            void OnEnteredToExecution()
            {
                UpdateUI(() => executeSelectedScenarioButton.Text      = "Executing...");
                UpdateUI(() => executeSelectedScenarioButton.IsEnabled = false);
            }

            void OnExitToExecution()
            {
                UpdateUI(() => executeSelectedScenarioButton.Text      = "Execute");
                UpdateUI(() => executeSelectedScenarioButton.IsEnabled = true);
            }

            void trace(string message)
            {
                scope.Get(Trace)(message);
            }

            OnEnteredToExecution();

            var scenarioIndex = scenarios.IndexOf(scenario);

            trace($"------------- EXECUTE STARTED For {scenarioIndex + 1} -------------");

            var invokeOutput = Invoker.Invoke(environmentInfo, trace, invocationInfo, scenarioIndex);

            UpdateScenarioOutput(scenarioIndex, invokeOutput);

            if (!IsNullOrWhiteSpace(scenario.ResponseOutputFilePath))
            {
                WriteToFile(scenario.ResponseOutputFilePath, invokeOutput.ExecutionResponseAsJson);
            }

            UpdateUI(() => { UpdateOutput?.Invoke(); });

            

            var runAssertions = fun(() =>
            {
                var methodDefinition = scope.Get(SelectedMethodDefinition);

                var runAssertion = fun((AssertionInfo assertionInfo) =>
                {
                    var actual = AssertionValueCalculator.CalculateFrom(assertionInfo.Actual,methodDefinition,invokeOutput);
                    var expected = AssertionValueCalculator.CalculateFrom(assertionInfo.Expected,methodDefinition,invokeOutput);

                    var errorMessage = AssertionValueCalculator.RunAssertion(actual, expected, assertionInfo.OperatorName);
                    
                    UpdateUI(() =>
                    {
                        var assertionsEditor = ActivateAssertions();

                        assertionsEditor.Loaded += (s, e) =>
                        {
                            assertionsEditor.scope.Update(AssertionErrorMap, new KeyValuePair<AssertionInfo, string>(assertionInfo, errorMessage));
                            assertionsEditor.selectedAssertion = assertionInfo;
                        };

                    });

                    if (errorMessage != null)
                    {

                        

                        return false;
                    }

                    return true;
                });

                foreach (var assertion in scenario.Assertions)
                {
                    var isSuccess = runAssertion(assertion);
                    if (!isSuccess)
                    {
                        return;
                    }
                }
            });

            runAssertions();

            trace(Empty);
            trace(Empty);
            trace($"------------- EXECUTE FINISHED {scenarioIndex + 1} -------------");

            OnExitToExecution();
        }

        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (!HasInvocationInfo || IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                ShowErrorNotification("MethodName can not be empty.");
                return;
            }

            Task.Run(() => scope.PublishEvent(HistoryEvent.SaveToHistory));
            Task.Run(() => ExecuteSelectedScenario());
        }

        void UpdateScenarioOutput(int scenarioIndex, InvokeOutput invokeOutput)
        {
            if (!scope.Contains(InvokeOutputs))
            {
                scope.Add(InvokeOutputs, new InvokeOutput[scenarios.Count]);
            }

            scope.Get(InvokeOutputs)[scenarioIndex] = invokeOutput;
        }
        #endregion
    }
}