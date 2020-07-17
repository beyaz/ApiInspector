using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ApiInspector.History;
using ApiInspector.Invoking;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    public partial class View
    {
        readonly DataSource history = new DataSource();

        readonly MainWindowViewModel model = new MainWindowViewModelBuilder().Build();

        readonly Invoker invoker;

        #region Constructors
        public View()
        {
            InitializeComponent();

            currentInvocationInfo.Model = model.InvocationEditor;

             invoker = new Invoker(model.TraceMessages.Add);

            var traceMonitor = new TraceMonitor(traceViewer, Dispatcher, model.TraceMessages);

            traceMonitor.StartToMonitor();

            InitializeHistoryPanel();
        }
        #endregion

        #region Public Methods
        public void RefreshValues()
        {
            responseOutputFilePath.Text = model.InvocationEditor.InvocationInfo.ResponseOutputFilePath;
        }
        #endregion

        #region Methods
        void AppendTraceMessage(string message)
        {
            model.TraceMessages.Add(message);
        }

       

        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            model.TraceMessages.Add("History item clicked.");
            SetSelectedInvocationInfo((InvocationInfo) historyListBox.SelectedItem);
        }

        void SetSelectedInvocationInfo(InvocationInfo invocationInfo)
        {
            model.InvocationEditor.InvocationInfo = invocationInfo;
            
            invokingResponseView.SetText(string.Empty);

            currentInvocationInfo.OnInvocationInfoChanged();

            model.TraceMessages.Add("Selected invocation was changed.");
        }



        

        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            new Thread(OnExecuteClicked).Start();
        }

        void OnExecuteClicked()
        {
            Action<string> trace = model.TraceMessages.Add;

            var invocationInfo = model.InvocationEditor.InvocationInfo;

            history.SaveToHistory(invocationInfo);
          

            Dispatcher.InvokeAsync(() => { invokingResponseView.SetText(string.Empty); });

            trace("------------- EXECUTE STARTED -----------------");

            

            var invokerOutput = invoker.Invoke(model.InvocationEditor.InvocationInfo);

            Dispatcher.InvokeAsync(() => { invokingResponseView.SetText(invokerOutput.ExecutionResponseAsJson); });

            TryToExportExecutionResponseToFile(invokerOutput.ExecutionResponseAsJson);

            trace(string.Empty);
            trace(string.Empty);
        }

       

        void ResponseOutputFilePath_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            model.InvocationEditor.InvocationInfo.ResponseOutputFilePath = responseOutputFilePath.Text;
        }

      

        void TryToExportExecutionResponseToFile(string executionResponseAsJson)
        {
            var outputFilePath = model.InvocationEditor.InvocationInfo.ResponseOutputFilePath;
            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                return;
            }

            Utility.WriteAllText(outputFilePath, executionResponseAsJson);
        }
        #endregion


        #region History
        bool HistoryFilter(object item)
        {
            if (string.IsNullOrEmpty(historyFilterTextBox.Text))
            {
                return true;
            }

            return ((InvocationInfo) item).ToString().IndexOf(historyFilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        void InitializeHistoryPanel()
        {
            model.TraceMessages.Add("History is loading...");

            historyListBox.ItemsSource = history.GetHistory();

            var view = (CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource);

            view.Filter = HistoryFilter;

            model.TraceMessages.Add("History is loaded.");
        }

        void HistoryFilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(historyListBox.ItemsSource).Refresh();
        }
        #endregion
    }
}