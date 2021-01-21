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
        #region Fields
        public Action<string> ShowErrorNotification;
        #endregion

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
        bool ExecuteSelectedScenario()
        {
            void trace(string message)
            {
                scope.Get(Trace)(message);
            }

            trace("EXECUTE STARTED");

            scope.TryRemove(InvokeOutputs);

            var invocationInfo  = InvocationInfo;
            var environmentInfo = EnvironmentInfo.Parse(invocationInfo.Environment);

            if (invocationInfo.MethodName == EndOfDay.MethodAccessText)
            {
                var invokeOutput = Invoker.Invoke(environmentInfo, trace, invocationInfo, -1);
                if (!invokeOutput.IsSuccess)
                {
                    trace(invokeOutput.Error.ToString());
                    trace("EXECUTION IS FAILED.");
                    return false;
                }

                trace("EXECUTION IS SUCCESSFULL");
                return true;
            }

            {
                var scenario = scope.Get(SelectedScenario);

                var scenarioIndex = scenarios.IndexOf(scenario);

                var invokeOutput = Invoker.Invoke(environmentInfo, trace, invocationInfo, scenarioIndex);

                UpdateScenarioOutput(scenarioIndex, invokeOutput);

                if (!IsNullOrWhiteSpace(scenario.ResponseOutputFilePath))
                {
                    WriteToFile(scenario.ResponseOutputFilePath, invokeOutput.ExecutionResponseAsJson);
                }

                UpdateUI(() => { UpdateOutput?.Invoke(); });

                if (!invokeOutput.IsSuccess)
                {
                    trace("EXECUTION IS FAILED.");
                    return false;
                }

                bool runAssertions()
                {
                    var methodDefinition = scope.Get(SelectedMethodDefinition);

                    bool runAssertion(AssertionInfo assertionInfo)
                    {
                        var env = EnvironmentInfo.Parse(invocationInfo.Environment);

                        var actual       = AssertionValueCalculator.CalculateFrom(assertionInfo.Actual, methodDefinition, invokeOutput, env);
                        var expected     = AssertionValueCalculator.CalculateFrom(assertionInfo.Expected, methodDefinition, invokeOutput, env);
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
                    }

                    foreach (var assertion in scenario.Assertions)
                    {
                        var isSuccess = runAssertion(assertion);
                        if (!isSuccess)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                var isAssertionsExecutedSuccessfully = runAssertions();
                if (!isAssertionsExecutedSuccessfully)
                {
                    trace("EXECUTION IS SUCCESSFULL BUT ASSERTIONS ARE FAILED.");
                    return false;
                }

                trace("EXECUTION IS SUCCESSFULL");

                return true;
            }
        }

        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (!HasInvocationInfo || IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                ShowErrorNotification("MethodName can not be empty.");
                return;
            }

            UpdateScenarioActionIcon(success: null);

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

            Task.Run(() => scope.PublishEvent(HistoryEvent.SaveToHistory));
            Task.Run(() =>
            {
                try
                {
                    OnEnteredToExecution();

                    var isSuccess = ExecuteSelectedScenario();

                    UpdateScenarioActionIcon(isSuccess);
                }
                catch (Exception exception)
                {
                    UpdateScenarioActionIcon(success: false);

                    MessageBox.Show("Hata: " + exception);
                }
                finally
                {
                    OnExitToExecution();
                }
            });
        }

        void UpdateScenarioActionIcon(bool? success)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var actionButton = FindSelectedActionButton();

                If(success == null, actionButton.HideIcon);
                If(success == true, actionButton.ShowSuccessIcon);
                If(success == false, actionButton.ShowFailIcon);
            });
        }

        void UpdateScenarioOutput(int scenarioIndex, InvokeOutput invokeOutput)
        {
            if (!scope.Contains(InvokeOutputs))
            {
                scope.Add(InvokeOutputs, new InvokeOutput[scenarios.Count]);
            }

            scope.Get(InvokeOutputs)[scenarioIndex] = invokeOutput;
        }

        void UpdateUI(Action action)
        {
            Dispatcher.InvokeAsync(action);
        }
        #endregion
    }
}