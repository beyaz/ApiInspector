using System.Collections.Generic;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.History
{
    /// <summary>
    ///     The history panel test
    /// </summary>
    [TestClass]
    public class HistoryPanelTest
    {
        #region Public Methods
        /// <summary>
        ///     Expecteds the behaviour.
        /// </summary>
        [TestMethod]
        public void ExpectedBehaviour()
        {
            var historyPanel = new HistoryPanel();

            // should load histories when connected
            {
                historyPanel.historyListBox.ItemsSource.Should().BeNullOrEmpty();

                historyPanel.Connect(message => { });

                historyPanel.historyListBox.ItemsSource.Should().NotBeNullOrEmpty();
            }

            // Should update selected invocation info when item clicked in list
            {
                historyPanel.SelectedInvocationInfo.Should().BeNull();

                var items = (IList<InvocationInfo>) historyPanel.historyListBox.ItemsSource;

                historyPanel.historyListBox.SelectedValue = items[2];

                historyPanel.SelectedInvocationInfo.ClassName.Should().Be(items[2].ClassName);
            }
        }
        #endregion
    }
}