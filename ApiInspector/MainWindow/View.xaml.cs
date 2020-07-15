using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ApiInspector.History;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.MainWindow
{
    public partial class View
    {
        #region Static Fields
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryDataKey = new DataKey<IReadOnlyList<InvocationInfo>>(nameof(HistoryDataKey));
        public static DataKey<InvocationInfo>                InvocationInfo = new DataKey<InvocationInfo>(nameof(InvocationInfo));
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
            HistoryManager.SaveToHistory(context.Get(InvocationInfo));

            invokingResponseView.SetText("invoke started...");

            Invoker.Invoke(context);

            invokingResponseView.SetText(context.Get(Invoker.ExecutionResponseAsJson));
        }
        #endregion
    }
}