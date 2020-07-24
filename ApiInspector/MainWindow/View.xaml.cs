using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
        readonly DataContext context = new DataContextBuilder().Build();

        #region Fields
        /// <summary>
        ///     The history
        /// </summary>
        readonly DataSource history = new DataSource();

        /// <summary>
        ///     The invoker
        /// </summary>
        readonly Invoker invoker;

        /// <summary>
        ///     The Model
        /// </summary>
        MainWindowViewModel Model => MainWindowViewModelKey[context];
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            InitializeComponent();

            currentInvocationInfo.Model = Model.InvocationEditor;

            invoker = new Invoker(TraceKey[context]);

            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher, Model.TraceMessages);

            traceMonitor.StartToMonitor();

            Loaded += (s, e) => { InitializeHistoryPanel(); };

        }
        #endregion

       

        #region Methods
        
        /// <summary>
        ///     Handles the OnSelected event of the HistoryListBox control.
        /// </summary>
        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            Model.TraceMessages.Add("History item clicked.");
            SetSelectedInvocationInfo((InvocationInfo) historyListBox.SelectedItem);
        }

        /// <summary>
        ///     Called when [execute clicked].
        /// </summary>
        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            if (Model.InvocationEditor.InvocationInfo == null || string.IsNullOrWhiteSpace(Model.InvocationEditor.InvocationInfo.MethodName) )
            {
                ((App)System.Windows.Application.Current).ErrorMonitor.ShowErrorNotification("Item should selected from history.");
                return;
            }

            Task.Run(() => history.SaveToHistory(Model.InvocationEditor.InvocationInfo));

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
        ///     Sets the selected invocation information.
        /// </summary>
        void SetSelectedInvocationInfo(InvocationInfo invocationInfo)
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

        /// <summary>
        ///     Updates the UI.
        /// </summary>
        void UpdateUI(Action action)
        {
            Dispatcher.InvokeAsync(action);
        }
        #endregion

        #region History
        /// <summary>
        ///     Histories the filter.
        /// </summary>
        bool HistoryFilter(object item)
        {
            if (string.IsNullOrEmpty(historyFilterTextBox.Text))
            {
                return true;
            }

            return ((InvocationInfo) item).ToString().IndexOf(historyFilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        ///     Initializes the history panel.
        /// </summary>
        void InitializeHistoryPanel()
        {
            Model.TraceMessages.Add("History is loading...");

            historyListBox.ItemsSource = history.GetHistory();

            var view = (CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource);

            view.Filter = HistoryFilter;

            Model.TraceMessages.Add("History is loaded.");
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the HistoryFilterTextBox control.
        /// </summary>
        void HistoryFilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(historyListBox.ItemsSource).Refresh();
        }
        #endregion

        void HistoryListBox_OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Delete)
            {
                if (historyListBox.SelectedItem != null)
                {
                    DeleteSelectedItemFromHistory((InvocationInfo)historyListBox.SelectedItem);
                }
            }
        }

        void DeleteSelectedItemFromHistory(InvocationInfo info)
        {
            history.Remove(info);

            InitializeHistoryPanel();
        }
    }
}