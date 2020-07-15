using System.Windows;
using ApiInspector.DataAccess;
using ApiInspector.DataFlow;
using ApiInspector.History;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.MainWindow
{
    public partial class View
    {
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

            historyListBox.ItemsSource = HistoryManager.GetHistory(context);
        }
        #endregion

        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            context.Update(Data.InvocationInfo,(InvocationInfo)historyListBox.SelectedItem);

        }
    }
}