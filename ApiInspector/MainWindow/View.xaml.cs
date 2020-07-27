using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Application;
using ApiInspector.History;
using ApiInspector.Invoking;
using ApiInspector.Invoking.Invokers;
using ApiInspector.Models;
using ApiInspector.Tracing;
using Ninject;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The view
    /// </summary>
    public partial class View
    {
        #region Fields
        /// <summary>
        ///     The history
        /// </summary>
        readonly DataSource History = new DataSource();

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
        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (InvocationInfo == null || string.IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                ((App) System.Windows.Application.Current).ErrorMonitor.ShowErrorNotification("MethodName can not be empty.");
                return;
            }

            Task.Run(() => History.SaveToHistory(InvocationInfo));

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

            using (var injector = new Injector(traceQueue))
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