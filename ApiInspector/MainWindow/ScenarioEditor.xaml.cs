using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Components;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Invoking;
using ApiInspector.Models;
using static ApiInspector.Keys;
using static ApiInspector.MainWindow.Mixin;
using static ApiInspector.WPFExtensions;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     Interaction logic for ScenarioEditor.xaml
    /// </summary>
    public partial class ScenarioEditor
    {
        #region Static Fields
        /// <summary>
        ///     The scenario data property
        /// </summary>
        public static readonly DependencyProperty ScenarioDataProperty = DependencyProperty.Register("ScenarioData", typeof(ScenarioInfo), typeof(ScenarioEditor), new PropertyMetadata(default(ScenarioInfo)));
        #endregion

        #region Fields
        /// <summary>
        ///     The scope
        /// </summary>
        Scope scope;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScenarioEditor"/> class.
        /// </summary>
        public ScenarioEditor()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
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
        #endregion

        #region Methods
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

            var assertionsEditor = new AssertionsEditor();

            assertionsEditor.Loaded += (s, e) => assertionsEditor.Connect(scope);

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

            InvokeOutput FindOutput()
            {
                var invokeOutputs = scope.TryGet(InvokeOutputs);

                if (invokeOutputs == null || invokeOutputs.Length == 0)
                {
                    return null;
                }

                var scenarioIndex = scenarios.IndexOf(scenario);
                if (scenarioIndex >= 0 && invokeOutputs.Length > scenarioIndex)
                {
                    return invokeOutputs[scenarioIndex];
                }

                return null;
            }

            var responseTextView = new JsonTextEditor();
            var updateResponseOutputText = fun(() =>
            {
                var output = FindOutput();
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


            var inputEditors = ParameterPanelIntegration.Create(scenario.MethodParameters, methodDefinition).ToArray();

            EnableAllTopButtons();
            buttonActivateInputOutputPanel.IsPressed = true;


            CurrentContent = NewColumnSplittedGrid(NewGroupBox(NewBoldTextBlock("Method Parameters"), NewStackPanel(inputEditors)),
                                                   NewGroupBox(NewBoldTextBlock("Response"), responseTextView));

            updateResponseOutputText();

            UpdateOutput = updateResponseOutputText;
        }

        Action UpdateOutput;

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

        /// <summary>
        ///     Attaches the events.
        /// </summary>
        void AttachEvents()
        {
            scope.OnUpdate(SelectedScenario, UpdateNumbers);
            scope.OnUpdate(SelectedScenario, ArrangeRemoveScenarioButtonVisibility);
            scope.OnUpdate(SelectedScenario, MakePressedSelectedScenario);
            scope.OnUpdate(SelectedMethodDefinition, BuildScenarioList);
            // scope.OnUpdate(SelectedMethodDefinition, ActivateInputOutputPanel);
            scope.OnUpdate(SelectedMethodDefinition, ArrangeVisibilityOnEodMethod);
            

            scope.OnUpdate(SelectedScenario, ActivateInputOutputPanel);

            scope.SubscribeEvent(ScenarioEvent.RemoveSelectedScenario, () =>
            {
                scenarios.Remove(selectedScenario);
                BuildScenarioList();
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
                scope.Get(AddNewScenario)(scenario);
            }
        }

        
        void EnableAllTopButtons()
        {
            buttonActivateInputOutputPanel.IsPressed = false;
            buttonActivateExportPanel.IsPressed      = false;
            buttonAssertions.IsPressed               = false;
        }

        /// <summary>
        ///     Finds the selected action button.
        /// </summary>
        ActionButton FindSelectedActionButton()
        {
            foreach (ActionButton child in scenarioNumbersContainer.Children)
            {
                var scenario = (ScenarioInfo) child.GetValue(ScenarioDataProperty);

                if (scenario == scope.Get(SelectedScenario))
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        ///     Makes the pressed selected scenario.
        /// </summary>
        void MakePressedSelectedScenario()
        {
            var actionButton = FindSelectedActionButton();

            if (actionButton != null)
            {
                actionButton.IsPressed = true;
            }
        }

        /// <summary>
        ///     Called when [add new scenario clicked].
        /// </summary>
        void OnAddNewScenarioClicked(object sender, RoutedEventArgs e)
        {
            scope.Get(AddNewScenario)(CreateNewScenarioInfo());
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
            scope.PublishEvent(ScenarioEvent.RemoveSelectedScenario);
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

                actionButton.SetValue(ScenarioDataProperty, scenario);

                actionButton.Click += (s, e) => { scope.Update(SelectedScenario, scenario); };

                scenarioNumbersContainer.Children.Add(actionButton);

                i++;
            }

            VerticalIndent(scenarioNumbersContainer, 10);
        }
        #endregion

        void ArrangeVisibilityOnEodMethod()
        {
            var isEodMethod = InvocationInfo?.MethodName == EndOfDay.MethodAccessText;
            if (isEodMethod)
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
    }
}