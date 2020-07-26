using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ApiInspector.Models;

namespace ApiInspector.History
{
    /// <summary>
    ///     Interaction logic for HistoryPanel.xaml
    /// </summary>
    public partial class HistoryPanel
    {
        #region Fields
        /// <summary>
        ///     The History
        /// </summary>
        readonly DataSource History = new DataSource();

        /// <summary>
        ///     The trace
        /// </summary>
        Action<string> Trace;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="HistoryPanel" /> class.
        /// </summary>
        public HistoryPanel()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Events
        /// <summary>
        ///     Occurs when [selected invocation changed].
        /// </summary>
        public event Action SelectedInvocationChanged;
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the selected invocation information.
        /// </summary>
        public InvocationInfo SelectedInvocationInfo { get; private set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Connects the specified trace.
        /// </summary>
        public void Connect(Action<string> trace)
        {
            Trace = trace;
            Refresh();
        }

        /// <summary>
        ///     Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            InitializeHistoryPanel();
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Deletes the selected item from History.
        /// </summary>
        void DeleteSelectedItemFromHistory(InvocationInfo info)
        {
            History.Remove(info);

            Refresh();
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

            SelectedInvocationInfo = (InvocationInfo) historyListBox.SelectedItem;
            OnSelectedInvocationChanged();
        }

        /// <summary>
        ///     Initializes the History panel.
        /// </summary>
        void InitializeHistoryPanel()
        {
            Trace("History is loading...");

            historyListBox.ItemsSource = History.GetHistory();

            var view = (CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource);

            view.Filter = HistoryFilter;

            Trace("History is loaded.");
        }

        /// <summary>
        ///     Called when [selected invocation changed].
        /// </summary>
        void OnSelectedInvocationChanged()
        {
            SelectedInvocationChanged?.Invoke();
        }
        #endregion
    }
}