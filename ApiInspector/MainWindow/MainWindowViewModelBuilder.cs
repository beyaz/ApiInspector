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
            const string defaultAssemblySearchDirectory = @"d:\boa\server\bin\";

            var traceMessages = new List<string>();

            return new MainWindowViewModel
            {
                InvocationEditor = new InvocationEditorViewModel
                {
                    InvocationInfo = new InvocationInfo
                    {
                        AssemblySearchDirectory = defaultAssemblySearchDirectory
                    },
                    ItemSourceList = new ItemSourceList
                    {
                        AssemblySearchDirectoryList = new List<string> {defaultAssemblySearchDirectory},
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