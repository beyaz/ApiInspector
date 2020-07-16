using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using ApiInspector.CardSystemOldAndNewApiCall;
using ApiInspector.History;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.Base;
using BOA.DataFlow;
using BOA.Process.Kernel.Card;

namespace ApiInspector.MainWindow
{
    public partial class View
    {
        #region Static Fields
        public static DataKey<ObjectHelper>                  BOAExecutionContext = new DataKey<ObjectHelper>(nameof(BOAExecutionContext));
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryDataKey      = new DataKey<IReadOnlyList<InvocationInfo>>(nameof(HistoryDataKey));
        public static DataKey<InvocationInfo>                InvocationInfo      = new DataKey<InvocationInfo>(nameof(InvocationInfo));

        public static DataKey<Action<string>> Trace = new DataKey<Action<string>>(nameof(Trace));
        #endregion

        #region Fields
        DataContext context;
        #endregion

        #region Constructors
        public View()
        {
            InitializeComponent();

            InitializeContext();

            currentInvocationInfo.Context = context;

            context.Update(Trace,AppendTraceMessage);

            StartTimer();
        }
        #endregion

        #region Methods
        bool HistoryFilter(object item)
        {
            if (string.IsNullOrEmpty(historyFilterTextBox.Text))
            {
                return true;
            }

            return ((InvocationInfo) item).ToString().IndexOf(historyFilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void HistoryFilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(historyListBox.ItemsSource).Refresh();
        }

        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            context.Update(InvocationInfo, (InvocationInfo) historyListBox.SelectedItem);
            invokingResponseView.SetText(string.Empty);
        }

        void InitializeContext()
        {
            var builder = new ContextBuilder();

            context = builder.Build();

            historyListBox.ItemsSource = context.Get(HistoryDataKey);

            var view = (CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource);
            view.Filter = HistoryFilter;
        }

        void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            //Action action = this.OnExecuteClicked;

            new Thread(OnExecuteClicked).Start();
            //Dispatcher.BeginInvoke(DispatcherPriority.Background,action);

        }
        void OnExecuteClicked()
        {
            var trace = context.Get(Trace);

            var invocationInfo = context.Get(InvocationInfo);

            HistoryManager.SaveToHistory(invocationInfo);

            
            Dispatcher.InvokeAsync(() => { invokingResponseView.SetText(string.Empty); });

            if (Detection.CanInvokeAsCardSystemOldAndNewApiCall(context))
            {
                var objectHelper = context.Get(BOAExecutionContext);

                trace("------------- EXECUTE STARTED FOR OLD CARD SYSTEM -----------------");
                {
                    objectHelper.Context.DBLayer.BeginTransaction();

                    invocationInfo.MethodName = "ExecuteInOldCardSystem";

                    Invoker.Invoke(context);

                    objectHelper.Context.DBLayer.CommitTransaction();
                    context.Update(ExternalCodeCompareProgramStarter.OldCardSystemResult, context.Get(Invoker.ExecutionResponseAsJson));
                }

                trace("------------- EXECUTE STARTED FOR NEW CARD SYSTEM -----------------");
                {
                    objectHelper.Context.DBLayer.BeginTransaction();

                    invocationInfo.MethodName = "ExecuteInNewCardSystem";

                    CardService.UseLocalProxy = true;

                    Invoker.Invoke(context);

                    objectHelper.Context.DBLayer.CommitTransaction();

                    context.Update(ExternalCodeCompareProgramStarter.NewCardSystemResult, context.Get(Invoker.ExecutionResponseAsJson));
                }

                ExternalCodeCompareProgramStarter.Start(context);

                return;
            }

            trace("------------- EXECUTE STARTED -----------------");
            Invoker.Invoke(context);

            Dispatcher.InvokeAsync(() => { invokingResponseView.SetText(context.Get(Invoker.ExecutionResponseAsJson)); });

            trace(string.Empty);
            trace(string.Empty);
            trace(string.Empty);
        }
        #endregion

        void StartTimer()
        {
            var timer         =  new System.Timers.Timer(50);
            timer.Elapsed += OnTimedEvent;
            timer.Start();
        }

        void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
            foreach (var message in traceMessages)
            {
               
                    traceViewer.AppendText("\r" + message);
                    traceViewer.ScrollToEnd();
              
            }

            traceMessages.Clear();
            });
        }

        readonly List<string> traceMessages = new List<string>();

        void AppendTraceMessage(string message)
        {
            traceMessages.Add(message);

            //Dispatcher.Invoke(() =>
            //{
            //    traceViewer.AppendText("\r" + message);
            //    traceViewer.ScrollToEnd();
            //    Thread.Sleep(100);
            //});
        }
    }
}