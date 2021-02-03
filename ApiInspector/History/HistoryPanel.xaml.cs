using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ApiInspector.Models;
using static ApiInspector._;
using static ApiInspector.History.DataKeys;
using static ApiInspector.History.HistoryPanelDatabaseRepository;
using static ApiInspector.Keys;
using static ApiInspector.MainWindow.Mixin;
using static FunctionalPrograming.FPExtensions;

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

        #region Properties
        bool HasAlreadyHistories => scope.Contains(HistoryItems);
        #endregion

        #region Public Methods
        /// <summary>
        ///     Connects the specified scope.
        /// </summary>
        public void Connect(Scope scope)
        {
            this.scope = scope;

            scope.SubscribeEvent(HistoryEvent.RemoveSelectedInvocationInfo, () => Remove(scope));
            scope.SubscribeEvent(HistoryEvent.SaveToHistory, () => SaveToHistory(scope));

            scope.OnUpdate(HistoryItems, fun(() =>
            {
                var items = scope.Get(HistoryItems);

                void initializeItemsSource()
                {
                    historyListBox.ItemsSource = items;
                }

                void trySelectFirstItem()
                {
                    historyListBox.SelectedItem = items.FirstOrDefault();
                }

                void reAssignCollectionFilter()
                {
                    ((CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource)).Filter = HistoryFilter;
                }

                initializeItemsSource();
                trySelectFirstItem();
                reAssignCollectionFilter();
            }));

            scope.OnUpdate(SearchTextKey, InitializeHistoryPanel);
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
        ///     Histories the filter.
        /// </summary>
        bool HistoryFilter(object item)
        {
            if (string.IsNullOrEmpty(historyFilterTextBox.Text))
            {
                return true;
            }

            var invocationInfo = (InvocationInfo) item;

            bool isMatch(string value)
            {
                if (value == null)
                {
                    return false;
                }

                return value.IndexOf(historyFilterTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (isMatch(invocationInfo.ToString()))
            {
                return true;
            }

            foreach (var scenarioInfo in invocationInfo.Scenarios)
            {
                foreach (var methodParameterInfo in scenarioInfo.MethodParameters)
                {
                    if (isMatch(methodParameterInfo.Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the HistoryFilterTextBox control.
        /// </summary>
        void HistoryFilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            scope.Update(SearchTextKey, historyFilterTextBox.Text);

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
                    scope.PublishEvent(HistoryEvent.RemoveSelectedInvocationInfo);

                    Refresh();
                }
            }
        }

        /// <summary>
        ///     Handles the OnSelected event of the HistoryListBox control.
        /// </summary>
        void HistoryListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            UserVisibleTrace("History item clicked.");

            scope.Update(SelectedInvocationInfo, (InvocationInfo) historyListBox.SelectedItem);
        }

        /// <summary>
        ///     Initializes the History panel.
        /// </summary>
        void InitializeHistoryPanel()
        {
            Dispatcher.InvokeAsync(() =>
            {
                UserVisibleTrace("History is loading...");

                var shouldContainsMinimumOneItem = fun((IReadOnlyList<InvocationInfo> items) =>
                {
                    if (items.Count > 0)
                    {
                        return items;
                    }

                    return new List<InvocationInfo> {CreateNewInvocationInfo()};
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

                var dbRecords = shouldContainsMinimumOneItem(getItems()).ToList();
                if (HasAlreadyHistories)
                {
                    var selectedItem = (InvocationInfo) historyListBox.SelectedItem;
                    if (selectedItem != null)
                    {
                        dbRecords.RemoveAll(x => InvocationInfo.IsSame(x, selectedItem));
                        dbRecords.Add(selectedItem);
                    }

                    historyListBox.ItemsSource = dbRecords;

                    CollectionViewSource.GetDefaultView(historyListBox.ItemsSource).Refresh();

                    UserVisibleTrace($"History is refreshed. Fetched record count is {dbRecords.Count}");

                    return;
                }

                scope.Update(HistoryItems, dbRecords);

                UserVisibleTrace("History is loaded.");
            });
        }
        #endregion
    }
}