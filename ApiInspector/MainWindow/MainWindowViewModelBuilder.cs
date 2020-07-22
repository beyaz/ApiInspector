using System.Collections.Generic;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.Models;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The main window view model builder
    /// </summary>
    class MainWindowViewModelBuilder
    {
        #region Public Methods
        /// <summary>
        ///     Builds this instance.
        /// </summary>
        public MainWindowViewModel Build()
        {
            var traceMessages = new List<string>();

            return new MainWindowViewModel
            {
                InvocationEditor = new InvocationEditorViewModel
                {
                    InvocationInfo = new InvocationInfo
                    {
                        AssemblySearchDirectory = AssemblySearchDirectories.serverBin
                    },
                    ItemSourceList = new ItemSourceList
                    {
                        AssemblySearchDirectoryList = new List<string> {AssemblySearchDirectories.serverBin, AssemblySearchDirectories.clientBin},
                        EnvironmentNameList         = new List<string> {"dev", "test"}
                    },
                    Logs = traceMessages
                },
                TraceMessages = traceMessages
            };
        }
        #endregion
    }
}