using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ApiInspector.History;
using ApiInspector.Models;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;
using static ApiInspector.DataFlow.ServiceKeys;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     Interaction logic for HistoryPanel.xaml
    /// </summary>
    public partial class HistoryPanel
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="HistoryPanel" /> class.
        /// </summary>
        public HistoryPanel()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     The Context
        /// </summary>
        public DataContext Context { get; set; }
        #endregion

        #region Properties
        /// <summary>
        ///     The History
        /// </summary>
        DataSource History => HistoryServiceKey[Context];
        #endregion

        #region Public Methods
        /// <summary>
        ///     Initializes the History panel.
        /// </summary>
        public void InitializeHistoryPanel()
        {
            Trace("History is loading...");

            historyListBox.ItemsSource = History.GetHistory();

            var view = (CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource);

            view.Filter = HistoryFilter;

            Trace("History is loaded.");
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Deletes the selected item from History.
        /// </summary>
        void DeleteSelectedItemFromHistory(InvocationInfo info)
        {
            History.Remove(info);

            InitializeHistoryPanel();
        }

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
        ///     Handles the OnTextChanged event of the HistoryFilterTextBox control.
        /// </summary>
        void HistoryFilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(historyListBox.ItemsSource).Refresh();
        }

        /// <summary>
        ///     Handles the OnKeyDown event of the HistoryListBox control.
        /// </summary>
        void HistoryListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (historyListBox.SelectedItem != null)
                {
                    DeleteSelectedItemFromHistory((InvocationInfo) historyListBox.SelectedItem);
                }
            }
        }

        /// <summary>
        ///     Handles the OnSelected event of the HistoryListBox control.
        /// </summary>
        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            Trace("History item clicked.");

            Context.Update(SelectedInvocationInfoKey, (InvocationInfo) historyListBox.SelectedItem);
        }

        void Trace(string message)
        {
            TraceKey[Context](message);
        }
        #endregion
    }
}