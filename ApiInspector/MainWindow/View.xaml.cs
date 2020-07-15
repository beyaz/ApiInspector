using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ApiInspector.Invoking;
using ApiInspector.Models;
using BOA.DataFlow;

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
        }
        #endregion

        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            context.Update(InvocationInfo,(InvocationInfo)historyListBox.SelectedItem);

        }

         void OnExecuteClicked(object sender, RoutedEventArgs e)
        {
            Invoker.Invoke(context);

            invokingResponseView.SetText(context.Get(Invoker.ExecutionResponse) + "");
        }

        
    }

    static class Extensions
    {
        public static string GetText(this RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart,
                                 richTextBox.Document.ContentEnd).Text;
        }

        public static void SetText(this RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
    }
}