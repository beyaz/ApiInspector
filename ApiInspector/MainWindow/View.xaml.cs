using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using ApiInspector.Tracing;
using static ApiInspector.Keys;
using static ApiInspector.Utility;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The view
    /// </summary>
    partial class View
    {
        #region Fields
        public Action<string> ShowErrorNotification;

        /// <summary>
        ///     The scope
        /// </summary>
        readonly Scope scope = new Scope();

        /// <summary>
        ///     The trace queue
        /// </summary>
        readonly TraceQueue traceQueue = new TraceQueue();
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            

            InitializeGlobalFontStyle();

            InitializeComponent();

            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher, traceQueue);

            traceMonitor.StartToMonitor();

            Loaded += (s, e) =>
            {
                scope.Update(Keys.Trace,traceQueue.AddMessage);

                historyPanel.Trace = traceQueue.AddMessage;

                historyPanel.Connect(scope);
                currentInvocationInfo.Connect(scope);

                historyPanel.Refresh();


                scenarioEditor.Connect(scope);
                scenarioEditor.ShowErrorNotification = ShowErrorNotification;

                scope.Update(AddNewScenario, fun((Scenario scenario) =>
                 {
                     InvocationInfo.Scenarios.Add(scenario);
                     scope.Update(SelectedScenario, scenario);
                     scope.PublishEvent(ScenarioEvent.NewScenarioAdded);
                 }));

                


                UpdateTitle();
            };

            Closed += (s, e) => { System.Windows.Application.Current.Shutdown(); };
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the invocation information.
        /// </summary>
        InvocationInfo InvocationInfo => scope.TryGet(SelectedInvocationInfo);
        #endregion

        #region Methods
        

        /// <summary>
        ///     Initializes the global font style.
        /// </summary>
        void InitializeGlobalFontStyle()
        {
            FontSize = 15;
        }

        /// <summary>
        ///     Called when [configure clicked].
        /// </summary>
        void OnConfigureClicked(object sender, RoutedEventArgs e)
        {
            Process.Start(@"D:\BOA\Server\bin\ApiInspectorConfiguration\");
        }

      


        /// <summary>
        ///     Called when [entered to execution].
        /// </summary>
        void OnEnteredToExecution()
        {
            UpdateUI(() => executeButton.Content   = "Executing...");
            UpdateUI(() => executeButton.IsEnabled = false);
        }

        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (InvocationInfo == null || string.IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                ShowErrorNotification("MethodName can not be empty.");
                return;
            }

            Task.Run(() => SaveToHistory());

            Dispatcher.InvokeAsync(OnExecuteClicked);
        }

        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked()
        {
            OnEnteredToExecution();

            var invocationInfo = InvocationInfo;

            var scenarioCount  = invocationInfo.Scenarios.Count;

            Trace("------------- EXECUTE STARTED -----------------");

            var invokeOutputs = new List<InvokeOutput>();

            scope.Update(InvokeOutputs,invokeOutputs);

            var environmentInfo = EnvironmentInfo.Parse(InvocationInfo.Environment);

            var trace = fun((string message) => { traceQueue.Trace(message); });
            
            var runScenarioAt = fun((int scenarioIndex) =>
            {
                var scenario = invocationInfo.Scenarios[scenarioIndex];

                scope.Update(SelectedScenario,scenario);

                invokeOutputs.Add(Invoker.Invoke(environmentInfo, trace, invocationInfo, scenarioIndex));
                
                if (!string.IsNullOrWhiteSpace(invocationInfo.ResponseOutputFilePath))
                {
                    WriteToFile(invocationInfo.ResponseOutputFilePath, invokeOutputs[scenarioIndex].ExecutionResponseAsJson);
                }
            });

            for (var i = 0; i < scenarioCount; i++)
            {
                runScenarioAt(i);
            }

            scope.PublishEvent(ScenarioEvent.ExecutionFinished);
            
            Trace(string.Empty);
            Trace(string.Empty);
            Trace("------------- EXECUTE FINISHED -----------------");

            OnExitToExecution();
        }

        /// <summary>
        ///     Called when [exit to execution].
        /// </summary>
        void OnExitToExecution()
        {
            UpdateUI(() => executeButton.Content   = "Execute");
            UpdateUI(() => executeButton.IsEnabled = true);
        }

        

        

        /// <summary>
        ///     Saves to history.
        /// </summary>
        void SaveToHistory()
        {
            scope.PublishEvent(HistoryEvent.SaveToHistory);
        }

        /// <summary>
        ///     Traces the specified message.
        /// </summary>
        void Trace(string message)
        {
            traceQueue.AddMessage(message);
        }

        

        /// <summary>
        ///     Updates the title.
        /// </summary>
        void UpdateTitle()
        {
            Title = "ApiInspector - " + EnvironmentVariables.GetUserName();
        }

        /// <summary>
        ///     Updates the UI.
        /// </summary>
        void UpdateUI(Action action)
        {
            Dispatcher.InvokeAsync(action);
        }
        #endregion
    }
}