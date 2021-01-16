using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Components;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using WpfControls;
using static ApiInspector.Keys;
using static ApiInspector.WPFExtensions;
using static FunctionalPrograming.FPExtensions;
using static ApiInspector.Utility;

namespace ApiInspector.MainWindow
{
    /// <summary>
    /// Interaction logic for ScenarioEditor.xaml
    /// </summary>
    public partial class ScenarioEditor
    {

        public Action<string> ShowErrorNotification;


        public static readonly DependencyProperty ScenarioDataProperty = DependencyProperty.Register(
                                                        "ScenarioData", typeof(Scenario), typeof(ScenarioEditor), new PropertyMetadata(default(Scenario)));

       

        Scope scope;

        internal void Connect(Scope scope)
        {
            this.scope = scope;
            AttachEvents();
        }

        void AttachEvents()
        {
            scope.OnUpdate(SelectedScenario, UpdateTabHeaders);
            scope.OnUpdate(SelectedScenario, ArrangeRemoveScenarioButtonVisibility);
            scope.OnUpdate(SelectedScenario, MakePressedSelectedScenario);
            scope.OnUpdate(SelectedMethodDefinition, BuildScenarioList);
            scope.OnUpdate(SelectedMethodDefinition, ActivateInputOutputPanel);

            scope.OnUpdate(SelectedScenario, ActivateInputOutputPanel);

            scope.SubscribeEvent(ScenarioEvent.RemoveSelectedScenario, () =>
            {
                scenarios.Remove(selectedScenario);
                BuildScenarioList();

            });
        }

        void BuildScenarioList()
        {
            var scenarioList = new List<Scenario>(scenarios);

            scenarios.Clear();

            foreach (var scenario in scenarioList)
            {
                scope.Get(AddNewScenario)(scenario);
            }
        }

        void ArrangeRemoveScenarioButtonVisibility()
        {
            if (scope.Contains(SelectedScenario) && scenarios.Count > 1)
            {
                removeScenarioButton.Visibility = Visibility.Visible;
                return;
            }

            removeScenarioButton.Visibility = Visibility.Hidden;
        }


        InvokeOutput FindOutput(Scenario scenario)
        {
            var invokeOutputs = scope.TryGet(InvokeOutputs);

            if (invokeOutputs == null || invokeOutputs.Length == 0)
            {
                return null;
            }

            var scenarioIndex = scenarios.IndexOf(scenario);
            if (scenarioIndex>=0 && invokeOutputs.Length>scenarioIndex)
            {
                return invokeOutputs[scenarioIndex];    
            }

            return null;
        }

        void EnableAllLeftButtons()
        {
            buttonActivateInputOutputPanel.IsPressed = false;
            buttonActivateExportPanel.IsPressed = false;
            buttonAssertions.IsPressed               = false;
        }

        void OnAssertionsClicked(object sender, RoutedEventArgs e)
        {
            EnableAllLeftButtons();
            buttonAssertions.IsPressed = true;

            ActivateAssertions();

        }

        void ActivateAssertions()
        {
            var scenario = scope.Get(SelectedScenario);


            var editor = new TextBox();

            Bind(editor,TextBox.TextProperty,scenario,nameof(scenario.ResponseOutputFilePath));


            if (scenario.Assertions.Count==0)
            {
                scenario.Assertions.Add(new Assertion
                {
                    Value = new ValueAccessInfo()
                });
            }

            var assertion = scenario.Assertions[0];


            var actualEditor   = NewStackPanel(NewBoldTextBlock("Actual"), editor);


            var valueAccessTypeEditor = new IntellisenseTextBox
            {
                Suggestions = Enum.GetNames(typeof(ValueAccessType))
            };
            Bind(valueAccessTypeEditor,AutoCompleteTextBox.TextProperty,assertion.Value,nameof(assertion.Value.Type));

            var expectedEditor = NewStackPanel(NewBoldTextBlock("Expected"), valueAccessTypeEditor);

            
            var operatorEditor = NewStackPanel(NewBoldTextBlock("Operator"), new TextBox());
            
            CurrentContent = NewGroupBox(NewBoldTextBlock("Assertions"),NewGridWithColumns(new int[]{5,2,5},actualEditor,operatorEditor, expectedEditor)).UpdatePadding(10);
        }

        void OnButtonActivateInputOutputPanelClicked(object sender, RoutedEventArgs e)
        {
            EnableAllLeftButtons();
            buttonActivateInputOutputPanel.IsPressed = true;

            ActivateInputOutputPanel();
            
        }

        void ActivateExportPanel(object sender, RoutedEventArgs e)
        {
            EnableAllLeftButtons();
            buttonActivateExportPanel.IsPressed = true;

            ActivateExportPanel();
        }

        void ActivateExportPanel()
        {
            var scenario = scope.Get(SelectedScenario);


            var editor = new TextBox();

            Bind(editor,TextBox.TextProperty,scenario,nameof(scenario.ResponseOutputFilePath));

            
            CurrentContent = NewGroupBox(NewBoldTextBlock("Export"),NewStackPanel(NewBoldTextBlock("Export Result To Path"), editor)).UpdatePadding(10);
        }

