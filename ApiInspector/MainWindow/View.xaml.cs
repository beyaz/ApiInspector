using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Application;
using ApiInspector.DataFlow;
using ApiInspector.History;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The view
    /// </summary>
    public partial class View
    {
        #region Fields
        /// <summary>
        ///     The context
        /// </summary>
        readonly DataContext context = new DataContextBuilder().Build();


         readonly TraceQueue traceQueue = new TraceQueue();

        InvocationInfo InvocationInfo => SelectedInvocationInfoKey[context];
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            InitializeComponent();

            

            
            
            context.OnUpdate(SelectedInvocationInfoKey, RefreshResponseOutputFilePath);
            context.OnUpdate(SelectedInvocationInfoKey, ()=>invokingResponseView.SetText(string.Empty));
            
            

            

            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher,traceQueue);

            traceMonitor.StartToMonitor();

            Loaded += (s, e) =>
            {
                historyPanel.Connect(traceQueue.AddMessage);
                historyPanel.SelectedInvocationChanged += () => context.Update(SelectedInvocationInfoKey, historyPanel.SelectedInvocationInfo);
                historyPanel.SelectedInvocationChanged += () => currentInvocationInfo.Connect(historyPanel.SelectedInvocationInfo,traceQueue.AddMessage);
            };
        }
        #endregion

        public void Connect(DataContext context)
        {
            
        }

        #region Properties
        /// <summary>
        ///     The history
        /// </summary>
        readonly DataSource History = new DataSource();

        #endregion

        #region Public Methods


        void RefreshResponseOutputFilePath()
        {
            var invocationInfo = SelectedInvocationInfoKey[context];
            if (invocationInfo == null)
            {
                responseOutputFilePath.Text = string.Empty;
                return;
            }

            responseOutputFilePath.Text = invocationInfo.ResponseOutputFilePath;
        }

      
        #endregion

        #region Methods
        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (InvocationInfo == null || string.IsNullOrWhiteSpace(InvocationInfo.MethodName))
            {
                ((App) System.Windows.Application.Current).ErrorMonitor.ShowErrorNotification("Item should selected from history.");
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

            var invoker = new Invoker(Trace);

            var invokerOutput = invoker.Invoke(invocationInfo);

            UpdateUI(() => { invokingResponseView.SetText(invokerOutput.ExecutionResponseAsJson); });

            if (!string.IsNullOrWhiteSpace(invocationInfo.ResponseOutputFilePath))
            {
                Utility.WriteAllText(invocationInfo.ResponseOutputFilePath, invokerOutput.ExecutionResponseAsJson);
            }

            Trace(string.Empty);
            Trace(string.Empty);
        }

        void Trace(string message)
        {
            traceQueue.AddMessage(message);
        }

        bool anyItemSelectedInHistoryPanel => context.Contains(SelectedInvocationInfoKey);
        /// <summary>
        ///     Handles the OnTextChanged event of the ResponseOutputFilePath control.
        /// </summary>
        void ResponseOutputFilePath_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!anyItemSelectedInHistoryPanel)
            {
                return;
            }

            InvocationInfo.ResponseOutputFilePath = responseOutputFilePath.Text;
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