using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ApiInspector.DataAccess;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using ApiInspector.Serialization;
using static System.String;
using static ApiInspector.Keys;
using static ApiInspector.MainWindow.Mixin;
using static ApiInspector.Utility;

namespace ApiInspector.MainWindow
{
    partial class ScenarioEditor
    {

        void ExecuteSelectedScenarioAndProcessResponse()
        {
            void trace(string message)
            {
                scope.Get(Trace)(message);
            }

            try
            {

                scope.PublishEvent(OnExecutionStarted);

                trace("EXECUTE STARTED");

                var response = ExecuteSelectedScenario();
                
                UpdateUI(()=>scope.UpdateScenarioExecuteResponse(response));

                var invokeOutput = response.InvokeOutput;
                if (!invokeOutput.IsSuccess)
                {
                    trace(invokeOutput.Error.ToString());
                    trace("EXECUTION IS FAILED.");
                    return;
                }

                var failedAssertion = response.AssertionExecuteResponses.FirstOrDefault(a => !a.IsSuccess)?.AssertionInfo;
                if (failedAssertion != null)
                {
                    trace("EXECUTION IS SUCCESSFULL BUT ASSERTIONS ARE FAILED.");

                    // focus to failed assertion
                    UpdateUI(() =>
                    {
                        var assertionsEditor = ActivateAssertions();

                        assertionsEditor.Loaded += (s, e) => { assertionsEditor.selectedAssertion = failedAssertion; };
                    });
                        
                    return;
                }

                trace("EXECUTION IS SUCCESSFULL");

            }
            catch (Exception exception)
            {
                MessageBox.Show("Beklenmedik bir hata oluştu.(Abdullah Beyaztaş'a mail atarsanız sevinirim.) " + exception);
            }
            finally
            {
                scope.PublishEvent(OnExecutionFinished);

            }
        }



        #region Methods
        ScenarioExecuteResponseInfo ExecuteSelectedScenario()
        {
            var returnValue = new ScenarioExecuteResponseInfo();

            var invocationInfo  = InvocationInfo;
            var environmentInfo = EnvironmentInfo.Parse(invocationInfo.Environment);

            if (IsEndOfDayMethod(invocationInfo))
            {
                returnValue.InvokeOutput = Invoker.Invoke(environmentInfo, scope.Get(Trace), invocationInfo, null);

                return returnValue;
            }

            var scenario = scope.Get(SelectedScenario);

            scope.ClearScenarioExecuteResponse(scenario);

            returnValue.Scenario = scenario;


            void processAssignmentsBeforeInvoke(AssertionInfo assertionInfo)
            {
                var methodDefinition = scope.Get(SelectedMethodDefinition);

               

                var env = EnvironmentInfo.Parse(invocationInfo.Environment);

                
                var actual = AssertionValueCalculator.CalculateFrom(assertionInfo.Actual, methodDefinition, new InvokeOutput(string.Empty)
                {
                    InvocationParameters = scenario.MethodParameters.Select(x=>x.Value).ToList()
                }, env);

                var targetPath = assertionInfo.Expected.Text.Trim();

                if (targetPath.Contains("."))
                {
                    foreach (var parameterDefinition in methodDefinition.Parameters)
                    {
                        var prefix = $"{CecilHelper.PrefixCharacter}{parameterDefinition.Name}.";
                        if (targetPath.StartsWith(prefix))
                        {
                            var instance = DeserializeForMethodParameter(scenario.MethodParameters[parameterDefinition.Index].Value, parameterDefinition.ParameterType.GetDotNetType());

                            ReflectionUtil.SaveValueToPropertyPath(actual, instance, targetPath.RemoveFromStart(prefix));
                            
                            scenario.MethodParameters[parameterDefinition.Index] = new InvocationMethodParameterInfo
                            {
                                Value = SerializeForMethodParameter(instance)
                            };
                        }

                    }
                }
                else
                {
                    foreach (var parameterDefinition in methodDefinition.Parameters)
                    {
                        if (targetPath == CecilHelper.PrefixCharacter + parameterDefinition.Name)
                        {
                            scenario.MethodParameters[parameterDefinition.Index] = new InvocationMethodParameterInfo
                            {
                                Value = SerializeForMethodParameter(actual)
                            };
                        }

                    }
                }

                returnValue.AssertionExecuteResponses.Add(new AssertionExecuteResponseInfo(assertionInfo));

            }

            foreach (var assertionInfo in scenario.Assertions.Where(a=>a.OperatorName == AssertionOperatorNames.AssignTo))
            {
                processAssignmentsBeforeInvoke(assertionInfo);
            }


            var invokeOutput = returnValue.InvokeOutput = Invoker.Invoke(environmentInfo, scope.Get(Trace), invocationInfo, scenario.MethodParameters);


            if (!invokeOutput.IsSuccess)
            {
                return returnValue;
            }

            

            void runAssertions()
            {
                var methodDefinition = scope.Get(SelectedMethodDefinition);

                string runAssertion(AssertionInfo assertionInfo)
                {
                    var env = EnvironmentInfo.Parse(invocationInfo.Environment);

                    var actual       = AssertionValueCalculator.CalculateFrom(assertionInfo.Actual, methodDefinition, invokeOutput, env);
                    var expected     = AssertionValueCalculator.CalculateFrom(assertionInfo.Expected, methodDefinition, invokeOutput, env);
                    var errorMessage = AssertionValueCalculator.RunAssertion(actual, expected, assertionInfo.OperatorName);

                    returnValue.AssertionExecuteResponses.Add(new AssertionExecuteResponseInfo(assertionInfo) {ErrorMessage = errorMessage});

                    return errorMessage;
                }

                foreach (var assertion in scenario.Assertions.Where(a=>a.OperatorName != AssertionOperatorNames.AssignTo))
                {
                    var assertionErrorMessage = runAssertion(assertion);
                    if (assertionErrorMessage != null)
                    {
                        return;
                    }
                }

            }

            runAssertions();

            return returnValue;

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
            
            Task.Run(() => scope.PublishEvent(HistoryEvent.SaveToHistory));
            Task.Run(() => { ExecuteSelectedScenarioAndProcessResponse(); });
        }

       

        void UpdateUI(Action action)
        {
            Dispatcher.InvokeAsync(action);
        }
        #endregion
    }
}