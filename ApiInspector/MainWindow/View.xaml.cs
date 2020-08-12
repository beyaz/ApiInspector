using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.History;
using ApiInspector.Invoking;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using ApiInspector.Tracing;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The view
    /// </summary>
    partial class View
    {
        #region Fields
        /// <summary>
        ///     The trace queue
        /// </summary>
        readonly TraceQueue traceQueue;

        readonly ErrorMonitor errorMonitor;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View(TraceQueue traceQueue, ErrorMonitor errorMonitor)
        {
            this.traceQueue   = traceQueue ?? throw new ArgumentNullException(nameof(traceQueue));
            this.errorMonitor = errorMonitor ?? throw new ArgumentNullException(nameof(errorMonitor));

            InitializeComponent();

            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher, traceQueue);

            traceMonitor.StartToMonitor();
            currentInvocationInfo.model.Trace = traceQueue.AddMessage;

            Loaded += (s, e) =>
            {
                historyPanel.Connect(traceQueue.AddMessage);
                historyPanel.SelectedInvocationChanged += RefreshResponseOutputFilePath;
                historyPanel.SelectedInvocationChanged += () => invokingResponseView.SetText(string.Empty);

                historyPanel.SelectedInvocationChanged += () => currentInvocationInfo.Connect(historyPanel.SelectedInvocationInfo);
            };
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the invocation information.
        /// </summary>
        InvocationInfo InvocationInfo => currentInvocationInfo.model.InvocationInfo;
        #endregion

        #region Methods
        Injector CreateNewInjector()
        {
            return new Injector(traceQueue, EnvironmentInfo.Parse(InvocationInfo.Environment));
        }

        void OnConfigureClicked(object sender, RoutedEventArgs e)
        {
            Process.Start(@"D:\BOA\Server\bin\ApiInspectorConfiguration\");
        }

        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (InvocationInfo == null || string.IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                errorMonitor.ShowErrorNotification("MethodName can not be empty.");
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
                Utility.WriteAllText(invocationInfo.ResponseOutputFilePath, invokerOutput.ExecutionResponseAsJson);
            }

            Trace(string.Empty);
            Trace(string.Empty);
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

        void SaveToHistory()
        {
            using (var injector = CreateNewInjector())
            {
                var dataSource = injector.Get<DataSource>();

                dataSource.SaveToHistory(InvocationInfo);
            }
        }

        /// <summary>
        ///     Traces the specified message.
        /// </summary>
        void Trace(string message)
        {
            traceQueue.AddMessage(message);
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