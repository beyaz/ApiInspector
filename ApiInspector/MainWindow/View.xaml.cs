using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Application;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using ApiInspector.Tracing;
using static ApiInspector.Keys;
using static ApiInspector.Utility;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The view
    /// </summary>
    partial class View
    {
        #region Fields
        /// <summary>
        ///     The scope
        /// </summary>
        readonly Scope scope;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View(Scope scope)
        {
            this.scope = scope;

            App.ApplicationScope.Update(UserVisibleTrace, traceQueue.AddMessage);

            InitializeGlobalFontStyle();

            InitializeComponent();


            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher, traceQueue);

            traceMonitor.StartToMonitor();

           

            Loaded += (s, e) =>
            {
                historyPanel.Connect(scope);
                currentInvocationInfo.Connect(scope);

                historyPanel.Refresh();

                scope.OnUpdate(Keys.SelectedInvocationInfo,RefreshResponseOutputFilePath);
                scope.OnUpdate(Keys.SelectedInvocationInfo,ClearResponseView);

                UpdateTitle();
            };
        }

        void ClearResponseView()
        {
            invokingResponseView.SetText(string.Empty);
        }

        #endregion

        #region Properties
        /// <summary>
        ///     Gets the invocation information.
        /// </summary>
        InvocationInfo InvocationInfo => scope.TryGet(Keys.SelectedInvocationInfo);

        /// <summary>
        ///     The trace queue
        /// </summary>
        TraceQueue traceQueue => scope.Get(Keys.TraceQueue);
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the new injector.
        /// </summary>
        Injector CreateNewInjector()
        {
            return new Injector(traceQueue, EnvironmentInfo.Parse(InvocationInfo.Environment));
        }

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
                scope.Get(Keys.ErrorMonitor).ShowErrorNotification("MethodName can not be empty.");
                return;
            }

            Task.Run(() => SaveToHistory());

            new Thread(OnExecuteClicked).Start();
        }

        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked()
        {
            OnEnteredToExecution();

            var invocationInfo = InvocationInfo;

            UpdateUI(() => { invokingResponseView.SetText(string.Empty); });

            Trace("------------- EXECUTE STARTED -----------------");

            InvokeOutput invokerOutput = null;

            using (var injector = CreateNewInjector())
            {
                var invoker = injector.Get<Invoker>();

                invokerOutput = invoker.Invoke(invocationInfo);
            }

            UpdateUI(() => { invokingResponseView.SetText(invokerOutput.ExecutionResponseAsJson); });

            if (!string.IsNullOrWhiteSpace(invocationInfo.ResponseOutputFilePath))
            {
                WriteToFile(invocationInfo.ResponseOutputFilePath, invokerOutput.ExecutionResponseAsJson);
            }

            Trace(string.Empty);
            Trace(string.Empty);

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
        ///     Refreshes the response output file path.
        /// </summary>
        void RefreshResponseOutputFilePath()
        {
            var invocationInfo = InvocationInfo;
            if (invocationInfo == null)
            {
                responseOutputFilePath.Text = string.Empty;
                return;
            }

            responseOutputFilePath.Text = invocationInfo.ResponseOutputFilePath;
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the ResponseOutputFilePath control.
        /// </summary>
        void ResponseOutputFilePath_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (InvocationInfo == null)
            {
                return;
            }

            InvocationInfo.ResponseOutputFilePath = responseOutputFilePath.Text;
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
            using (var injector = new Injector(traceQueue, EnvironmentInfo.Dev))
            {
                var userName = injector.Get<EnvironmentVariable>().GetUserName();
                Title = "ApiInspector - " + userName;
            }
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