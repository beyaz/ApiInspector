using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ApiInspector.Application;
using ApiInspector.Models;
using static ApiInspector.History.HistoryPanelDatabaseRepository;
using static ApiInspector.Keys;
using static FunctionalPrograming.Extensions;

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

            scope.OnUpdate(HistoryItems, fun(() =>
            {
                var getItems = fun(() => scope.Get(HistoryItems));

                var initializeItemsSource = fun(() => { historyListBox.ItemsSource = getItems(); });

                var trySelectFirstItem = fun(() => { historyListBox.SelectedItem = getItems().FirstOrDefault(); });

                var reAssignCollectionFilter = fun(() => { ((CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource)).Filter = HistoryFilter; });

                initializeItemsSource();
                trySelectFirstItem();
                reAssignCollectionFilter();
            }));
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
            App.ApplicationScope.Get(UserVisibleTrace)(message);
        }

        /// <summary>
        ///     Deletes the selected item from History.
        /// </summary>
        void DeleteSelectedItemFromHistory(InvocationInfo info)
        {
            new Scope
            {
                {SelectedInvocationInfo, info}
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

            scope.Update(SelectedInvocationInfo, (InvocationInfo) historyListBox.SelectedItem);
        }

        /// <summary>
        ///     Initializes the History panel.
        /// </summary>
        void InitializeHistoryPanel()
        {
            Dispatcher.InvokeAsync(() =>
            {
                Trace("History is loading...");

                var shouldContainsMinimumOneItem = fun((IReadOnlyList<InvocationInfo> items) =>
                {
                    if (items.Count > 0)
                    {
                        return items;
                    }

                    return new List<InvocationInfo> {new InvocationInfo()};
                });

                var getItems = fun(() =>
                {
                    var response = Run(() => GetHistory(scope));
                    if (response.IsSuccess)
                    {
                        return response.Value;
                    }

                    return new List<InvocationInfo>();
                });

                scope.Update(HistoryItems, shouldContainsMinimumOneItem(getItems()));

                Trace("History is loaded.");
            });
        }
        #endregion
    }
}