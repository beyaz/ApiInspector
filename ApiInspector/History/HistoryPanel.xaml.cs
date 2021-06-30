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

     

        #region Public Methods
        /// <summary>
        ///     Connects the specified scope.
        /// </summary>
        public void Connect(Scope scope)
        {
            this.scope = scope;

            scope.SubscribeEvent(HistoryEvent.RemoveSelectedInvocationInfo, () => Remove(scope));
            scope.SubscribeEvent(HistoryEvent.SaveToHistory, () => SaveToHistory(scope));

            scope.OnUpdate(HistoryItems, () => OnHistoryItemsUpdated(scope.Get(HistoryItems)));

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
        static bool HistoryFilter(object item, string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
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

                return value.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
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
                
                var dbRecords = SafeRun(() => GetHistory(scope))
                                .IfNull(() => new List<InvocationInfo>())
                                .AddNewOneItemIfListIsEmpty(CreateNewInvocationInfo)
                                .ToList();
                
                var hasAlreadyHistories = scope.Contains(HistoryItems);
                if (hasAlreadyHistories)
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

        void OnHistoryItemsUpdated(IReadOnlyList<InvocationInfo> items)
        {
            // update source
            historyListBox.ItemsSource = items;

            // trySelectFirstItem
            historyListBox.SelectedItem = items.FirstOrDefault();

            // reAssignCollectionFilter
            ((CollectionView) CollectionViewSource.GetDefaultView(historyListBox.ItemsSource)).Filter = x => HistoryFilter(x, historyFilterTextBox.Text);
        }
        #endregion
    }
}