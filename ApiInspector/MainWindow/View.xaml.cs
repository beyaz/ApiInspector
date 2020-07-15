using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ApiInspector.History;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.DataFlow;
using Newtonsoft.Json;

namespace ApiInspector.MainWindow
{
    public partial class View
    {
        public static  DataKey<InvocationInfo> InvocationInfo = new DataKey<InvocationInfo>(nameof(InvocationInfo));
        public static DataKey<IReadOnlyList<InvocationInfo>> HistoryDataKey = new DataKey<IReadOnlyList<InvocationInfo>>(nameof(HistoryDataKey));

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
        

        void InitializeContext()
        {
            var builder = new ContextBuilder();

            context = builder.Build();

            historyListBox.ItemsSource = context.Get(HistoryDataKey);

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(historyListBox.ItemsSource);
            view.Filter = HistoryFilter;
        }
        #endregion

        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            context.Update(InvocationInfo,(InvocationInfo)historyListBox.SelectedItem);

        }

         void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            invokingResponseView.SetText("invoke started...");

            Invoker.Invoke(context);

            var response = context.Get(Invoker.ExecutionResponse);
            
            invokingResponseView.SetText(ResultSerializer.SerializeToJson(response));

            HistoryManager.SaveToHistory(context.Get(InvocationInfo));
        }

         void HistoryFilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
         {
             CollectionViewSource.GetDefaultView(historyListBox.ItemsSource).Refresh();
         }

         private bool HistoryFilter(object item)
         {
             if(String.IsNullOrEmpty(historyFilterTextBox.Text))
                 return true;
             
             
             return ((InvocationInfo)item) .ToString().IndexOf(historyFilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
         }
    }

    class ResultSerializer
    {
       public static string SerializeToJson(object value, bool ignoreDefaultValues = true)
        {
            if (value == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = ignoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                Formatting           = Formatting.Indented,
                DateFormatString     = "yyyy.MM.dd hh:mm:ss"
            };

            return JsonConvert.SerializeObject(value, settings);
        }
    }
}