using System.Collections.Generic;
using ApiInspector.InvocationInfoEditor;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The main window view model
    /// </summary>
    class MainWindowViewModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the invocation editor.
        /// </summary>
        public InvocationEditorViewModel InvocationEditor { get; set; }

        /// <summary>
        ///     Gets or sets the trace messages.
        /// </summary>
        public List<string> TraceMessages { get; set; }
        #endregion
    }
}