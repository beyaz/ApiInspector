using System;
using ApiInspector.History;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using BOA.DataFlow;

namespace ApiInspector.DataFlow
{
    /// <summary>
    ///     The data keys
    /// </summary>
    class DataKeys
    {
        #region Static Fields
        /// <summary>
        ///     The main window view model key
        /// </summary>
        public static DataKey<MainWindowViewModel> MainWindowViewModelKey = new DataKey<MainWindowViewModel>(nameof(MainWindowViewModel));

        /// <summary>
        ///     The selected invocation information key
        /// </summary>
        public static DataKey<InvocationInfo> SelectedInvocationInfoKey = new DataKey<InvocationInfo>(nameof(InvocationInfo));
        #endregion
    }

    /// <summary>
    ///     The service keys
    /// </summary>
    static class ServiceKeys
    {
        #region Static Fields
        /// <summary>
        ///     The history service key
        /// </summary>
        public static DataKey<DataSource> HistoryServiceKey = new DataKey<DataSource>(nameof(History));

        /// <summary>
        ///     The trace key
        /// </summary>
        public static DataKey<Action<string>> TraceKey = new DataKey<Action<string>>(nameof(TraceKey));
        #endregion
    }
}