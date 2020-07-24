using System.Collections.Generic;
using ApiInspector.InvocationInfoEditor;
using ApiInspector.MainWindow;
using ApiInspector.Models;
using BOA.DataFlow;
using static ApiInspector.DataFlow.DataKeys;

namespace ApiInspector.DataFlow
{
    class DataContextBuilder
    {
        public DataContext Build()
        {
            var context = new DataContext();

            var traceMessages = new List<string>();

            MainWindowViewModelKey[context] = new MainWindowViewModel
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

            TraceKey[context] = traceMessages.Add;
            
            
            return context;
        }
    }
}