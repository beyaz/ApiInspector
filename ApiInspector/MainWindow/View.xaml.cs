using System;
using System.Diagnostics;
using System.Windows;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Models;
using ApiInspector.Tracing;
using static ApiInspector.Keys;
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

            void UpdateTitle()
            {
                Title = "ApiInspector - " + EnvironmentVariables.GetUserName();
            }

            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher, traceQueue);

            traceMonitor.StartToMonitor();

            Loaded += (s, e) =>
            {
                scope.Update(Keys.Trace, traceQueue.AddMessage);

                historyPanel.Trace = traceQueue.AddMessage;

                historyPanel.Connect(scope);
                currentInvocationInfo.Connect(scope);

                historyPanel.Refresh();

                scenarioEditor.ShowErrorNotification = ShowErrorNotification;
                scenarioEditor.Connect(scope);

                scope.Update(AddNewScenario, fun((ScenarioInfo scenario) =>
                {
                    scope.Get(SelectedInvocationInfo).Scenarios.Add(scenario);
                    scope.Update(SelectedScenario, scenario);
                }));

                UpdateTitle();
            };

            Closed += (s, e) => { System.Windows.Application.Current.Shutdown(); };
        }
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

        #endregion
    }
}