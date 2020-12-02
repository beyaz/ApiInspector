using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ApiInspector.Application;
using ApiInspector.Models;
using static ApiInspector.History.HistoryPanelDatabaseRepository;
using static ApiInspector.Utility;

namespace ApiInspector.History
{
    /// <summary>
    ///     Interaction logic for HistoryPanel.xaml
    /// </summary>
    partial class HistoryPanel
    {
        #region Fields
        /// <summary>
        ///     The scope
        /// </summary>
        Scope scope;
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

        #region Public Methods
        /// <summary>
        ///     Connects the specified scope.
        /// </summary>
        public void Connect(Scope scope)
        {
            this.scope = scope;

            scope.OnUpdate(Keys.HistoryItems, OnHistoryItemsUpdated);
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
        ///     Traces the specified message.
        /// </summary>
        static void Trace(string message)
        {
            App.ApplicationScope.Get(Keys.UserVisibleTrace)(message);
        }

        /// <summary>
        ///     Deletes the selected item from History.
        /// </summary>
        void DeleteSelectedItemFromHistory(InvocationInfo info)
        {
            new Scope
            {
                {Keys.SelectedInvocationInfo, info}
            }.PublishEvent(HistoryEvent.RemoveSelectedInvocationInfo);

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

            scope.Update(Keys.SelectedInvocationInfo, (InvocationInfo) historyListBox.SelectedItem);
        }

        /// <summary>
        ///     Initializes the History panel.
        /// </summary>
        void InitializeHistoryPanel()
        {
            Trace("History is loading...");

            scope.Update(Keys.HistoryItems, TryRun(() => GetHistory(new Scope())) ?? new List<InvocationInfo>());

            Trace("History is loaded.");
        }

        /// <summary>
        ///     Called when [history items updated].
        /// </summary>
        void OnHistoryItemsUpdated()
        {
            historyListBox.ItemsSource = scope.Get(Keys.HistoryItems);

            var view = (CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource);

            view.Filter = HistoryFilter;
        }
        #endregion
    }
}