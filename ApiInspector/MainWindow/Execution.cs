using System;
using System.Threading.Tasks;
using System.Windows;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using static System.String;
using static ApiInspector.Keys;
using static ApiInspector.MainWindow.Mixin;
using static ApiInspector.Utility;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    partial class ScenarioEditor
    {
        

        #region Methods
        bool ExecuteSelectedScenario()
        {
            void trace(string message)
            {
                scope.Get(Trace)(message);
            }

            trace("EXECUTE STARTED");

            scope.ClearScenarioOutputs();
            scope.ClearAssertionExecuteResponses();

            var invocationInfo  = InvocationInfo;
            var environmentInfo = EnvironmentInfo.Parse(invocationInfo.Environment);

            if (IsEndOfDayMethod(invocationInfo))
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

                scope.UpdateScenarioOutput(scenario, invokeOutput);

                if (!invokeOutput.IsSuccess)
                {
                    trace("EXECUTION IS FAILED.");
                    return false;
                }

                if (!IsNullOrWhiteSpace(scenario.ResponseOutputFilePath))
                {
                    WriteToFile(scenario.ResponseOutputFilePath, invokeOutput.ExecutionResponseAsJson);
                }

                bool runAssertions()
                {
                    var methodDefinition = scope.Get(SelectedMethodDefinition);

                    string runAssertion(AssertionInfo assertionInfo)
                    {
                        var env = EnvironmentInfo.Parse(invocationInfo.Environment);

                        var actual       = AssertionValueCalculator.CalculateFrom(assertionInfo.Actual, methodDefinition, invokeOutput, env);
                        var expected     = AssertionValueCalculator.CalculateFrom(assertionInfo.Expected, methodDefinition, invokeOutput, env);
                        var errorMessage = AssertionValueCalculator.RunAssertion(actual, expected, assertionInfo.OperatorName);
                        
                        scope.UpdateAssertionExecuteResponse(new AssertionExecuteResponseInfo(assertionInfo) {ErrorMessage = errorMessage});

                        UpdateUI(() =>
                        {
                            var assertionsEditor = ActivateAssertions();

                            assertionsEditor.Loaded += (s, e) => { assertionsEditor.selectedAssertion = assertionInfo; };
                        });

                        return errorMessage;
                    }

                    foreach (var assertion in scenario.Assertions)
                    {
                        var assertionErrorMessage = runAssertion(assertion);
                        if (assertionErrorMessage != null)
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

        const string OnExecutionStarted = nameof(OnExecutionStarted);
        const string OnExecutionFinished = nameof(OnExecutionFinished);

        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (!HasInvocationInfo || IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                scope.ShowErrorNotification("MethodName can not be empty.");
                return;
            }
            
            UpdateScenarioActionIcon(success: null);

            Task.Run(() => scope.PublishEvent(HistoryEvent.SaveToHistory));
            Task.Run(() =>
            {
                try
                {
                    scope.PublishEvent(OnExecutionStarted);

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
                    scope.PublishEvent(OnExecutionFinished);

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

        void UpdateUI(Action action)
        {
            Dispatcher.InvokeAsync(action);
        }
        #endregion
    }
}