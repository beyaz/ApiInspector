using System;
using ApiInspector.MainWindow;
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
        ///     The trace key
        /// </summary>
        public static DataKey<Action<string>> TraceKey = new DataKey<Action<string>>(nameof(TraceKey));
        #endregion
    }
}