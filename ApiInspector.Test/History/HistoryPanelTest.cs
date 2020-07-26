using System.Collections.Generic;
using ApiInspector.DataFlow;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ApiInspector.DataFlow.DataKeys;

namespace ApiInspector.History
{
    [TestClass]
    public class HistoryPanelTest
    {
        #region Public Methods
        [TestMethod]
        public void ExpectedBehaviour()
        {
            var context = new DataContextBuilder().Build();

            var historyPanel = new HistoryPanel {Context = context};

            // should load histories when refresh method called
            {
                historyPanel.historyListBox.ItemsSource.Should().BeNullOrEmpty();

                historyPanel.Refresh();

                historyPanel.historyListBox.ItemsSource.Should().NotBeNullOrEmpty();
            }

            // Should update selected invocation info when item clicked in list
            {
                context.Contains(SelectedInvocationInfoKey).Should().BeFalse();

                var items = (IList<InvocationInfo>) historyPanel.historyListBox.ItemsSource;

                historyPanel.historyListBox.SelectedValue = items[2];

                SelectedInvocationInfoKey[context].ClassName.Should().Be(items[2].ClassName);
            }
        }
        #endregion
    }
}