using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Components;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Models;
using static ApiInspector.Keys;
using static ApiInspector.MainWindow.Mixin;
using static ApiInspector.WPFExtensions;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     Interaction logic for ScenarioEditor.xaml
    /// </summary>
    public partial class ScenarioEditor
    {

        /// <summary>
        ///     The scope
        /// </summary>
        Scope scope;


        /// <summary>
        ///     Initializes a new instance of the <see cref="ScenarioEditor" /> class.
        /// </summary>
        public ScenarioEditor()
        {
            InitializeComponent();
        }
        /// <summary>
        ///     Sets the content of the current.
        /// </summary>
        FrameworkElement CurrentContent
        {
            set
            {
                contentContainer.Children.Clear();
                contentContainer.Children.Add(value);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance has invocation information.
        /// </summary>
        bool HasInvocationInfo => scope.Contains(SelectedInvocationInfo);

        /// <summary>
        ///     Gets the invocation information.
        /// </summary>
        InvocationInfo InvocationInfo => scope.TryGet(SelectedInvocationInfo);

        /// <summary>
        ///     Gets the scenarios.
        /// </summary>
        List<ScenarioInfo> scenarios => scope.Get(SelectedInvocationInfo).Scenarios;

        /// <summary>
        ///     Gets the selected scenario.
        /// </summary>
        ScenarioInfo selectedScenario => scope.Get(SelectedScenario);
        /// <summary>
        ///     Connects the specified scope.
        /// </summary>
        internal void Connect(Scope scope)
        {
            this.scope = scope;
            AttachEvents();
        }

        /// <summary>
        ///     Activates the assertions.
        /// </summary>
        AssertionsEditor ActivateAssertions()
        {
            EnableAllTopButtons();
            buttonAssertions.IsPressed = true;

            var assertionsEditor = new AssertionsEditor
            {
                scope = scope
            };

            CurrentContent = assertionsEditor;

            return assertionsEditor;
        }

        /// <summary>
        ///     Activates the export panel.
        /// </summary>
        void ActivateExportPanel(object sender, RoutedEventArgs e)
        {
            ActivateExportPanel();
        }

        /// <summary>
        ///     Activates the export panel.
        /// </summary>
        void ActivateExportPanel()
        {
            var scenario = scope.Get(SelectedScenario);

            EnableAllTopButtons();
            buttonActivateExportPanel.IsPressed = true;

            var editor = new TextBox();

            Bind(editor, TextBox.TextProperty, scenario, nameof(scenario.ResponseOutputFilePath));

            CurrentContent = NewGroupBox(NewBoldTextBlock("Export"), NewStackPanel(NewBoldTextBlock("Export Result To Path"), editor)).UpdatePadding(10);
        }

        /// <summary>
        ///     Activates the input output panel.
        /// </summary>
        void ActivateInputOutputPanel()
        {
            var scenario         = scope.Get(SelectedScenario);
            var methodDefinition = scope.Get(SelectedMethodDefinition);

            EnableAllTopButtons();
            buttonActivateInputOutputPanel.IsPressed = true;
            
            var responseTextView = new JsonTextEditor();

            void updateResponseOutputText()
            {
                Dispatcher.InvokeAsync(() =>
                {

                    var output = scope.TryGetScenarioExecuteResponse(scenario)?.InvokeOutput;
                    if (output == null)
                    {
                        responseTextView.Text = null;
                        return;
                    }

                    if (!output.IsSuccess)
                    {
                        responseTextView.Text = "ERROR: " + output.Error;
                        return;
                    }

                    responseTextView.Text = output.ExecutionResponseAsJson;
                });
            }

            var inputEditors = ParameterPanelIntegration.Create(scenario.MethodParameters, methodDefinition).ToArray();

            EnableAllTopButtons();
            buttonActivateInputOutputPanel.IsPressed = true;

            var content = NewColumnSplittedGrid(NewGroupBox(NewBoldTextBlock("Method Parameters"), NewStackPanel(inputEditors)),
                                                   NewGroupBox(NewBoldTextBlock("Response"), responseTextView));


            responseTextView.Loaded   += (s, e) => scope.SubscribeEvent(OnScenarioExecuteResponseUpdated,  updateResponseOutputText);
            responseTextView.Unloaded += (s, e) => scope.UnSubscribeEvent(OnScenarioExecuteResponseUpdated, updateResponseOutputText);

            CurrentContent = content;

            updateResponseOutputText();

        }

        void AddNewScenario(ScenarioInfo scenarioInfo)
        {
            scenarios.Add(scenarioInfo);
            scope.Update(SelectedScenario, scenarioInfo);
        }

        /// <summary>
        ///     Arranges the remove scenario button visibility.
        /// </summary>
        void ArrangeRemoveScenarioButtonVisibility()
        {
            if (scope.Contains(SelectedScenario) && scenarios.Count > 1)
            {
                removeScenarioButton.Visibility = Visibility.Visible;
                return;
            }

            removeScenarioButton.Visibility = Visibility.Hidden;
        }

        void ArrangeVisibilityOnEodMethod()
        {
            if (IsEndOfDayMethod(InvocationInfo))
            {
                tabHeadersContainerPanel.Visibility = Visibility.Collapsed;
                addRemovePanel.Visibility           = Visibility.Collapsed;
                scenarioNumbersContainer.Visibility = Visibility.Collapsed;
                contentContainer.Visibility         = Visibility.Collapsed;
            }
            else
            {
                tabHeadersContainerPanel.Visibility = Visibility.Visible;
                addRemovePanel.Visibility           = Visibility.Visible;
                scenarioNumbersContainer.Visibility = Visibility.Visible;
                contentContainer.Visibility         = Visibility.Visible;
            }
        }

        /// <summary>
        ///     Attaches the events.
        /// </summary>
        void AttachEvents()
        {
            scope.OnUpdate(SelectedScenario, ()=>UpdateUI(UpdateNumbers));
            scope.OnUpdate(SelectedScenario, ()=>UpdateUI(ArrangeRemoveScenarioButtonVisibility));
            scope.OnUpdate(SelectedScenario, ()=>UpdateUI(ActivateInputOutputPanel));

            scope.OnUpdate(SelectedMethodDefinition, BuildScenarioList);
            scope.OnUpdate(SelectedMethodDefinition, ArrangeVisibilityOnEodMethod);

            scope.SubscribeEvent(OnExecutionStarted, () =>
            {
                UpdateUI(() =>
                {
                    executeSelectedScenarioButton.Text      = "Executing...";
                    executeSelectedScenarioButton.IsEnabled = false;
                });
            });

            scope.SubscribeEvent(OnExecutionFinished, () =>
            {
                UpdateUI(() =>
                {
                    executeSelectedScenarioButton.Text      = "Execute";
                    executeSelectedScenarioButton.IsEnabled = true;
                });
            });

           
        }

        /// <summary>
        ///     Builds the scenario list.
        /// </summary>
        void BuildScenarioList()
        {
            var scenarioList = new List<ScenarioInfo>(scenarios);

            scenarios.Clear();

            foreach (var scenario in scenarioList)
            {
                AddNewScenario(scenario);
            }
        }

        void EnableAllTopButtons()
        {
            buttonActivateInputOutputPanel.IsPressed = false;
            buttonActivateExportPanel.IsPressed      = false;
            buttonAssertions.IsPressed               = false;
        }

        

        

        /// <summary>
        ///     Called when [add new scenario clicked].
        /// </summary>
        void OnAddNewScenarioClicked(object sender, RoutedEventArgs e)
        {
            AddNewScenario(CreateNewScenarioInfo());
        }

        /// <summary>
        ///     Called when [assertions clicked].
        /// </summary>
        void OnAssertionsClicked(object sender, RoutedEventArgs e)
        {
            ActivateAssertions();
        }

        /// <summary>
        ///     Called when [button activate input output panel clicked].
        /// </summary>
        void OnButtonActivateInputOutputPanelClicked(object sender, RoutedEventArgs e)
        {
            ActivateInputOutputPanel();
        }

        /// <summary>
        ///     Called when [remove selected scenario clicked].
        /// </summary>
        void OnRemoveSelectedScenarioClicked(object sender, RoutedEventArgs e)
        {
            scenarios.Remove(selectedScenario);
            BuildScenarioList();
        }

        /// <summary>
        ///     Updates the numbers.
        /// </summary>
        void UpdateNumbers()
        {
            scenarioNumbersContainer.Children.Clear();

            var i = 1;

            foreach (var scenario in scenarios)
            {
                var actionButton = new ActionButton
                {
                    Text = i.ToString()
                };


                actionButton.Click += (s, e) => { scope.Update(SelectedScenario, scenario); };

                void calculateIcon()
                {
                    var scenarioExecuteResponseInfo = scope.TryGetScenarioExecuteResponse(scenario);
                    if (scenarioExecuteResponseInfo == null)
                    {
                        actionButton.IconVisibility = Visibility.Collapsed;
                        return;
                    }

                    if (scenarioExecuteResponseInfo.IsSuccess)
                    {
                        actionButton.ShowSuccessIcon();
                        return;
                    }

                    actionButton.ShowFailIcon();
                }
                calculateIcon();
                actionButton.Loaded   += (s, e) => scope.SubscribeEvent(OnScenarioExecuteResponseUpdated, calculateIcon);
                actionButton.Unloaded += (s, e) => scope.UnSubscribeEvent(OnScenarioExecuteResponseUpdated, calculateIcon);

                void arrangeIsPressed()
                {
                    if (selectedScenario == scenario)
                    {
                        actionButton.IsPressed = true;
                        return;
                    }

                    actionButton.IsPressed = false;
                }

                arrangeIsPressed();

                actionButton.Loaded   += (s, e) => scope.OnUpdate(SelectedScenario, arrangeIsPressed);
                actionButton.Unloaded += (s, e) => scope.UnUpdate(SelectedScenario, arrangeIsPressed);



                scenarioNumbersContainer.Children.Add(actionButton);

                i++;
            }

            VerticalIndent(scenarioNumbersContainer, 10);
        }
    }
}