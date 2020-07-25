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
using static ApiInspector.DataFlow.ServiceKeys;

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
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            InitializeComponent();

            historyPanel.Context = context;

            context.OnUpdate(SelectedInvocationInfoKey, () => SetSelectedInvocationInfo(SelectedInvocationInfoKey[context]));

            currentInvocationInfo.Context = context;

            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher, Model.TraceMessages);

            traceMonitor.StartToMonitor();

            Loaded += (s, e) => { historyPanel.InitializeHistoryPanel(); };
        }
        #endregion

        #region Properties
        /// <summary>
        ///     The history
        /// </summary>
        DataSource History => HistoryServiceKey[context];

        /// <summary>
        ///     The Model
        /// </summary>
        MainWindowViewModel Model => MainWindowViewModelKey[context];
        #endregion

        #region Public Methods
        /// <summary>
        ///     Sets the selected invocation information.
        /// </summary>
        public void SetSelectedInvocationInfo(InvocationInfo invocationInfo)
        {
            Model.InvocationEditor.InvocationInfo = invocationInfo;

            invokingResponseView.SetText(string.Empty);

            if (invocationInfo != null)
            {
                responseOutputFilePath.Text = invocationInfo.ResponseOutputFilePath;
            }

            currentInvocationInfo.OnInvocationInfoChanged();

            Model.TraceMessages.Add("Selected invocation was changed.");
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (Model.InvocationEditor.InvocationInfo == null || string.IsNullOrWhiteSpace(Model.InvocationEditor.InvocationInfo.MethodName))
            {
                ((App) System.Windows.Application.Current).ErrorMonitor.ShowErrorNotification("Item should selected from history.");
                return;
            }

            Task.Run(() => History.SaveToHistory(Model.InvocationEditor.InvocationInfo));

            new Thread(OnExecuteClicked).Start();
        }

        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked()
        {
            Action<string> trace = Model.TraceMessages.Add;

            var invocationInfo = Model.InvocationEditor.InvocationInfo;

            UpdateUI(() => { invokingResponseView.SetText(string.Empty); });

            trace("------------- EXECUTE STARTED -----------------");

            var invoker = new Invoker(TraceKey[context]);

            var invokerOutput = invoker.Invoke(invocationInfo);

            UpdateUI(() => { invokingResponseView.SetText(invokerOutput.ExecutionResponseAsJson); });

            if (!string.IsNullOrWhiteSpace(invocationInfo.ResponseOutputFilePath))
            {
                Utility.WriteAllText(invocationInfo.ResponseOutputFilePath, invokerOutput.ExecutionResponseAsJson);
            }

            trace(string.Empty);
            trace(string.Empty);
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the ResponseOutputFilePath control.
        /// </summary>
        void ResponseOutputFilePath_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Model.InvocationEditor.InvocationInfo.ResponseOutputFilePath = responseOutputFilePath.Text;
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