        void ActivateInputOutputPanel()
        {

            var scenario         = scope.Get(SelectedScenario);
            var methodDefinition = scope.Get(SelectedMethodDefinition);
            
            var responseTextView = new JsonTextEditor();
            
            var updateResponseOutputText = fun(() =>
            {
                var output = FindOutput(scenario);
                if (output == null)
                {
                    responseTextView.Text = null;
                    return;
                }

                if (!output.IsSuccess)
                {
                    responseTextView.Text = "ERROR: "+ output.Error.ToString();  
                    return;
                }
                responseTextView.Text = output.ExecutionResponseAsJson;
            });

            scope.UnSubscribeEvent(ScenarioEvent.ExecutionFinished, updateResponseOutputText);
            scope.SubscribeEvent(ScenarioEvent.ExecutionFinished, updateResponseOutputText);
            
            var inputEditors = ParameterPanelIntegration.Create(scenario.MethodParameters, methodDefinition).ToArray();
            
            


            CurrentContent = NewColumnSplittedGrid(NewGroupBox(NewBoldTextBlock("Method Parameters"), NewStackPanel(inputEditors)), 
                                                   NewGroupBox(NewBoldTextBlock("Response"), responseTextView));

            updateResponseOutputText();
        }

        FrameworkElement CurrentContent
        {
            set
            {
                contentContainer.Children.Clear();
                contentContainer.Children.Add(value);
            }
        }

        List<Scenario> scenarios =>scope.Get(SelectedInvocationInfo).Scenarios;
        Scenario selectedScenario => scope.Get(SelectedScenario);

        public ScenarioEditor()
        {
            InitializeComponent();
        }

        void OnAddNewScenarioClicked(object sender, RoutedEventArgs e)
        {
            scope.Get(AddNewScenario)(new Scenario());
        }

        void OnRemoveSelectedScenarioClicked(object sender, RoutedEventArgs e)
        {
            scope.PublishEvent(ScenarioEvent.RemoveSelectedScenario);
        }

        void MakePressedSelectedScenario()
        {
            var actionButton = FindSelectedActionButton();

            if (actionButton != null)
            {
                actionButton.IsPressed = true;
            }
        }

        ActionButton FindSelectedActionButton()
        {
            foreach (ActionButton child in scenarioNumbersContainer.Children)
            {
                var scenario = (Scenario)child.GetValue(ScenarioDataProperty);

                if (scenario == scope.Get(SelectedScenario))
                {
                    return child;
                }
            }

            return null;
        }

        void UpdateTabHeaders()
        {

            scenarioNumbersContainer.Children.Clear();

            var i = 1;

            foreach (var scenario in scenarios)
            {
                var actionButton = new ActionButton
                {
                    Text = i.ToString(),
                   
                };

                actionButton.SetValue(ScenarioDataProperty,scenario);

                actionButton.Click += (s, e) =>
                {
                    scope.Update(SelectedScenario,scenario);
                };

                scenarioNumbersContainer.Children.Add(actionButton);
                
                i++;
            }

            VerticalIndent(scenarioNumbersContainer,10);
        }

        InvocationInfo InvocationInfo => scope.TryGet(SelectedInvocationInfo);
        
         void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (InvocationInfo == null || string.IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                ShowErrorNotification("MethodName can not be empty.");
                return;
            }

            Task.Run(() => scope.PublishEvent(HistoryEvent.SaveToHistory));
            Task.Run(()=>ExecuteSelectedScenario());
        }


         void UpdateScenarioOutput(int scenarioIndex, InvokeOutput invokeOutput)
         {
             if (!scope.Contains(InvokeOutputs))
             {
                 scope.Add(InvokeOutputs,new InvokeOutput[scenarios.Count]);
             }

             scope.Get(InvokeOutputs)[scenarioIndex] = invokeOutput;
         }

         void ExecuteAllScenarioList()
         {
             foreach (var scenario in scenarios)
             {
                 scope.Update(SelectedScenario,scenario);

                 ExecuteSelectedScenario();
             }
         }

         void ExecuteSelectedScenario()
         {
             var scenario      = scope.Get(SelectedScenario);

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
                 UpdateUI(() => executeSelectedScenarioButton.Text   = "Execute");
                 UpdateUI(() => executeSelectedScenarioButton.IsEnabled = true);
             }

             void trace(string message)
             {
                 scope.Get(Keys.Trace)(message);
             }

             OnEnteredToExecution();
            

             var scenarioIndex = scenarios.IndexOf(scenario);
            
             trace($"------------- EXECUTE STARTED For {scenarioIndex+1} -------------");


             
              var invokeOutput = Invoker.Invoke(environmentInfo, trace, invocationInfo, scenarioIndex);

              UpdateScenarioOutput(scenarioIndex,invokeOutput);

             
                
             if (!string.IsNullOrWhiteSpace(scenario.ResponseOutputFilePath))
             {
                 WriteToFile(scenario.ResponseOutputFilePath, invokeOutput.ExecutionResponseAsJson);
             }

             UpdateUI(()=>scope.PublishEvent(ScenarioEvent.ExecutionFinished));
            
             trace(string.Empty);
             trace(string.Empty);
             trace($"------------- EXECUTE FINISHED {scenarioIndex+1} -------------");

             OnExitToExecution();
         }

         
    }
}
